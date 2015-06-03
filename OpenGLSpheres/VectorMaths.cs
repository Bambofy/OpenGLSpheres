using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenGLSpheres
{
    static public class VectorMaths
    {
        /// <summary>
        /// "Pushes" a vector from A->B to a given Length.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Vector3 Normalize(Vector3 A, Vector3 B, float length)
        {
            // Distance between A and B along the X/Y/Z axis;
            Vector3 Direction = B - A;
            float Distance = (B - A).Length;

            // right now distance is from A to B we want it to equal the new length
            Direction *= length / Distance;

            Vector3 newPoint = new Vector3();
            newPoint = A + Direction;

            return newPoint;
        }

        public static Vector3 Midpoint(Vector3 Origin, Vector3 Target)
        {
            return (Origin + Target) / 2;
        }

    }
}
