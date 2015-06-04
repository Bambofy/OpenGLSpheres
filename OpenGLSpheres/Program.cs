using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenGLSpheres
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MainWindow window = new MainWindow())
            {
                window.Run();
            }
        }
    }

    public class MainWindow : GameWindow
    {
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;

        private List<Pickup> Pickups = new List<Pickup>();
        private List<Player> Players = new List<Player>();


        protected override void OnLoad(EventArgs e)
        {
            // Defaults
            Title = "OpenTK Spheres";

            Width = 1080;
            Height = 900;

            GL.ClearColor(0, 0, 0, 1);
            CursorVisible = true;
            GL.Enable(EnableCap.DepthTest);

            // Create our first sphere shader
            ShaderProgram _sphereShaderProgram = new ShaderProgram();
            _sphereShaderProgram.LoadShader("shaders/sphere_vs.glsl", ShaderType.VertexShader);
            _sphereShaderProgram.LoadShader("shaders/sphere_fs.glsl", ShaderType.FragmentShader);

            // Give it to our handler
            ShaderProgramHandler.AddProgram(_sphereShaderProgram);
            ShaderProgramHandler.SetActive(0);

            // MVP matrix init and loading 
            GL.Uniform2(ShaderProgramHandler.GetUniformLocation("resolution"), new Vector2(Width, Height));
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Width/(float) Height,
                0.5f, 3000);
            GL.UniformMatrix4(ShaderProgramHandler.GetUniformLocation("proj"), false, ref _projectionMatrix);

            // Set up world objects
            Players.Add(new Player(new Vector3(0, 0, 0)));
            Pickups.Add(new Pickup(new Vector3(20, 0, 0)));
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Width / (float)Height,
                0.5f, 3000);
            GL.UniformMatrix4(ShaderProgramHandler.GetUniformLocation("proj"), false, ref _projectionMatrix);
            GL.Viewport(0,0, Width, Height);
        }

        public static bool Wireframe = false;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // Debug parameters //
            // Wireframe mode
            if (e.KeyChar == 'p')
            {
                Wireframe = !Wireframe;

                if (Wireframe)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                else
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            UpdateMouse();
            UpdateKeyboard();

            for (int i = 0; i < Players.Count; i++)
            {
                // Check for player pickup collisions
                Player thisPlayer = Players[i];

                for (int k = 0; k < Pickups.Count; k++)
                {
                    Pickup thisPickup = Pickups[k];

                    Sphere winner = Sphere.SphereCollision(ref thisPlayer, ref thisPickup);

                    // winner is always player if not null
                    if (winner != null)
                    {
                        thisPlayer.Score++;
                        Pickups.RemoveAt(k);
                    }
                }
            }
        }

        private Vector2 previousMouseState;
        public void UpdateMouse()
        {
            Vector2 currentMouseState = new Vector2(Mouse.X, Mouse.Y);

            if (currentMouseState.X != previousMouseState.X && currentMouseState.Y != previousMouseState.Y)
            {
                float dx = currentMouseState.X - Width/2f;
                float dy = currentMouseState.Y - Height/2f;

                Players[0].Direction = new Vector3(dx, -dy, 0).Normalized();
            }

            previousMouseState = currentMouseState;
        }

        private KeyboardState previousKeyboardState;
        public void UpdateKeyboard()
        {
            KeyboardState currentKeyboardState = OpenTK.Input.Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Key.Escape) && previousKeyboardState.IsKeyUp(Key.Escape))
            {
                Exit();
            }

            previousKeyboardState = currentKeyboardState;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (var player in Players) player.Render(e.Time);
            foreach (var pickup in Pickups) pickup.Render();

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            ShaderProgramHandler.Dispose();

            foreach (var ply in Players) ply.Dispose();
            foreach (var pickup in Pickups) pickup.Dispose();
        }
    }
}
