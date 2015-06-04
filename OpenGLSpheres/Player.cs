using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSpheres
{
    public class Player : Sphere
    {
        private int _score = 0;
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                _radius = (_score);
            }
        }

        public Camera Camera = new Camera();
        protected float Speed = 5f;
        public Vector3 Direction = Vector3.Zero;

        public Player(Vector3 origin)
            : base(origin, 1f, true)
        {
            // Locked means it will ignore rotation attribute.
            Camera.Position = new Vector3(0, 0, _radius);

            Random rand = new Random(Guid.NewGuid().GetHashCode());

            int digit = rand.Next(1, 7);
            switch (digit)
            {
                case 1:
                    _colour = new Vector4(1, 0, 0, 0.3f);
                    break;
                case 2:
                    _colour = new Vector4(0, 1, 0, 0.3f);
                    break;
                case 3:
                    _colour = new Vector4(0, 0, 1, 0.3f);
                    break;
                case 4:
                    _colour = new Vector4(1, 1, 0, 0.3f);
                    break;
                case 5:
                    _colour = new Vector4(0, 1, 1, 0.3f);
                    break;
                case 6:
                    _colour = new Vector4(1, 0, 1, 0.3f);
                    break;
            }
        }

        public void Render(double dt)
        {
            int ShaderID = ShaderProgramHandler.GetProgramID();

            // always point the velocity towards the direction with length Speed
            Vector3 Velocity = Direction*Speed;
            _origin += Velocity*(float) dt;

            var r2 = 10 + ((_radius*_radius)*2);
            Camera.Position = _origin + new Vector3(0, 0, r2);
            GL.UniformMatrix4(GL.GetUniformLocation(ShaderID, "view"), false, ref Camera.ViewMatrix);


            // Calculate the lightpos from the direction we are travelling.
            // Direction is normalized.
            Vector3 directionOffset = Direction * r2;
            Vector3 lightPos = Camera.Position + directionOffset;
            GL.Uniform3(GL.GetUniformLocation(ShaderID, "lightPos"), lightPos);

            // Grey-ish light
            Vector3 lightColour = new Vector3(0.85f, 0.85f, 0.85f);
            GL.Uniform3(GL.GetUniformLocation(ShaderID, "lightColour"), lightColour);

            // Tell the shaders we are NOT a wobbly-one
            GL.Uniform1(GL.GetUniformLocation(ShaderID, "isPickup"), 0);
            GL.Uniform4(GL.GetUniformLocation(ShaderID, "colour"), _colour);

            base.Render(ShaderID);
        }
    }
}
