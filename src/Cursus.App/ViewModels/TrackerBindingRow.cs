using System;

using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une déclaration de tracker telle qu'elle se lit à l'écran — le pendant de
/// <see cref="TrackerConnectionRow"/>, et polymorphe pour la même raison : ce que le
/// dépôt déclare viser dépend du tracker visé.
///
/// <para>
/// Elle vit ici et non en Core parce que c'est du <b>libellé</b> : la formulation
/// française destinée à l'écran n'a pas à descendre dans le modèle, qui n'a pas d'écran.
/// Le prix est une discrimination du genre concret, admise pour l'affichage seul — la
/// <em>résolution</em>, elle, reste sans discrimination aucune, portée par
/// <see cref="TrackerBinding.Matches"/> et <see cref="TrackerConnection.ToBinding"/>.
/// </para>
/// </summary>
public abstract class TrackerBindingRow(TrackerBinding binding)
{
    public TrackerBinding Binding { get; } = binding;

    /// <summary>Le tableau visé, en clair — ce qui doit figurer dans le constat d'une divergence.</summary>
    public abstract string Label { get; }

    /// <summary>La ligne qui convient à cette déclaration ; ajouter un tracker se voit à un seul endroit.</summary>
    public static TrackerBindingRow For(TrackerBinding binding) => binding switch
    {
        LinearBinding linear => new LinearBindingRow(linear),
        _ => throw new NotSupportedException(
            $"Aucun libellé d'affichage pour {binding.GetType().Name}."),
    };
}

/// <summary>Un dépôt qui suit un espace Linear : « l'espace Linear « cursus-app » ».</summary>
public sealed class LinearBindingRow(LinearBinding binding) : TrackerBindingRow(binding)
{
    public override string Label => $"l'espace Linear « {binding.WorkspaceKey} »";
}
