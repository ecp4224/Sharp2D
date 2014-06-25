using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Game.Sprites
{
    public class OpenGL3SpriteRenderJob : SpriteRenderJob
    {
        private const int POS_LOCATION = 0;
        private const int TEXCOORD_LOCATION = 1;

        private static Shader shader;

        private int vao_id;
        private int vbo_id;
        private int tri_id;
        private bool gen;
        private float[] quad_points = new float[] 
        {
            0.0f, 0.0f, 0.0f, 0.0f,   1.0f, 0.0f, 1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f, 1.0f, 0.0f, 1.0f
        };
        private uint[] rectangleindicies = new uint[] 
        {
            0, 1, 2, 0, 2, 3
        };

        public OpenGL3SpriteRenderJob()
        {
            Screen.Camera = new OpenGL3Camera();
        }

        public override void PerformJob()
        {
            if (!gen)
            {
                gen = true;
                CreateVBOs();
            }
            lock (group_lock)
            {
                GL.BindVertexArray(vao_id);
                
                foreach (ShaderGroup s in group)
                {
                    if (s.key != null)
                    {
                        s.key.Use();

                        if (s.key.ProgramID == shader.ProgramID) //If this shader is our shader
                        {
                            shader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), shader.Uniforms["camPosAndScale"]);
                            Vector2 aspect = Screen.Settings.WindowAspectRatio;
                            shader.Uniforms.SetUniform(aspect.X / aspect.Y, shader.Uniforms["screenRatioFix"]);
                        }
                    }
                    else
                    {
                        s.key = shader; //Always default..ALWAYS

                        s.key.Use();

                        if (s.key.ProgramID == shader.ProgramID) //If this shader is our shader
                        {
                            shader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), shader.Uniforms["camPosAndScale"]);
                            Vector2 aspect = Screen.Settings.WindowAspectRatio;
                            shader.Uniforms.SetUniform(aspect.X / aspect.Y, shader.Uniforms["screenRatioFix"]);
                        }
                    }

                    foreach (TextureGroup t in s.group)
                    {
                        if (t.key != null)
                            t.key.Bind();

                        foreach (Sprite sprite in t.group)
                        {
                            if (sprite.IsOffScreen || !sprite.Visible)
                                continue;

                            if (sprite.FirstRun)
                            {
                                sprite.Display();
                                sprite.FirstRun = false;
                            }

                            sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

                            if (s.key != null && s.key.ProgramID == shader.ProgramID) //If this sprite is using our ID
                            {
                                shader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, sprite.Width, sprite.Height), shader.Uniforms["spritePos"]);
                                float tsize = sprite.TexCoords.SquardSize;
                                shader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), shader.Uniforms["texCoordPosAndScale"]);
                            }

                            //GL.DrawArrays(BeginMode.Quads, 0, 4);
                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }
                    }
                }
            }
        }

        private void CreateVBOs()
        {
            Screen.ValidateOpenGLSafe("CreateVBOs");


            GL.GenVertexArrays(1, out vao_id);

            GL.BindVertexArray(vao_id);

            vbo_id = GL.GenBuffer();
            tri_id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_id);
            unsafe
            {
                fixed (float* data = quad_points)
                {
                    fixed (uint* tdata = rectangleindicies)
                    {
                        IntPtr point = (IntPtr)data;
                        IntPtr tpoint = (IntPtr)tdata;
                        IntPtr size = (IntPtr)(4 * 4 * sizeof(float));
                        IntPtr tsize = (IntPtr)(6 * sizeof(uint));

                        GL.BufferData(BufferTarget.ArrayBuffer, size, point, BufferUsageHint.StaticDraw); //TODO Maybe don't use static draw

                        GL.EnableVertexAttribArray(POS_LOCATION);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(POS_LOCATION, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(0));

                        GL.EnableVertexAttribArray(TEXCOORD_LOCATION);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(TEXCOORD_LOCATION, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, tri_id);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, tsize, tpoint, BufferUsageHint.StaticDraw); //TODO Maybe don't use static draw
                    }
                }
            }

            if (shader == null)
            {
                shader = new Shader("shaders/sprite.vert", "shaders/sprite.frag"); //TODO Change files

                shader.LoadAll();
                shader.CompileAll();
                GL.BindAttribLocation(shader.ProgramID, POS_LOCATION, "posattrib");
                GL.BindAttribLocation(shader.ProgramID, TEXCOORD_LOCATION, "tcattrib");
                shader.LinkAll();
            }

            List<Sprite> sprites = Sprites;
            foreach (Sprite sprite in sprites)
            {
                if (sprite.Shader == null)
                    sprite.Shader = shader;
            }
        }

        public override void AddSprite(Sprite sprite)
        {
            if (sprite.Shader == null)
                sprite.Shader = shader;

            base.AddSprite(sprite); 
        }
    }
}
