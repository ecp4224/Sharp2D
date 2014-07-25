using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Reflection;

namespace Sharp2D.Core.Graphics.Shaders
{
    public class Source
    {
        public string SourceCode { get; private set; }
        public string FilePath { get; private set; }
        public int ID { get; private set; }
        public ShaderType Type { get; private set; }

        public Source(string File, ShaderType Type)
        {
            this.FilePath = File;
            this.Type = Type;
        }

        public void LoadSource()
        {
            Screen.ValidateOpenGLUnsafe("Source.LoadSource", true);

            if (!File.Exists(FilePath))
            {
                TryResourceLoad();
                return;
            }
            SourceCode = File.ReadAllText(FilePath);
        }

        private void TryResourceLoad()
        {
            Assembly extender = Assembly.GetEntryAssembly();
            Assembly sharp2d = this.GetType().Assembly;

            Stream stream = extender.GetManifestResourceStream(FilePath);
            if (stream == null)
                stream = sharp2d.GetManifestResourceStream(FilePath);

            byte[] buffer = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int length;
                while ((length = stream.Read(buffer, 0, 1024)) > 0)
                {
                    ms.Write(buffer, 0, length);
                    buffer = new byte[1024];
                }
                
                ms.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(ms))
                {
                    SourceCode = reader.ReadToEnd();
                }
            }
            stream.Close();
            stream.Dispose();
        }

        public void Create()
        {
            Screen.ValidateOpenGLSafe("Source.Create");

            ID = GL.CreateShader(Type);
            GL.ShaderSource(ID, SourceCode);
            GL.CompileShader(ID);
            Logger.Debug(GL.GetShaderInfoLog(ID));
        }

        public void Reload()
        {

        }

        public void Delete()
        {

        }
    }
}
