using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace ActionTranslate;

internal class Service
{
    /// <summary>
    /// Gets the Dalamud plugin interface.
    /// </summary>
    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud command manager.
    /// </summary>
    [PluginService]
    internal static CommandManager CommandManager { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud sigscanner.
    /// </summary>
    [PluginService]
    internal static SigScanner sigScanner { get; private set; } = null;
}