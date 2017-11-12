using System;
using ImGuiNET;

namespace Discord
{
    class Program
    {
        static void Main(string[] args)
        {
            Window w = new Window(new WindowSettings());
            w.Run();
        }
    }
}
