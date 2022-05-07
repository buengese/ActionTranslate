using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace ActionTranslate;

public class ActionTranslate : IDalamudPlugin
{
    public string Name => "ActionTranslate";

    private const string CommandName = "/actiontranslate";
    
    private delegate IntPtr GetResourceSyncPrototype(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr pPath, IntPtr a6);
    private delegate IntPtr GetResourceAsyncPrototype(IntPtr manager, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr pPath, IntPtr a6, byte a7);
    private readonly Hook<GetResourceSyncPrototype> _getResourceSyncHook;
    private readonly Hook<GetResourceAsyncPrototype> _getResourceAsyncHook;

    public ActionTranslate(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle ActionTranslate",
        });


        var getResourceAsync = Service.sigScanner.ScanText("E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00");
        var getResourceSync = Service.sigScanner.ScanText("E8 ?? ?? 00 00 48 8D 8F ?? ?? 00 00 48 89 87 ?? ?? 00 00");
        _getResourceAsyncHook = new Hook<GetResourceAsyncPrototype>(getResourceAsync, GetResourceAsyncDetour);
        _getResourceSyncHook = new Hook<GetResourceSyncPrototype>(getResourceSync, GetResourceSyncDetour);
        _getResourceAsyncHook.Enable();
        _getResourceSyncHook.Enable();
    }
    
    private IntPtr GetResourceSyncDetour(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr pPath, IntPtr a6)
    {
        var ret = _getResourceSyncHook.Original(a1, a2, a3, a4, pPath, a6);
        return ret; 
    }
    
    private IntPtr GetResourceAsyncDetour(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr pPath, IntPtr a6, byte a7)
    {
        IntPtr ret;
        if (pPath != IntPtr.Zero)
        {
            var path = Marshal.PtrToStringAnsi(pPath);
            if (path != null && path.StartsWith("exd/Action_0_de"))
            {
                PluginLog.Information("Rewriting exd/Action_0_de.exd -> exd/Action_0_en.exd");
                var englishPath = Marshal.StringToHGlobalAnsi("exd/Action_0_en.exd");
                ret = _getResourceAsyncHook.Original(a1, a2, a3, a4, englishPath, a6, a7);
                Marshal.FreeHGlobal(englishPath);
                return ret;  
            }
        }
        ret = _getResourceAsyncHook.Original(a1, a2, a3, a4, pPath, a6, a7);
        return ret;    
    }

    private void OnCommand(string command, string rawArgs)
    {
        PluginLog.Information("Attempting to reload Action Sheet");
        unsafe
        {
            var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
            framework->ExdModule->ExcelModule->LoadSheet("Action");
        }
        PluginLog.Information("Successfully reloaded Action Sheet");
    }

    public void Dispose()
    {
        _getResourceSyncHook?.Disable();
        _getResourceSyncHook?.Dispose();
        _getResourceAsyncHook?.Disable();
        _getResourceAsyncHook?.Dispose();
        Service.CommandManager.RemoveHandler(CommandName);
    }
}