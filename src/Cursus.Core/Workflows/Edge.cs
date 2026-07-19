namespace Cursus.Core.Workflows;

/// <summary>
/// Une arête sortante du graphe : si sa <see cref="Guard"/> matche le résultat
/// de l'étape, le moteur route vers l'étape <see cref="Target"/>.
/// </summary>
public sealed record Edge(Guard Guard, string Target);
