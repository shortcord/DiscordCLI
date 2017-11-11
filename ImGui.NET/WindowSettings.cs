using System;
using System.Collections.Generic;
using System.Text;

namespace ImGuiNET
{
    public sealed class WindowSettings
    {
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public string Title { get; set; } = "My ImGUI Window";
        public float DesiredFrameRate { get; set; } = (1f / 60.0f);
    }
}
