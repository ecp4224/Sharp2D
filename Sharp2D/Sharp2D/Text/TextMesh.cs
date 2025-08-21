using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Sharp2D.Text
{
    /// <summary>
    /// Holds dynamic vertex data for a piece of text.
    /// </summary>
    public class TextMesh : IDisposable
    {
        public int Vao { get; private set; }
        public int Vbo { get; private set; }
        public int VertexCount { get; private set; }
        public SdfFont Font { get; private set; }
        private string _text = string.Empty;

        public void SetText(SdfFont font, string text, Vector2 origin, float scale, bool useKerning)
        {
            Font = font;
            _text = text;
            var layout = TextLayout.Build(font, text, origin, scale, useKerning);
            VertexCount = layout.QuadCount * 6;

            if (Vao == 0)
            {
                GL.GenVertexArrays(1, out int vao);
                GL.GenBuffers(1, out int vbo);
                Vao = vao;
                Vbo = vbo;
                GL.BindVertexArray(Vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            }
            GL.BindVertexArray(Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, layout.Vertices.Length * sizeof(float), layout.Vertices, BufferUsageHint.DynamicDraw);
        }

        public string CurrentText => _text;

        public void Dispose()
        {
            if (Vbo != 0)
            {
                GL.DeleteBuffer(Vbo);
                Vbo = 0;
            }
            if (Vao != 0)
            {
                GL.DeleteVertexArray(Vao);
                Vao = 0;
            }
        }
    }
}
