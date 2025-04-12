using Dalamud.Plugin;
using ECommons.SimpleGui;
using ECommons.Configuration;
using PartySortPlus.Configuration;
using ECommons.Automation.LegacyTaskManager;
using ECommons;
using ECommons.DalamudServices;
using System;
using ECommons.Schedulers;
using ECommons.Logging;
using System.IO.Compression;
using System.IO;
using PartySortPlus.GUI;
using ECommons.EzEventManager;
using ECommons.GameHelpers;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Linq;

namespace PartySortPlus;

public unsafe class PartySortPlus: IDalamudPlugin
{
    public static PartySortPlus? P;
    public static Config? C;

    public TaskManager? TaskManager;

    public bool SoftForceUpdate = false;
    public bool ForceUpdate = false;

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
            EzCmd.Add("/psp", OnCommand, "Run a party list sort!"
                + "\n/psp ui → Show the configuration window."
                + "\n/psp config → Show the configuration window."
            );

            new EzFrameworkUpdate(OnUpdate);
            new EzTerritoryChanged(TerritoryChanged);
            TaskManager = new TaskManager()
            {
                TimeLimitMS = 2000,
                AbortOnTimeout = true,
                TimeoutSilently = false,
            };
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
        } 
        else if(arguments.EqualsIgnoreCaseAny("ui") || arguments.EqualsIgnoreCaseAny("config"))
        {
            EzConfigGui.Window.IsOpen ^= true;
        }
        else if (arguments.EqualsIgnoreCaseAny("mini"))
        {
            AgentHUD.Instance()->PartyMembers[0].Index = 1;
            AgentHUD.Instance()->PartyMembers[1].Index = 0;
        }
        else
        {
           ForceUpdate = true;
        }
    }

    private void OnUpdate()
    {
        if (C == null) return;
        if (TaskManager == null) return;

        if (Player.Interactable)
        {
            if (!TaskManager.IsBusy)
            {
                List<ApplyRule> newRule = [];
                if (C.Enable)
                {
                    foreach (var r in C.GlobalProfile.Rules)
                    {
                        if(r.Enabled)
                        {
                            var territories = r.Territories;
                            var jobs = r.Jobs;
                            var partyJobs = r.PartyJobs;
                            if (C.Cond_Enabled && C.Cond_Territory && C.Cond_Roles && C.Cond_Jobs && C.Cond_PartyJobs)
                            {
                                if (C.AllowNegativeConditions)
                                {
                                    if (r.Not.Territories.Count > 0 || r.Not.Jobs.Count > 0 || r.Not.PartyJobs.Count > 0)
                                    {
                                        newRule.Add(r);
                                    }
                                }
                                else
                                {
                                    newRule.Add(r);
                                }
                            }
                        }
                    }
                    if (ForceUpdate || (SoftForceUpdate && newRule.Count >0))
                    {
                        SoftForceUpdate = false;
                        ForceUpdate = false;
                        PluginLog.Debug($"Force updating party list with {newRule.Count} rules.");

                        // Iterate through the rules and filter them based on the conditions.
                        // We need the following: PartyJobs have to be all in the current party, Our Player's job has to match one of the jobs in the rule, and the territory has to match.
                        PluginLog.Debug($"Checking {newRule.Count} rules.");
                        PluginLog.Debug($"Current territory: {Player.Territory}");
                        PluginLog.Debug($"Current job: {Player.Job}");
                        foreach (ref var partyMember in AgentHUD.Instance()->PartyMembers)
                        {
                            if (partyMember.Object == null) continue;
                            PluginLog.Debug($"Current party member: {partyMember.Name}");
                            PluginLog.Debug($"Current party member job: {partyMember.Object->ClassJob}");
                        }

                        // Rule filtering logic
                        foreach (var rule in newRule)
                        {
                            bool skipTerritoryCheck = false;
                            bool skipJobCheck = false;
                            bool skipPartyJobCheck = false;

                            PluginLog.Debug($"Checking rule: {rule.GUID}");
                            if (rule.Territories.Count() == 0) skipTerritoryCheck = true;
                            if (rule.Jobs.Count() == 0) skipJobCheck = true;
                            if (rule.PartyJobs.Count() == 0) skipPartyJobCheck = true;

                            if (skipTerritoryCheck) PluginLog.Debug($"Territory check skipped for rule: {rule.GUID}");
                            if (skipJobCheck) PluginLog.Debug($"Job check skipped for rule: {rule.GUID}");
                            if (skipPartyJobCheck) PluginLog.Debug($"Party job check skipped for rule: {rule.GUID}");

                            // Check to see if the territory skip is false, and if so, does our territory be contained or not?
                            if (!skipTerritoryCheck && rule.Territories.Contains(Player.Territory))
                            {
                                PluginLog.Debug($"Territory check passed for rule: {rule.GUID}");
                            } else
                            {
                                continue;
                            }

                            // Check to see if the job skip is false, and if so, does our job be contained or not?
                            if (!skipJobCheck && rule.Jobs.Contains(Player.Job))
                            {
                                PluginLog.Debug($"Job check passed for rule: {rule.GUID}");
                            }
                            else
                            {
                                continue;
                            }

                            // Check to see if ALL of the party jobs that we currently have in party (GetPartyMemberJobs()) are contained in the rule, if so we can proceed, else skip the rule.
                            var partyJobs = GetPartyMemberJobs();
                            var allPartyJobsContained = true;
                            foreach (var job in rule.PartyJobs)
                            {
                                if (!partyJobs.Contains(job.ToString()))
                                {
                                    allPartyJobsContained = false;
                                    PluginLog.Debug($"Party job check failed for rule: {rule.GUID}");
                                    break;
                                }
                            }

                            if (allPartyJobsContained)
                            {
                                PluginLog.Information($"We can process this rule: {rule.GUID}");
                            }

                            try
                            {
                                // Grab the preset from this rule, and break out of the loop at the end.
                                string? presetNameToUse = rule.SelectedPresets.First();

                                if (presetNameToUse != null)
                                {
                                    Preset presetToUse = C.GlobalProfile.Presets[C.GlobalProfile.Presets.FindIndex(x => x.Name.EqualsIgnoreCase(presetNameToUse))];
                                    SortPartyList(presetToUse);
                                } 
                                else
                                {
                                    PluginLog.Error($"No preset found for rule: {rule.GUID}");
                                    continue;
                                }
                            } 
                            catch
                            {
                                PluginLog.Error($"Error while trying to get the preset from rule {rule.GUID}");
                                continue;
                            }

                            PluginLog.Information($"Processed rule {rule.GUID}! All other rules will be ignored, as this was first found one that matches current conditions.");
                            break;
                        }
                    }
                }
            }
        }
    }

    private void SortPartyList(Preset presetToUse)
    {
        PluginLog.Information($"Sorting party list with preset {presetToUse.Name}.");
    }

    private List<string> GetPartyMemberJobs()
    {
        var partyJobs = new List<string>();
        foreach (ref var partyMember in AgentHUD.Instance()->PartyMembers)
        {
            if (partyMember.Object != null)
            {
                var job = ECommons.ExcelServices.ExcelJobHelper.GetJobById(partyMember.Object->ClassJob);
                if (job.HasValue)
                {
                    partyJobs.Add(job.Value.Abbreviation.ToString());
                }
            }
        }
        return partyJobs;
    }
}
