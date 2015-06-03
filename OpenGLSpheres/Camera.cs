using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace OpenGLSpheres
{
    // +Z up
    public class Camera
    {
        private Vector3 _position = new Vector3(0, 0, 0);
        private Vector3 _lookDir = new Vector3(0, 0, -100);

        public Matrix4 ViewMatrix;

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                ViewMatrix = Matrix4.LookAt(_position, _position + _lookDir, Vector3.UnitY);
            }
        }
    }
}
