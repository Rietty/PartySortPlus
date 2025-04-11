using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ECommons.SimpleGui;
using ECommons.Configuration;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using PartySortPlus.Configuration;
using System.Reflection;
using ECommons;
using ECommons.DalamudServices;
using System;
using ECommons.Schedulers;
using ECommons.Logging;
using System.IO.Compression;
using System.IO;
using PartySortPlus.GUI;
using ECommons.EzEventManager;

namespace PartySortPlus;

public unsafe class PartySortPlus: IDalamudPlugin
{
    public static PartySortPlus? P;
    public static Config? C;

    public bool SoftForceUpdate = false;

    public PartySortPlus(IDalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, this, ECommons.Module.DalamudReflector);

        _ = new TickScheduler(() =>
        {
            C = EzConfig.Init<Config>();
            var ver = GetType().Assembly.GetName().Version?.ToString();
            if (C != null && C.LastVersion != ver)
            {
                try
                {
                    using (var fs = new FileStream(Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, $"Backup_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.zip"), FileMode.Create))
                    using (var arch = new ZipArchive(fs, ZipArchiveMode.Create))
                    {
                        arch.CreateEntryFromFile(EzConfig.DefaultConfigurationFileName, EzConfig.DefaultSerializationFactory.DefaultConfigFileName);
                    }
                    C.LastVersion = ver ?? string.Empty; // Ensure non-null assignment
                    DuoLog.Information($"Because plugin version was changed, a backup of your current configuraton has been created.");
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }

            EzConfigGui.Init(UI.DrawMain);
            EzCmd.Add("/psp", OnCommand, "open the plugin interface");

            new EzTerritoryChanged(TerritoryChanged);
        });
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
        P = null;
        C = null;
    }

    private void TerritoryChanged(ushort id)
    {
        SoftForceUpdate = true;
    }

    private void OnCommand(string command, string arguments)
    {
        if (arguments.EqualsIgnoreCaseAny("debug"))
        {
            if (C != null)
            {
                C.Debug = !C.Debug;
                DuoLog.Information($"Debug mode is now {(C.Debug ? "enabled" : "disabled")}");
            }
        } else
        {
            EzConfigGui.Window.IsOpen ^= true;
        }
    }
}
