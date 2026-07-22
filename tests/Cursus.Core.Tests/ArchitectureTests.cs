using Cursus.Core.Projects;

namespace Cursus.Core.Tests;

/// <summary>
/// Les invariants d'architecture rendus exécutables (presentation.md §7). Le
/// critère central : l'UI n'est qu'un driver ; toute la logique doit rester
/// atteignable sans Avalonia. Aucune relecture de code ne le garantit — ce test,
/// si.
/// </summary>
public sealed class ArchitectureTests
{
    [Fact(DisplayName = "étant donné l'assembly du noyau, quand on inspecte ses références, alors aucune ne pointe vers Avalonia")]
    public void The_core_depends_on_no_avalonia_assembly()
    {
        // arrange — un type quelconque du noyau désigne son assembly
        var core = typeof(Project).Assembly;

        // act
        var referenced = core.GetReferencedAssemblies();

        // assert — le jour où une dépendance UI se glisse dans Core (même par
        // mégarde transitive employée), ce test tombe
        Assert.DoesNotContain(
            referenced,
            assembly => assembly.Name!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }
}
