using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Game.Sprite
{
    public class OpenGL1SpriteRenderJob : IRenderJob
    {
        private Dictionary<Texture, List<Sprite>> groups = new Dictionary<Texture, List<Sprite>>();
        private object group_lock = new object();
        public void PerformJob()
        {
            lock (group_lock)
            {
                foreach (Texture texture in groups.Keys)
                {
                    texture.Bind();
                    foreach (Sprite s in groups[texture])
                    {
                        float bx = s.Width / 2f;
                        float by = s.Height / 2f;
                        float z = 0f;

                        GL.Translate(s.X, s.Y, 0f);

                        GL.Begin(BeginMode.Quads);
                        GL.TexCoord2(s.TexCoords.X, s.TexCoords.Y);
                        GL.Vertex3(-bx, -by, z);
                        GL.TexCoord2(s.TexCoords.Width, s.TexCoords.Y);
                        GL.Vertex3(bx, -by, z);
                        GL.TexCoord2(s.TexCoords.Width, s.TexCoords.Height);
                        GL.Vertex3(bx, by, z);
                        GL.TexCoord2(s.TexCoords.X, s.TexCoords.Height);
                        GL.Vertex3(-bx, by, z);
                        GL.End();
                    }
                }
            }
        }

        public void Dispose()
        {
            groups.Clear();
        }

        public void AddSprite(Sprite sprite)
        {
            lock (group_lock)
            {
                if (groups.ContainsKey(sprite.Texture))
                {
                    groups[sprite.Texture].Add(sprite);
                }
                else
                {
                    List<Sprite> new_group = new List<Sprite>();
                    new_group.Add(sprite);
                    groups.Add(sprite.Texture, new_group);
                }
            }
        }

        public void RemoveSprite(Sprite sprite)
        {
            lock (group_lock)
            {
                if (groups.ContainsKey(sprite.Texture))
                {
                    groups[sprite.Texture].Remove(sprite);
                }
            }
        }
    }
}
