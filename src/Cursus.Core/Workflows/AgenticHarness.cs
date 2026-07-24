namespace Cursus.Core.Workflows;

/// <summary>
/// Un harness agentique : l'outil qui héberge un agent — nommé (« Claude Code ») et
/// portant la liste des modèles qu'il offre. Concept <b>nommé</b> plutôt qu'un enum en
/// dur sur Claude : une étape-agent le référence par son nom, la validation et l'UI
/// lisent ses modèles ici. Une seule instance connue aujourd'hui (<see cref="ClaudeCode"/>) ;
/// l'<b>invocation</b> réelle d'un modèle (« claude --model … ») vit dans l'exécuteur, pas
/// ici — ce type n'est que de la donnée déclarée.
/// </summary>
public sealed record AgenticHarness(string Name, IReadOnlyList<AgentModel> Models)
{
    /// <summary>Le harness Claude Code et sa lignée de modèles.</summary>
    public static readonly AgenticHarness ClaudeCode = new(
        "Claude Code",
        [
            new AgentModel("opus", "Opus"),
            new AgentModel("sonnet", "Sonnet"),
            new AgentModel("haiku", "Haiku"),
            new AgentModel("fable", "Fable"),
        ]);

    /// <summary>Les harness que Cursus connaît. Un seul pour l'instant.</summary>
    public static readonly IReadOnlyList<AgenticHarness> Known = [ClaudeCode];

    /// <summary>Le harness de ce nom, ou <c>null</c> s'il est inconnu.</summary>
    public static AgenticHarness? ByName(string name) => Known.FirstOrDefault(h => h.Name == name);

    /// <summary>Vrai si ce harness offre un modèle de cet identifiant.</summary>
    public bool HasModel(string modelId) => Models.Any(m => m.Id == modelId);
}
