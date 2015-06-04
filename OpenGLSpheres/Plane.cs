using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSpheres
{
    public class Plane
    {
        private int VertexID;
        private int ElementID;

        public Plane()
        {
            Vector3[] vertices =
            {
                new Vector3(-1, 1, 0),
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
            };

            uint[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };

            GL.GenBuffers(1, out VertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexID);
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(vertices.Length * Vector3.SizeInBytes),
                vertices,
                BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out ElementID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(sizeof(uint) * indices.Length),
                indices,
                BufferUsageHint.StaticDraw);

        }
    }
}
