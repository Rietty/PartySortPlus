using ImGuiNET;
using OtterGui.Widgets;
using OtterGuiInternal.Utility;
using PartySortPlus.GUI;
using System.Numerics;  

namespace PartySortPlus.GUI
{
    public class TristateCheckboxEx : TristateCheckbox
    {
        protected override void RenderSymbol(sbyte value, Vector2 position, float size)
        {
            switch (value)
            {
                case -1:
                    SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, CrossColor, size);
                    break;
                case 1:
                    SymbolHelpers.RenderCheckmark(ImGui.GetWindowDrawList(), position, CheckColor, size);
                    break;
                default:
                    //SymbolHelpers.RenderDot(ImGui.GetWindowDrawList(), position, DotColor, size);
                    break;
            }
        }
    }
}
