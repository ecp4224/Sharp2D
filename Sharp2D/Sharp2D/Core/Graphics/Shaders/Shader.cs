using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Sharp2D.Core.Graphics.Shaders
{
    public class Shader
    {
        public delegate void ShaderEvent(Shader shader);
        public event ShaderEvent OnShaderBound;
        public event ShaderEvent OnShaderCompiled;
        public event ShaderEvent OnShaderLinked;

        private List<Source> _sources = new List<Source>();
        public IList<Source> Sources
        {
            get
            {
                return _sources.AsReadOnly();
            }
        }
        
        public bool IsActive { get; private set; }
        public bool IsLinked { get; private set; }
        public bool IsLoaded { get; private set; }
        public bool IsCompiled { get; private set; }

        private UniformHolder _holder;
        public UniformHolder Uniforms
        {
            get
            {
                if (!IsActive)
                    throw new InvalidOperationException("This shader is not in use!");

                return _holder;
            }
            private set
            {
                this._holder = value;
            }
        }

        public int ProgramID { get; private set; }

        public Shader(params string[] files)
        {
            foreach (string f in files)
            {
                ShaderType type;
                if (f.EndsWith(".frag"))
                    type = ShaderType.FragmentShader;
                else if (f.EndsWith(".vert"))
                    type = ShaderType.VertexShader;
                else if (f.EndsWith(".cpu"))
                    type = ShaderType.ComputeShader;
                else
                    throw new FileLoadException("Not a valid shader file!", f);

                var source = new Source(f, type);

                _sources.Add(source);
            }
        }

        public Shader(params Source[] sources)
        {
            _sources.AddRange(sources);
        }

        public void LoadAll()
        {
            _sources.ForEach(s => s.LoadSource());
            IsLoaded = true;
        }

        public void CompileAll()
        {
            _sources.ForEach(s => s.Create());
            IsCompiled = true;
            if (OnShaderCompiled != null)
                OnShaderCompiled(this);
        }

        public void LinkAll()
        {
            Screen.ValidateOpenGLSafe("Shader.LinkALL");

            ProgramID = GL.CreateProgram();

            _sources.ForEach(s => GL.AttachShader(ProgramID, s.ID));

            GL.LinkProgram(ProgramID);

            int status;
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                throw new InvalidProgramException("Linking the program failed!\n" + GL.GetShaderInfoLog(ProgramID));
            }
            Logger.Debug(GL.GetProgramInfoLog(ProgramID));

            Uniforms = new UniformHolder(ProgramID);

            int ucount, acount;
            GL.GetProgram(ProgramID, ProgramParameter.ActiveUniforms, out ucount);
            GL.GetProgram(ProgramID, ProgramParameter.ActiveAttributes, out acount);



            /*for (int i = 0; i < ucount; i++)
            {
                int length, size;
                ActiveUniformType type;
                StringBuilder name = new StringBuilder();
                GL.GetActiveUniform(ProgramID, i, 256, out length, out size, out type, name);

                _holder.locations.Add(name.ToString(), GL.GetUniformLocation(ProgramID, name.ToString()));
            }*/

            IsCompiled = true;

            if (OnShaderLinked != null)
                OnShaderLinked(this);
        }

        private static Shader _currentShader;
        public void Use()
        {

            if (OnShaderBound != null)
                OnShaderBound(this);

            if (this == _currentShader)
                return;

            GL.UseProgram(ProgramID);
            IsActive = true;

            if (_currentShader != null)
                _currentShader.IsActive = false;
            
            _currentShader = this;
        }

        public void Reload()
        {

        }
    }
}
