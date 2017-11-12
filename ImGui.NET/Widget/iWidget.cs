using System;
using System.Collections.Generic;
using System.Text;

namespace ImGui.NET.Widget
{
    public interface IWidget
    {
        void OnLoad();
        void OnUnload();
        void Draw();
    }
}
