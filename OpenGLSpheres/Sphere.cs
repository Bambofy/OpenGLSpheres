using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;
using LibNoise.Primitive;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSpheres
{
    public class Sphere : IDisposable
    {
        static public Sphere SphereCollision(ref Player s1, ref Pickup s2)
        {
            // Vector between centres of each sphere.
            float distance = (s1._origin - s2._origin).Length;
            
            // distance < s1.radius or s2.radius then collision!
            if (distance <= s1._radius || distance <= s2._radius)
            {
                if (s1._radius > s2._radius)
                {
                    return s1;
                }
                if (s1._radius < s2._radius)
                {
                    return s2;
                }
            }

            return null;
        }

        static public Sphere SphereCollision(ref Pickup s1, ref Player s2)
        {
            return SphereCollision(ref s2, ref s1);
        }

        protected Vector3 _origin;
        protected float _radius;
        protected float _mass = 50;
        protected Vector4 _colour = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        protected bool _flat;

        /// <summary>
        /// VBO handles.
        /// </summary>
        private int VertexID;
        private int NormalID;
        private int ElementID;

        public ImprovedPerlin PerlinNoise = new ImprovedPerlin();
        public List<Triangle> Triangles = new List<Triangle>(); 

        public Sphere(Vector3 origin, float radius, bool flat)
        {
            _origin = origin;
            _radius = radius;
            _flat = flat;

            // Get VBO handles.
            GL.GenBuffers(1, out VertexID);
            GL.GenBuffers(1, out ElementID);
            GL.GenBuffers(1, out NormalID);

            // Only need to call this once during the setup.
            GenerateIsohedron();

            SplitTriangles();
            SplitTriangles();
            SplitTriangles();
        }
        

        public void ResetNoiseValues()
        {
            PerlinNoise.Seed = Guid.NewGuid().GetHashCode();

            Triangles = new List<Triangle>();
            GenerateIsohedron();
            SplitTriangles();
        }

        private void GenerateIsohedron()
        {
            float X = 1.0f;
            float Z = 1.61803399f; // thnx bean

            // now we need to make 20 triangles with these vertices.
            Vector3[] verts = new Vector3[] 
            {                     
                new Vector3(-X, 0f, Z),
                new Vector3(X, 0f, Z),
                new Vector3(-X, 0f, -Z),
 
                new Vector3(X, 0f, -Z),
                new Vector3(0f, Z, X),
                new Vector3(0f, Z, -X),
 
                new Vector3(0f, -Z, X),
                new Vector3(0f, -Z, -X),
                new Vector3(Z, X, 0f),
 
                new Vector3(-Z, X, 0f),
                new Vector3(Z, -X, 0f),
                new Vector3(-Z, -X, 0f) 
            };

            int[] indices = new int[]
            {
                1, 4, 0,
                4, 9, 0,
                4, 5, 9,
                8, 5, 4,
                1, 8, 4,
                1, 10, 8,
                10, 3, 8,
                8, 3, 5,
                3, 2, 5,
                3, 7, 2,
                3, 10, 7,
                10, 6, 7,
                6, 11, 7,
                6, 0, 11,
                6, 1, 0,
                10, 1, 6,
                11, 0, 9,
                2, 11, 9,
                5, 2, 9,
                11, 2, 7 
            };

            for (int i = 0; i < indices.Length; i += 3)
            {
                uint i1 = (uint) indices[i];
                uint i2 = (uint) indices[i+1];
                uint i3 = (uint) indices[i+2];

                Vector3 v1 = verts[i1];
                Vector3 v2 = verts[i2];
                Vector3 v3 = verts[i3];

                Triangle tri = new Triangle();
                tri.Vertices[0] = v1;
                tri.Vertices[1] = v2;
                tri.Vertices[2] = v3;

                Triangles.Add(tri);
            }
        }

        /// <summary>
        /// This method generates a new list of triangles from the current ones,
        /// splits them up.
        /// </summary>
        public void SplitTriangles()
        {
            Console.WriteLine("\nSplitting...");

            int triangleCount = Triangles.Count;
            List<Triangle> newTriangles = new List<Triangle>();

            // Loop through all the triangles, split them, then add them to our new list.
            for (int i = 0; i < triangleCount; i++)
            {
                    newTriangles.AddRange(Triangles[i].Split(ref PerlinNoise, _radius, _flat));
            }

            Triangles = null;
            GC.Collect();
            Triangles = newTriangles;

            Console.WriteLine("Total added triangles: " + Triangles.Count);

            // Grab all the shape data as an array.
            Vector3[] triVerts = GetVertices();
            uint[] triElements = GetElements();
            Vector3[] triNormals = GetNormals();

            // Each triangle has 3 vertices attached.
            int vertexCount = triVerts.Length;
            int elementCount = triElements.Length;
            int normalCount = vertexCount;

            // buffer objects updating.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexID);
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(vertexCount * Vector3.SizeInBytes),
                triVerts,
                BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, NormalID);
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(normalCount * Vector3.SizeInBytes),
                triNormals,
                BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(elementCount * sizeof(uint)),
                triElements,
                BufferUsageHint.StreamDraw);

            Console.WriteLine("Done. Generated " + elementCount + " new vertices.");
            GC.Collect();
        }

        /// <summary>
        /// Returns an array of all triangles vertex position data in the correct order
        /// to match the Elements array!
        /// </summary>
        private Vector3[] GetVertices()
        {
            Vector3[] returnVerts = new Vector3[Triangles.Count * 3];

            int indexCount = 0;
            for (int i = 0; i < Triangles.Count; i++)
            {
                Vector3[] vertices = Triangles[i].Vertices;

                returnVerts[indexCount] = vertices[0];
                returnVerts[indexCount + 1] = vertices[1];
                returnVerts[indexCount + 2] = vertices[2];

                indexCount += 3;
            }

            return returnVerts;
        }

        private uint[] GetElements()
        {
            uint[] returnEles = new uint[Triangles.Count * 3];

            int indexCount = 0;
            for (int i = 0; i < Triangles.Count; i++)
            {
                uint[] elements = Triangles[i].Elements;

                returnEles[indexCount] = (uint)(indexCount + elements[0]);
                returnEles[indexCount + 1] = (uint)(indexCount + elements[1]);
                returnEles[indexCount + 2] = (uint)(indexCount + elements[2]);

                indexCount += 3;
            }

            return returnEles;
        }

        private Vector3[] GetNormals()
        {
            Vector3[] returnNormals = new Vector3[Triangles.Count * 3];

            int indexCount = 0;
            for (int i = 0; i < Triangles.Count; i++)
            {
                Vector3[] normals = Triangles[i].Normals;

                returnNormals[indexCount] = normals[0];
                returnNormals[indexCount + 1] = normals[1];
                returnNormals[indexCount + 2] = normals[2];

                indexCount += 3;
            }

            return returnNormals;
        }

        public void Render(int ShaderID)
        {
            // Translation/Scaling
            Matrix4 modelMatrix = Matrix4.CreateScale(_radius) * Matrix4.CreateTranslation(_origin);
            GL.UniformMatrix4(GL.GetUniformLocation(ShaderID, "model"), false, ref modelMatrix);

            // Set vertex attribs.
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexID);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, NormalID);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementID);
            GL.DrawElements(PrimitiveType.Triangles, Triangles.Count * 3, DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(VertexID);
            GL.DeleteBuffer(NormalID);
            GL.DeleteBuffer(ElementID);
            Triangles = null;

            GC.Collect();
        }
    }
}
