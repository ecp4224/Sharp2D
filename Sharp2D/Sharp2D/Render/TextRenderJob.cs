using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Sharp2D.Core.Graphics.Shaders;
using Sharp2D.Core.Interfaces;
using Sharp2D.Fonts;
using Sharp2D.Text;

namespace Sharp2D.Render
{
    /// <summary>
    /// Render job that draws <see cref="TextSprite"/> objects using SDF shaders.
    /// </summary>
    public class TextRenderJob : IRenderJob
    {
        private readonly List<TextSprite> _sprites = new List<TextSprite>();
        private readonly Shader _shader;
        private readonly GenericWorld _world;
        private bool _shaderInit;

        public TextRenderJob(GenericWorld world)
        {
            _world = world;
            _shader = new Shader("Sharp2D.Resources.sdf_text.vert", "Sharp2D.Resources.sdf_text.frag");
        }

        public void Add(TextSprite sprite)
        {
            if (!_sprites.Contains(sprite))
                _sprites.Add(sprite);
        }

        public void Remove(TextSprite sprite)
        {
            _sprites.Remove(sprite);
        }

        public void PerformJob()
        {
            if (_sprites.Count == 0) return;

            if (!_shaderInit)
            {
                _shader.LoadAll();
                _shader.CompileAll();
                _shader.LinkAll();

                _shaderInit = true;
            }

            GL.DepthMask(false);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader.Use();
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, _world.PixelWidth, 0, _world.PixelHeight, -1, 1);
            _shader.Uniforms.SetUniform(proj, _shader.Uniforms["u_projection"]);
            _shader.Uniforms.SetUniform(0, _shader.Uniforms["u_texture"]);

            foreach (var group in GroupByFont())
            {
                SdfFont font = group.Key;
                GL.ActiveTexture(TextureUnit.Texture0);
                font.AtlasTexture.Bind();

                foreach (var sprite in group.Value)
                {
                    var col = new Vector4(sprite.Color.Red / 255f, sprite.Color.Green / 255f, sprite.Color.Blue / 255f, sprite.Color.Alpha / 255f);
                    _shader.Uniforms.SetUniform(col, _shader.Uniforms["u_textColor"]);
                    _shader.Uniforms.SetUniform(sprite.Threshold, _shader.Uniforms["u_threshold"]);
                    _shader.Uniforms.SetUniform(sprite.Smoothing, _shader.Uniforms["u_smoothing"]);

                    GL.BindVertexArray(sprite.Mesh.Vao);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, sprite.Mesh.VertexCount);
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }

        private Dictionary<SdfFont, List<TextSprite>> GroupByFont()
        {
            var dict = new Dictionary<SdfFont, List<TextSprite>>();
            foreach (var s in _sprites)
            {
                if (!dict.TryGetValue(s.Font, out var list))
                {
                    list = new List<TextSprite>();
                    dict[s.Font] = list;
                }
                list.Add(s);
            }
            return dict;
        }

        public void Dispose()
        {
            foreach (var s in _sprites)
            {
                s.Mesh.Dispose();
            }
            _sprites.Clear();
        }
    }
}
