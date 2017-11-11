using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImGuiNET
{
    public sealed class WindowControl
    {

        INativeWindow window;
        IO io;

        public WindowControl(ref INativeWindow window, ref IO io)
        {
            this.window = window;
            this.io = io;
        }

        public void Update()
        {
            UpdateMouse();
        }


        float _wheelPosition;

        public void UpdateMouse()
        {
            MouseState cursorState = Mouse.GetCursorState();
            MouseState mouseState = Mouse.GetState();

            if (window.Focused)
            {
                Point windowPoint = window.PointToClient(new Point(cursorState.X, cursorState.Y));
                io.MousePosition = new System.Numerics.Vector2(windowPoint.X / io.DisplayFramebufferScale.X, windowPoint.Y / io.DisplayFramebufferScale.Y);
            }
            else
                io.MousePosition = new System.Numerics.Vector2(-1f, -1f);

            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            float newWheelPos = mouseState.WheelPrecise;
            float delta = newWheelPos - _wheelPosition;
            _wheelPosition = newWheelPos;
            io.MouseWheel = delta;
        }
    }
}
