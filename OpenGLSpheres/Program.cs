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
                //window.WindowState = WindowState.Fullscreen;
                window.Run();
            }
        }
    }

    public class MainWindow : GameWindow
    {
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;

        private int shaderID;

        private const string vertexSource = @"#version 330

layout (location = 0) in vec3 vertex_position;
layout (location = 1) in vec3 vertex_normal;

uniform mat4 proj;
uniform mat4 view;
uniform mat4 model;
uniform int isPickup;

out vec4 vpos;
out vec4 vnormal;
                         
// the normals given from the program is just vertex_position.normalized() so ignore.
void main()
{
    // view space
    vnormal = normalize(vec4(vertex_position, 1.0)); //vec4(vertex_normal, 0.0);

    // if it's a pickup then oscillate.
    vec4 offset = vec4(0, 0, 0, 0);
    if (isPickup == 1)
    {
        float length = 1;           // vertex_normal.x + (sin(ticker) / 10);
        offset = vnormal * length;
    }

    // apply the amplitude * length of sine value.
    vec4 newPosition = vec4(vertex_position + offset.xyz, 1.0);


    vnormal = normalize(newPosition);
    vpos = model * newPosition;

    gl_Position = proj * view * vpos;
}";

        private const string fragmentSource = @"#version 330

in vec4 vpos;       // model space
in vec4 vnormal;    // view space

uniform vec4 colour;
uniform vec2 resolution;
uniform vec3 lightPos;
uniform vec3 lightColour;

out vec4 out_color;

void main()
{
    vec4 LightPos = vec4(lightPos.xyz, 1.0);
    vec4 diffuseColor = vec4(colour.xyz, 1.0);

    vec4 LightDir = LightPos - vpos;
    LightDir.x *= resolution.x/resolution.y;

    float D = length(LightDir);
    vec4 L = normalize(LightDir);

    vec4 Diffuse = vec4(lightColour, 1.0) * max(dot(vnormal, L), 0.0);

    out_color = diffuseColor + Diffuse;
}";


        private Pickup pickup;
        private Player player;

        protected override void OnLoad(EventArgs e)
        {
            Title = "OpenTK Spheres";

            Width = 1080;
            Height = 900;

            GL.ClearColor(0, 0, 0, 1);
            CursorVisible = true;

            GL.Enable(EnableCap.DepthTest);

            // Create shaders //
            shaderID = GL.CreateProgram();
            int vsID = GL.CreateShader(ShaderType.VertexShader);
            int fsID = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vsID, vertexSource);
            GL.ShaderSource(fsID, fragmentSource);

            GL.CompileShader(vsID);
            GL.CompileShader(fsID);

            Console.WriteLine(GL.GetShaderInfoLog(vsID));
            Console.WriteLine(GL.GetShaderInfoLog(fsID));

            GL.AttachShader(shaderID, vsID);
            GL.AttachShader(shaderID, fsID);
            
            GL.LinkProgram(shaderID);
            Console.WriteLine(GL.GetProgramInfoLog(shaderID));
            GL.UseProgram(shaderID);

            GL.Uniform2(GL.GetUniformLocation(shaderID, "resolution"), new Vector2(Width, Height));
            // MVP matrix init and loading //
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Width/(float) Height,
                0.5f, 3000);

            GL.UniformMatrix4(GL.GetUniformLocation(shaderID, "proj"), false, ref _projectionMatrix);

            player = new Player(new Vector3(0, 0, 0));
            pickup = new Pickup(new Vector3(20, 0, 0));
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Width / (float)Height,
                0.5f, 3000);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderID, "proj"), false, ref _projectionMatrix);
            GL.Viewport(0,0, Width, Height);
        }

        public static bool Wireframe = false;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == 'g')
            {
                player.SplitTriangles();
            }

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

            // Regenerate sphere but with new offsets.
            if (e.KeyChar == 'm')
            {
                player.ResetNoiseValues();
            }

            if (e.KeyChar == 'r')
            {
                player.SetRadius(player.GetRadius() + 1);
            }
        }

        private float _ticker = 0;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            UpdateMouse();
            UpdateKeyboard();

            // Check for sphere collisions
            if (Sphere.simpleSphereCollision(player, pickup))
            {
                Console.WriteLine("Hit");
                Sphere.sphereCollisionResponse(player, pickup);
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

                player.Direction = new Vector3(dx, -dy, 0).Normalized();
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

            player.Render(shaderID, e.Time);
            pickup.Render(shaderID);

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteProgram(shaderID);
            player.Delete();
            pickup.Delete();
        }
    }
}
