using Dalamud.Interface;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PartySortPlus;
using System.Threading.Tasks;

namespace PartySortPlus.GUI
{
    public static class UITutorial
    {
        private static readonly string Content = @"Welcome to Party Sort+!";

        public static void Draw()
        {
            if (PartySortPlus.C != null)
            {
                ImGuiEx.CheckboxInverted("Hide tutorial", ref PartySortPlus.C.ShowTutorial);
            }
            ImGuiEx.TextWrapped(Content);
        }
    }
}
