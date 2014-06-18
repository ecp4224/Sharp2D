using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Core.Graphics
{
    public class VertexArrayObject
    {
        public int ID { get; private set; }

        public void CreateAndBind()
        {
            ID = GL.GenVertexArray();
            GL.BindVertexArray(ID);
        }
    }
}
