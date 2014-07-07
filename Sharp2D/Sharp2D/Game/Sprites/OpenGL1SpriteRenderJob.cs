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
        public OpenGL1SpriteRenderJob()
        {
            Screen.Camera = new OpenGL1Camera();
        }

        public override void PerformJob()
        {
            lock (group_lock)
            {
                foreach (ShaderGroup sgroup in group)
                {
                    if (sgroup.key != null)
                        sgroup.key.Use();

                    foreach (TextureGroup tgroup in sgroup.group)
                    {
                        if (tgroup.key != null)
                            tgroup.key.Bind();

                        foreach (Sprite s in tgroup.group)
                        {
                            if (s.IsOffScreen || !s.Visible)
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
                            GL.Vertex3(x - bx, y + by, z);
                            GL.TexCoord2(s.TexCoords.BottomRight.X, s.TexCoords.BottomRight.Y);
                            GL.Vertex3(x + bx, y + by, z);
                            GL.TexCoord2(s.TexCoords.BottomLeft.X, s.TexCoords.BottomLeft.Y);
                            GL.Vertex3(x + bx, y - by, z);
                            GL.End();

                            if (s is ICollidable)
                            {
                                if ((s as ICollidable).Hitbox == null) { continue; }
                                var h = (s as ICollidable).Hitbox.GetRelativeHitbox(s as ICollidable);

                                GL.Color3(200f / 255f, 30f / 255f, 30f / 255f);
                                GL.Begin(PrimitiveType.LineLoop);
                                foreach (var v in h.Vertices)
                                {
                                    GL.Vertex3(v.X, v.Y, 0);
                                }
                                GL.End();
                                GL.Color3(1f, 1f, 1f);
                            }                                        
                        }
                    }
                }
            }
        }
    }
}