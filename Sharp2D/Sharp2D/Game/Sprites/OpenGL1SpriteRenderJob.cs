using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Game.Sprites
{
    public class OpenGL1SpriteRenderJob : SpriteRenderJob
    {
        public override void PerformJob()
        {
            lock (group_lock)
            {
                foreach (Texture texture in groups.Keys)
                {
                    texture.Bind();
                    List<Sprite> group = groups[texture];
                    foreach (Sprite s in group)
                    {
                        if (s.IsOffScreen)
                            continue;

                        float bx = s.Width / 2f;
                        float by = s.Height / 2f;
                        float z = 0f;
                        float x = s.X;
                        float y = s.Y;

                        GL.Begin(PrimitiveType.Quads);
                        GL.TexCoord2(s.TexCoords.X, s.TexCoords.Y);
                        GL.Vertex3(x - bx, y - by, z);
                        GL.TexCoord2(s.TexCoords.Width, s.TexCoords.Y);
                        GL.Vertex3(x + bx, y - by, z);
                        GL.TexCoord2(s.TexCoords.Width, s.TexCoords.Height);
                        GL.Vertex3(x + bx, y + by, z);
                        GL.TexCoord2(s.TexCoords.X, s.TexCoords.Height);
                        GL.Vertex3(x - bx, y + by, z);
                        GL.End();

                    }
                }
            }
        }
    }
}