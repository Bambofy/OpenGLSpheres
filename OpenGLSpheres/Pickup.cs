using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSpheres
{
    public class Pickup : Sphere
    {
        private Vector3 lightPosition;

        public Pickup(Vector3 origin)
            : base(origin, 1f, true)
        {
            Random rand = new Random();

            int digit = rand.Next(1, 7);
            Console.WriteLine(digit);
            {
                switch (digit)
                {
                    case 1:
                        _colour = new Vector4(1, 0, 0, 0.8f);
                        break;
                    case 2:
                        _colour = new Vector4(0, 1, 0, 0.8f);
                        break;
                    case 3:
                        _colour = new Vector4(0, 0, 1, 0.8f);
                        break;
                    case 4:
                        _colour = new Vector4(1, 1, 0, 0.8f);
                        break;
                    case 5:
                        _colour = new Vector4(0, 1, 1, 0.8f);
                        break;
                    case 6:
                        _colour = new Vector4(1, 0, 1, 0.8f);
                        break;
                }
            }
        }

        public void Render(int ShaderID)
        {
            // always be above it.
            Vector3 lightPos = _origin + new Vector3(0, 12, 0);
            GL.Uniform3(GL.GetUniformLocation(ShaderID, "lightPos"), lightPos);

            GL.Uniform3(GL.GetUniformLocation(ShaderID, "lightColour"), Vector3.One);

            // Tell the shaders we are a wobbly-one
            GL.Uniform1(GL.GetUniformLocation(ShaderID, "isPickup"), 1);
            GL.Uniform4(GL.GetUniformLocation(ShaderID, "colour"), _colour);

            base.Render(ShaderID);
        }
    }
}
