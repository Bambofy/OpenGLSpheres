using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGLSpheres
{
    /// <summary>
    /// Container for all the shader programs in the game, will need unique ones
    /// for spheres/planes/skybox.etc
    /// </summary>
    public static class ShaderProgramHandler
    {
        private static ShaderProgram activeProgram;
        private static List<ShaderProgram> _shaderPrograms = new List<ShaderProgram>();

        /// <summary>
        /// Insert it into the handler's list.
        /// </summary>
        /// <param name="shaderProgram"></param>
        public static void AddProgram(ShaderProgram shaderProgram)
        {
            _shaderPrograms.Add(shaderProgram);
        }

        /// <summary>
        /// Enables the shader to get locations and set locations. Also binds.
        /// </summary>
        /// <param name="index"></param>
        public static void SetActive(int index)
        {
            activeProgram = _shaderPrograms[index];
            activeProgram.Bind();
        }

        public static int GetUniformLocation(string name)
        {
            return activeProgram.GetUniformLocation(name);
        }

        public static int GetProgramID()
        {
            return activeProgram.ID;
        }


        public static void Dispose()
        {
            foreach (var shader in _shaderPrograms)
            {
                shader.Dispose();
            }
        }
    }

    public class ShaderProgram : IDisposable
    {
        public int ID = -1;
        public Dictionary<string, int> ShaderIDs = new Dictionary<string, int>();
        public Dictionary<string, int> UniformLocations = new Dictionary<string, int>();

        private static string ReadFile(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                return sr.ReadToEnd();
            }
        }

        public ShaderProgram()
        {
            ID = GL.CreateProgram();
        }

        public void LoadShader(string filename, ShaderType type)
        {
            // Create shader //
            int sID = GL.CreateShader(type);

            ShaderIDs.Add(filename, sID);

            GL.ShaderSource(sID, ReadFile(filename));
            GL.CompileShader(sID);
            Console.WriteLine(GL.GetShaderInfoLog(sID));

            GL.AttachShader(ID, sID);

            GL.LinkProgram(ID);
            Console.WriteLine(GL.GetProgramInfoLog(ID));
        }

        public void Bind()
        {
            GL.UseProgram(ID);
        }

        public int GetUniformLocation(string name)
        {
            int location = -1;

            if (!UniformLocations.TryGetValue(name, out location))
            {
                location = GL.GetUniformLocation(ID, name);
                UniformLocations.Add(name, location);
            }

            return location;
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing of shader program with id : " + ID);

            foreach (var shader in ShaderIDs)
            {
                GL.DeleteShader(shader.Value);
            }

            GL.DeleteProgram(ID);
        }
    }
}
