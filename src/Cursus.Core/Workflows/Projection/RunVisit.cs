namespace Cursus.Core.Workflows.Projection;

/// <summary>
/// Une visite d'étape dans la trajectoire d'un run. Une étape en boucle en
/// produit plusieurs, que <see cref="Iteration"/> distingue — c'est ce qui fait
/// de la trajectoire une <b>liste</b> et non un graphe (parcours §1.4). Tant que
/// <see cref="Result"/> est absent, la visite <b>tourne</b> ; il apparaît quand
/// elle s'achève.
/// </summary>
public sealed record RunVisit(string StepId, int Iteration, ScriptResult? Result = null)
{
    /// <summary>Vrai tant que la visite n'a pas rendu son résultat.</summary>
    public bool IsRunning => Result is null;
}
