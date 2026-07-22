
using Cursus.Core.Workflows;

using Microsoft.Data.Sqlite;

namespace Cursus.Persistence;

/// <summary>
/// Journal durable sur SQLite. <c>run_events</c> est la source ; <c>runs</c>
/// n'en est qu'une projection, entretenue à l'écriture pour qu'une liste ne
/// coûte pas un rejeu complet du journal.
/// </summary>
public sealed class SqliteRunJournal : IRunJournal, IRunJournalReader, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IClock _clock;

    // La connexion est unique et non thread-safe : deux runs concurrents sur le
    // même projet écriraient sur elle en même temps. Le verrou les sérialise —
    // négligeable devant un lancement de process, et la seule façon correcte ici,
    // le pool de Microsoft.Data.Sqlite n'offrant pas de plafond à une connexion.
    private readonly Lock _writeLock = new();

    public SqliteRunJournal(string databasePath, IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(databasePath))!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString());
        _connection.Open();

        CreateSchema();
    }

    public void Append(string runId, WorkflowEvent @event)
    {
        var at = _clock.UtcNow;

        lock (_writeLock)
        {
            // Une transaction par événement : négligeable devant un lancement de
            // process, et un crash laisse alors un journal exploitable jusqu'au
            // dernier instant plutôt qu'un tampon perdu.
            using var transaction = _connection.BeginTransaction();

            Project(runId, at, @event, transaction);
            InsertEvent(runId, at, @event, transaction);

            transaction.Commit();
        }
    }

    public IReadOnlyList<RunSummary> ListRuns()
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
            "SELECT id, started_at, state, abort_reason FROM runs ORDER BY started_at DESC, id DESC;";

        var runs = new List<RunSummary>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            runs.Add(new RunSummary(
                reader.GetString(0),
                ReadInstant(reader, 1),
                reader.IsDBNull(2) ? null : Enum.Parse<RunState>(reader.GetString(2)),
                reader.IsDBNull(3) ? null : Enum.Parse<AbortReason>(reader.GetString(3))));
        }

        return runs;
    }

    public IReadOnlyList<JournalEntry> ReadEvents(string runId)
    {
        // La définition n'est lue qu'à la demande : un seul événement par run
        // la porte, et la reconstruire coûte un passage de sérialiseur.
        var definition = new Lazy<WorkflowDefinition>(() => ReadDefinition(runId));

        using var command = _connection.CreateCommand();
        command.CommandText =
            "SELECT seq, at, kind, payload FROM run_events WHERE run_id = $runId ORDER BY seq;";
        command.Parameters.AddWithValue("$runId", runId);

        var entries = new List<JournalEntry>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var @event = RunEventCodec.Decode(reader.GetString(2), reader.GetString(3), () => definition.Value);
            entries.Add(new JournalEntry(runId, reader.GetInt64(0), ReadInstant(reader, 1), @event));
        }

        return entries;
    }

    private WorkflowDefinition ReadDefinition(string runId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT definition_json FROM runs WHERE id = $id;";
        command.Parameters.AddWithValue("$id", runId);

        var json = (string?)command.ExecuteScalar()
            ?? throw new InvalidOperationException($"Aucun run journalisé sous l'identifiant {runId}.");

        var loaded = WorkflowSerializer.Read(json);
        return loaded.Definition
            ?? throw new InvalidOperationException(
                $"La définition figée du run {runId} n'est plus relisible : {loaded.Report}");
    }

    public void Dispose() => _connection.Dispose();

    // --- écriture ---

    /// <summary>Entretient la ligne de <c>runs</c> que cet événement fait bouger.</summary>
    private void Project(string runId, DateTimeOffset at, WorkflowEvent @event, SqliteTransaction transaction)
    {
        switch (@event)
        {
            case WorkflowEvent.RunStarted started:
                Execute(transaction,
                    """
                    INSERT INTO runs (id, definition_json, workspace_root, trigger_kind, trigger_task_key, started_at)
                    VALUES ($id, $definition, $root, $triggerKind, $taskKey, $at);
                    """,
                    ("$id", runId),
                    ("$definition", WorkflowSerializer.Write(started.Definition)),
                    ("$root", started.WorkspaceRoot),
                    ("$triggerKind", started.Trigger.Kind.ToString()),
                    ("$taskKey", started.Trigger.TaskKey),
                    ("$at", Format(at)));
                break;

            case WorkflowEvent.RunFinished finished:
                Execute(transaction,
                    "UPDATE runs SET ended_at = $at, state = $state, abort_reason = $reason WHERE id = $id;",
                    ("$id", runId),
                    ("$at", Format(at)),
                    ("$state", finished.State.ToString()),
                    ("$reason", finished.AbortReason?.ToString()));
                break;
        }
    }

    private void InsertEvent(string runId, DateTimeOffset at, WorkflowEvent @event, SqliteTransaction transaction)
    {
        var seq = NextSeq(runId, transaction);
        var (stepId, iteration) = Anchor(@event);

        Execute(transaction,
            """
            INSERT INTO run_events (run_id, seq, at, kind, step_id, iteration, payload)
            VALUES ($runId, $seq, $at, $kind, $stepId, $iteration, $payload);
            """,
            ("$runId", runId),
            ("$seq", seq),
            ("$at", Format(at)),
            ("$kind", RunEventCodec.KindOf(@event)),
            ("$stepId", stepId),
            ("$iteration", iteration),
            ("$payload", RunEventCodec.Encode(@event)));
    }

    private long NextSeq(string runId, SqliteTransaction transaction)
    {
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COALESCE(MAX(seq), 0) + 1 FROM run_events WHERE run_id = $runId;";
        command.Parameters.AddWithValue("$runId", runId);
        return (long)command.ExecuteScalar()!;
    }

    /// <summary>
    /// L'étape et l'itération auxquelles un événement se rattache, quand il s'en
    /// rattache une — promues en colonnes pour être filtrables sans ouvrir le JSON.
    /// </summary>
    private static (string? StepId, int? Iteration) Anchor(WorkflowEvent @event) => @event switch
    {
        WorkflowEvent.StepStarted started => (started.StepId, started.Iteration),
        WorkflowEvent.StepFinished finished => (finished.StepId, finished.Iteration),
        WorkflowEvent.EdgeChosen chosen => (chosen.FromStepId, null),
        _ => (null, null),
    };

    // --- schéma et plomberie ---

    private void CreateSchema()
    {
        // WAL : l'interface doit pouvoir relire un run pendant qu'il s'écrit.
        Execute(transaction: null, "PRAGMA journal_mode=WAL;");

        Execute(transaction: null,
            """
            CREATE TABLE IF NOT EXISTS runs (
                id               TEXT PRIMARY KEY,
                definition_json  TEXT NOT NULL,
                workspace_root   TEXT NOT NULL,
                trigger_kind     TEXT NOT NULL,
                trigger_task_key TEXT,
                started_at       TEXT NOT NULL,
                ended_at         TEXT,
                state            TEXT,
                abort_reason     TEXT
            );

            CREATE TABLE IF NOT EXISTS run_events (
                run_id    TEXT NOT NULL REFERENCES runs(id),
                seq       INTEGER NOT NULL,
                at        TEXT NOT NULL,
                kind      TEXT NOT NULL,
                step_id   TEXT,
                iteration INTEGER,
                payload   TEXT NOT NULL,
                PRIMARY KEY (run_id, seq)
            );
            """);
    }

    private void Execute(SqliteTransaction? transaction, string sql, params (string Name, object? Value)[] parameters)
    {
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    // ISO-8601 en UTC : trié lexicographiquement, il l'est aussi
    // chronologiquement — mais c'est `seq` qui fait foi sur l'ordre.
    private static string Format(DateTimeOffset instant) => instant.UtcDateTime.ToString("O");

    private static DateTimeOffset ReadInstant(SqliteDataReader reader, int ordinal) =>
        new(DateTime.Parse(reader.GetString(ordinal), null, System.Globalization.DateTimeStyles.RoundtripKind),
            TimeSpan.Zero);
}
