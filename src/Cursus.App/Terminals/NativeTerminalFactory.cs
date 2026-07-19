using RoyalTerminal.Avalonia.Controls;
using RoyalTerminal.Avalonia.Services;
using RoyalTerminal.Terminal;
using RoyalTerminal.Terminal.Services;

namespace Cursus.App.Terminals;

/// <summary>
/// Construit un <see cref="TerminalControl"/> adossé au moteur VT natif
/// (libghostty-vt). Indispensable au suivi correct des modes VT — dont
/// « application cursor keys » (DECCKM) — sans lequel les touches spéciales
/// (flèches) ne sont pas encodées comme les TUI l'attendent.
///
/// Le constructeur sans paramètre du contrôle n'enregistre aucun provider
/// natif (sa <c>DefaultVtProcessorFactory</c> est vide) et reste donc en
/// moteur managé ; on recompose ici la factory avec le provider Ghostty.
/// La préférence par défaut (<c>Auto</c>) essaie le natif puis retombe
/// proprement sur le managé s'il est indisponible.
/// </summary>
public static class NativeTerminalFactory
{
    public static TerminalControl Create()
    {
        var vtFactory = new DefaultVtProcessorFactory(
            new INativeVtProcessorProvider[] { new GhosttyVtProcessorProvider() });

        return new TerminalControl(
            new TerminalSessionService(),
            new DefaultTerminalInputAdapter(),
            new DefaultTerminalSelectionService(),
            new DefaultTerminalScrollService(),
            vtFactory,
            new DefaultPtyFactory());
    }
}
