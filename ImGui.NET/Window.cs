using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Threading;

#if NETFRAMEWORK
using System.Drawing;
#endif

namespace ImGuiNET
{
    public class Window
    {
        INativeWindow window;
        IGraphicsContext gfxContext;
        IO io;
        WindowSettings setting;
        WindowControl control;
        Dictionary<string, Font> loadedFonts;

        public Window(WindowSettings settings)
        {
            loadedFonts = new Dictionary<string, Font>();

            setting = settings;
            window = new NativeWindow(setting.Width, setting.Height, setting.Title, GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);
            gfxContext = new GraphicsContext(GraphicsMode.Default, window.WindowInfo, 3, 0, GraphicsContextFlags.Default);

            io = ImGui.GetIO();
            control = new WindowControl(ref window, ref io);

            gfxContext.LoadAll();
            gfxContext.MakeCurrent(window.WindowInfo);
            GL.ClearColor(Color.Black);

            loadedFonts.Add("default", io.FontAtlas.AddDefaultFont());

            CreateDeviceObjects();

            window.Visible = true;
        }

        public void Run()
        {
            DateTime previousFrameStartTime;

            while (window.Exists)
            {
                previousFrameStartTime = DateTime.UtcNow;

                Render();

                window.ProcessEvents();

                DateTime afterFrameTime = DateTime.UtcNow;
                double elapsed = (afterFrameTime - previousFrameStartTime).TotalSeconds;
                double sleepTime = setting.DesiredFrameRate - elapsed;
                if (sleepTime > 0.0)
                {
                    DateTime finishTime = afterFrameTime + TimeSpan.FromSeconds(sleepTime);
                    while (DateTime.UtcNow < finishTime)
                        Thread.Sleep(0);
                }
            }
        }

        unsafe void Render()
        {
            io.DisplaySize = new System.Numerics.Vector2(window.Width, window.Height);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(window.Width / setting.Width);
            io.DeltaTime = setting.DesiredFrameRate;

            control.Update();

            ImGui.NewFrame();
            DrawUI();
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }

        void DrawUI()
        {
            bool windowOpen = window.Visible;

            System.Numerics.Vector2 windowSize
                = new System.Numerics.Vector2(window.Width, window.Height);




            ImGui.SetNextWindowSize(windowSize, Condition.Always);
            ImGui.SetNextWindowPosCenter(Condition.Always);
            ImGui.BeginWindow(window.Title, ref windowOpen, WindowFlags.NoResize | WindowFlags.NoTitleBar | WindowFlags.NoMove);

            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("About", "Ctrl-Alt-A", false, true))
                {

                }
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();

            ImGui.Text("Hello,");
            ImGui.Text("World!");
            ImGui.Text("From ImGui.NET. ...Did that work?");
            var pos = io.MousePosition;
            bool leftPressed = io.MouseDown[0];
            ImGui.Text("Current mouse position: " + pos + ". Pressed=" + leftPressed);
            ImGui.EndWindow();
        }

        unsafe void CreateDeviceObjects()
        {
            IO io = ImGui.GetIO();

            // Build texture atlas
            FontTextureData texData = io.FontAtlas.GetTexDataAsAlpha8();

            // Create OpenGL texture
            int s_fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, s_fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Alpha,
                texData.Width,
                texData.Height,
                0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte,
                new IntPtr(texData.Pixels));

            // Store the texture identifier in the ImFontAtlas substructure.
            io.FontAtlas.SetTexID(s_fontTexture);

            // Cleanup (don't clear the input data if you want to append new fonts later)
            //io.Fonts->ClearInputData();
            io.FontAtlas.ClearTexData();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        unsafe void RenderImDrawData(DrawData* draw_data)
        {
            // Rendering
            int display_w, display_h;
            display_w = window.Width;
            display_h = window.Height;

            Vector4 clear_color = new Vector4(114f / 255f, 144f / 255f, 154f / 255f, 1.0f);
            GL.Viewport(0, 0, display_w, display_h);
            GL.ClearColor(clear_color.X, clear_color.Y, clear_color.Z, clear_color.W);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // We are using the OpenGL fixed pipeline to make the example code simpler to read!
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers.
            int last_texture;
            GL.GetInteger(GetPName.TextureBinding2D, out last_texture);
            GL.PushAttrib(AttribMask.EnableBit | AttribMask.ColorBufferBit | AttribMask.TransformBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            GL.UseProgram(0);

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            IO io = ImGui.GetIO();
            ImGui.ScaleClipRects(draw_data, io.DisplayFramebufferScale);

            // Setup orthographic projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(
                0.0f,
                io.DisplaySize.X / io.DisplayFramebufferScale.X,
                io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                0.0f,
                -1.0f,
                1.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Render command lists

            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                NativeDrawList* cmd_list = draw_data->CmdLists[n];
                byte* vtx_buffer = (byte*)cmd_list->VtxBuffer.Data;
                ushort* idx_buffer = (ushort*)cmd_list->IdxBuffer.Data;

                DrawVert vert0 = *((DrawVert*)vtx_buffer);
                DrawVert vert1 = *(((DrawVert*)vtx_buffer) + 1);
                DrawVert vert2 = *(((DrawVert*)vtx_buffer) + 2);

                GL.VertexPointer(2, VertexPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.PosOffset));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.UVOffset));
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.ColOffset));

                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pcmd->TextureId.ToInt32());
                        GL.Scissor(
                            (int)pcmd->ClipRect.X,
                            (int)(io.DisplaySize.Y - pcmd->ClipRect.W),
                            (int)(pcmd->ClipRect.Z - pcmd->ClipRect.X),
                            (int)(pcmd->ClipRect.W - pcmd->ClipRect.Y));
                        ushort[] indices = new ushort[pcmd->ElemCount];
                        for (int i = 0; i < indices.Length; i++) { indices[i] = idx_buffer[i]; }
                        GL.DrawElements(PrimitiveType.Triangles, (int)pcmd->ElemCount, DrawElementsType.UnsignedShort, new IntPtr(idx_buffer));
                    }
                    idx_buffer += pcmd->ElemCount;
                }
            }

            // Restore modified state
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindTexture(TextureTarget.Texture2D, last_texture);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.PopAttrib();

            gfxContext.SwapBuffers();
        }
    }
}
