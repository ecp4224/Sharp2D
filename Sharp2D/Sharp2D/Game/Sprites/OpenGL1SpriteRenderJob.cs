using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Sharp2D.Core.Graphics;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core.Physics;
using Sharp2D.Core.Graphics.Shaders;

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

                        if (s.FirstRun)
                        {
                            s.Display();
                            s.FirstRun = false;
                        }

                        float bx = s.Width / 2f;
                        float by = s.Height / 2f;
                        float z = 0;
                        float x = s.X;
                        float y = s.Y;

                        GL.Begin(PrimitiveType.Quads);

                        GL.TexCoord2(s.TexCoords.TopLeft.X, s.TexCoords.TopLeft.Y);
                        GL.Vertex3(x - bx, y - by, z);
                        GL.TexCoord2(s.TexCoords.TopRight.X, s.TexCoords.TopRight.Y);
                        GL.Vertex3(x + bx, y - by, z);
                        GL.TexCoord2(s.TexCoords.BottomRight.X, s.TexCoords.BottomRight.Y);
                        GL.Vertex3(x + bx, y + by, z);
                        GL.TexCoord2(s.TexCoords.BottomLeft.X, s.TexCoords.BottomLeft.Y);
                        GL.Vertex3(x - bx, y + by, z);
                        GL.End();

                        /*if (s is PhysicsSprite)
                        {
                            var h = (s as PhysicsSprite).Hitbox.GetRelativeHitbox(s as PhysicsSprite);

                            GL.LineWidth(2f);
                            GL.Color3(200f / 255f, 30f / 255f, 30f / 255f);
                            GL.Begin(PrimitiveType.LineLoop);
                            foreach (var v in h.Vertices)
                            {
                                GL.Vertex3(v.X, v.Y, 0);
                            }
                            GL.End();

                            GL.Color3(230f / 255f, 90f / 255f, 250f / 255f);
                            GL.Begin(PrimitiveType.LineLoop);
                            var i = 1f;
                            foreach (var v in h.ConstructEdgesRelative(s as PhysicsSprite))
                            {
                                i++;
                                GL.LineWidth(i);
                                GL.Vertex3(v.X, v.Y, 0);
                            }
                            GL.End();

                            GL.LineWidth(1f);
                            GL.Color3(1, 1, 1);
                        }*/
                    }
                }
            }
        }
    }
}