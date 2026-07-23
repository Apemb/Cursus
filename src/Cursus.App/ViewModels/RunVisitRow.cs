using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Workflows.Projection;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une visite dans la trajectoire déroulée : une étape, son tour de boucle, et
/// son issue. Un tour de boucle engendre une ligne de plus (l'itération la
/// distingue), jamais un remplacement — la maquette validée déroule la traversée
/// plutôt que de replier les boucles. Non testé, comme toute la vue (§7.12).
/// </summary>
public partial class RunVisitRow : ObservableObject
{
    public RunVisitRow(string stepId, int iteration)
    {
        StepId = stepId;
        Iteration = iteration;
    }

    public string StepId { get; }

    public int Iteration { get; }

    /// <summary>Le libellé : l'étape, et le tour quand c'est une visite de boucle au-delà du premier.</summary>
    public string Label => Iteration > 1 ? $"{StepId} · tour {Iteration}" : StepId;

    /// <summary>L'état de la visite — pilote le glyphe et sa couleur ; <c>Running</c> tant qu'elle n'est pas close.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Glyph))]
    [NotifyPropertyChangedFor(nameof(StatusBrush))]
    private RunVisitStatus _status = RunVisitStatus.Running;

    /// <summary>Le repère d'un coup d'œil : en cours, réussi, échoué.</summary>
    public string Glyph => Status switch
    {
        RunVisitStatus.Succeeded => "✓",
        RunVisitStatus.Failed => "✗",
        _ => "▸",
    };

    /// <summary>La couleur du glyphe — sémantique (bon/mauvais/en cours), distincte de l'accent de la coquille.</summary>
    public IBrush StatusBrush => Status switch
    {
        RunVisitStatus.Succeeded => SucceededBrush,
        RunVisitStatus.Failed => FailedBrush,
        _ => RunningBrush,
    };

    /// <summary>Recale l'état à partir de la visite projetée — en cours, ou close sur son issue.</summary>
    public void SyncWith(RunVisit visit) =>
        Status = visit.IsRunning
            ? RunVisitStatus.Running
            : visit.Result!.IsSuccess ? RunVisitStatus.Succeeded : RunVisitStatus.Failed;

    // Couleurs sémantiques, séparées de l'accent chrome de la coquille (§9.5). Le
    // bleu « en cours » reprend le bleu système macOS ; le vert et le rouge disent
    // l'issue sans ambiguïté.
    private static readonly IBrush RunningBrush = new SolidColorBrush(Color.Parse("#0A84FF"));
    private static readonly IBrush SucceededBrush = new SolidColorBrush(Color.Parse("#2F9E44"));
    private static readonly IBrush FailedBrush = new SolidColorBrush(Color.Parse("#C0392B"));
}

/// <summary>L'issue d'une visite, telle que la lit l'œil : en cours, réussie, échouée.</summary>
public enum RunVisitStatus
{
    Running,
    Succeeded,
    Failed,
}
