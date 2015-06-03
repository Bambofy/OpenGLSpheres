using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using LibNoise;
using LibNoise.Modifier;
using LibNoise.Primitive;

namespace OpenGLSpheres
{
    public class Triangle
    {
        public Vector3[] Vertices;
        public uint[] Elements;
        public Vector3[] Normals;

        public Triangle()
        {
            // 0 = bottom left, 1 = bottom right, 2 = top
            Vertices = new Vector3[3];
            Elements = new uint[]
            {
                0, 1, 2
            };
            // 3 vertices on a triangle, each one needs a normal.
            Normals = new Vector3[3];
        }

        // todo but this in the sphere class
        public List<Triangle> Split(ref ImprovedPerlin noise, float sphereRadius, bool flat)
        {
            List<Triangle> newTriangles = new List<Triangle>();

            // Source reference
            Vector3 bottomLeft = Vertices[0];
            Vector3 bottomRight = Vertices[1];
            Vector3 top = Vertices[2];

            // we generate 4 new triangles (therefore 12 new vertices in total) //

            // Midpoint of sides
            Vector3 leftMid = VectorMaths.Midpoint(bottomLeft, top);
            Vector3 rightMid = VectorMaths.Midpoint(bottomRight, top);
            Vector3 bottomMid = VectorMaths.Midpoint(bottomLeft, bottomRight);

            // Bottom left triangle
            Triangle bottomLeftTriangle = new Triangle();
            bottomLeftTriangle.Vertices[0] = leftMid;
            bottomLeftTriangle.Vertices[1] = bottomLeft;
            bottomLeftTriangle.Vertices[2] = bottomMid;

            // Bottom right triangle
            Triangle bottomRightTriangle = new Triangle();
            bottomRightTriangle.Vertices[0] = rightMid;
            bottomRightTriangle.Vertices[1] = bottomRight;
            bottomRightTriangle.Vertices[2] = bottomMid;

            // Middle triangle
            Triangle middleTriangle = new Triangle();
            middleTriangle.Vertices[0] = leftMid;
            middleTriangle.Vertices[1] = rightMid;
            middleTriangle.Vertices[2] = bottomMid;

            // Top triangle
            Triangle topTriangle = new Triangle();
            topTriangle.Vertices[0] = leftMid;
            topTriangle.Vertices[1] = rightMid;
            topTriangle.Vertices[2] = top;

            newTriangles.Add(bottomLeftTriangle);
            newTriangles.Add(bottomRightTriangle);
            newTriangles.Add(topTriangle);
            newTriangles.Add(middleTriangle);

            // Apply our NORMALIZATION
            for (int i = 0; i < newTriangles.Count; i++)
            {
                for (int k = 0; k < newTriangles[i].Vertices.Length; k++)
                {
                    Vector3 viewSpaceVertex = newTriangles[i].Vertices[k];
                    float offset = noise.GetValue(viewSpaceVertex.X/2, viewSpaceVertex.Y/2, viewSpaceVertex.Z/2)/2;

                    if (!flat)
                    {
                        newTriangles[i].Vertices[k] = VectorMaths.Normalize(new Vector3(0, 0, 0), viewSpaceVertex,
                            sphereRadius + offset);
                        newTriangles[i].Normals[k] = new Vector3(offset);
                    }
                    else
                    {
                        newTriangles[i].Vertices[k] = VectorMaths.Normalize(new Vector3(0, 0, 0), viewSpaceVertex,
                            sphereRadius);
                        newTriangles[i].Normals[k] = new Vector3(0,0,0);
                    }

                }
            }

            return newTriangles;
        }
    }
}
