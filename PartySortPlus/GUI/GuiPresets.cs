using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ECommons.ExcelServices;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using ImGuiNET;
using PartySortPlus.Configuration;
using System.Numerics;

namespace PartySortPlus.GUI
{
    public static class GuiPresets
    {
        private static Vector2 iconSize => new(24f);

        public static void Draw()
        {
            if (PartySortPlus.C == null) return;

            using (var leftPanelStyle = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.WindowPadding, Vector2.Zero))
            {
                using (var leftPanelChild = ImRaii.Child("LeftPanel", new Vector2(200, ImGui.GetContentRegionAvail().Y), true))
                {
                    if (leftPanelChild)
                    {
                        leftPanelStyle.Push(ImGuiStyleVar.WindowPadding, new Vector2(4, 3));
                        using (var presetListChild = ImRaii.Child("PresetsList", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeight()), true))
                        {
                            if (presetListChild)
                            {
                                foreach (var preset in PartySortPlus.C.GlobalProfile.Presets)
                                {
                                    if (ImGui.Selectable(preset.Name, PartySortPlus.C.GlobalProfile.SelectedPreset == preset))
                                    {
                                        PartySortPlus.C.GlobalProfile.SelectedPreset = preset;
                                    }
                                }
                            }
                        }
                        leftPanelStyle.Pop();

                        leftPanelStyle.Push(ImGuiStyleVar.FrameRounding, 0);
                        if (ImGuiEx.IconButton(FontAwesome.Plus, "##addpreset", new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetFrameHeight())))
                        {
                            var newPresetName = $"Preset {PartySortPlus.C.GlobalProfile.Presets.Count + 1}";
                            var newPreset = new Preset(newPresetName);
                            PartySortPlus.C.GlobalProfile.Presets.Add(newPreset);
                        }

                        ImGui.SameLine();

                        if (ImGuiEx.IconButton(FontAwesome.Trash, "##deletepreset", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())) && ImGui.GetIO().KeyCtrl)
                        {
                            if (PartySortPlus.C.GlobalProfile.SelectedPreset != null)
                            {
                                PluginLog.Debug($"Deleting preset: {PartySortPlus.C.GlobalProfile.SelectedPreset.Name}");
                                PartySortPlus.C.GlobalProfile.Presets.Remove(PartySortPlus.C.GlobalProfile.SelectedPreset);
                                PartySortPlus.C.GlobalProfile.SelectedPreset = null;
                            }
                        }

                        leftPanelStyle.Pop();
                    }
                }
            }

            ImGui.SameLine();

            using (var rightPanelStyle = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0))
            {
                using (var rightPanelChild = ImRaii.Child("RightPanel", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y), true))
                {
                    if (rightPanelChild)
                    {
                        var selectedPreset = PartySortPlus.C.GlobalProfile.SelectedPreset;
                        if (selectedPreset != null)
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.PencilAlt, "##editpresetname"))
                            {
                                PartySortPlus.C.GlobalProfile.isEditingPresetName = true;
                            }

                            ImGui.SameLine();

                            if (PartySortPlus.C.GlobalProfile.isEditingPresetName)
                            {
                                ImGui.InputText("##EditName", ref selectedPreset.Name, 100);
                                if (ImGui.IsItemDeactivated())
                                {
                                    PartySortPlus.C.GlobalProfile.isEditingPresetName = false;
                                }
                            }
                            else
                            {
                                ImGui.Text($"{selectedPreset.Name}");
                            }
                        }

                        if (selectedPreset != null)
                        {
                            ImGui.Separator();
                            using (ImRaii.Child("PresetSettings", new Vector2(0, ImGui.GetFrameHeight() * 2)))
                            {
                                ImGui.Text("Settings:");
                                ImGui.Checkbox("Place YOUR name at top of list?", ref selectedPreset.isPlayerAtTop);
                            }
                            
                            ImGui.TextWrapped("You can drag and drop jobs to reorder them according to your desired sorting preferences.");
                            using (ImRaii.Child("JobOrderList", new Vector2(0, ImGui.GetContentRegionAvail().Y), true))
                            {
                                foreach (var job in selectedPreset.JobOrder)
                                {
                                    if (ThreadLoadImageHandler.TryGetIconTextureWrap((uint)job.GetIcon(), false, out var texture))
                                    {
                                        ImGui.Image(texture.ImGuiHandle, iconSize);
                                        ImGui.SameLine();
                                    }
                                    var cursor = ImGui.GetCursorPos();
                                    ImGui.SetCursorPos(new Vector2(cursor.X - 5, cursor.Y + 4)); // push down by 2 pixels
                                    ImGui.Text($"{job}");
                                }
                            }
                        }
                        else
                        {
                            ImGui.Text("No preset selected.");
                        }
                    }
                }
            }
        }
    }
}
