using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.IO;

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

            SourceCode = File.ReadAllText(FilePath);
        }

        public void Create()
        {
            Screen.ValidateOpenGLSafe("Source.Create");

            ID = GL.CreateShader(Type);
            GL.ShaderSource(ID, SourceCode);
            GL.CompileShader(ID);
            Utils.Logger.Debug(GL.GetShaderInfoLog(ID));
        }
    }
}
