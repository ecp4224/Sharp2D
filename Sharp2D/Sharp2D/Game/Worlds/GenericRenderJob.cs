using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Sharp2D.Game.Sprites;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;
using System.Drawing;

namespace Sharp2D.Game.Worlds
{
    public class DrawBatch : SpriteBatch
    {
        public List<Sprite> alphaSprites = new List<Sprite>();
        public int type;
        private int drawCount;
        public int DrawCount
        {
            get
            {
                if (type == 0)
                    return Count;
                else
                    return drawCount;
            }
        }

        public override int Count
        {
            get
            {
                if (type != 2)
                {
                    return base.Count;
                }
                else
                {
                    return alphaSprites.Count;
                }
            }
        }

        public override void ForEach(Action<Shader, Texture, Sprite> callBack)
        {
            if (type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (Sprite sprite in alphaSprites)
                {
                    callBack(sprite.Shader, sprite.Texture, sprite);
                }
            }
        }

        public override void ForEach(Action<Sprite> callBack)
        {
            if (type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (Sprite sprite in alphaSprites)
                {
                    callBack(sprite);
                }
            }
        }

        public override void Add(Sprite sprite)
        {
            if (type != 2)
            {
                base.Add(sprite);
            }
            else
            {
                alphaSprites.Add(sprite);
                Order();
            }
            if (type == 1)
                drawCount += sprite.Lights.Count;
            else
                drawCount += sprite.Lights.Count + 1;
        }

        public override void Remove(Sprite sprite)
        {
            if (type != 2)
            {
                base.Remove(sprite);
            }
            else
            {
                alphaSprites.Remove(sprite);
                Order();
            }
        }

        private void Order()
        {
            if (type == 2)
            {
                alphaSprites.Sort();
            }
        }
    }
    public class GenericRenderJob : SpriteRenderJob
    {
        
        internal object render_lock = new object();

        public const int POS_LOCATION = 0;
        public const int TEXCOORD_LOCATION = 1;

        private int vao_id;
        private int vbo_id;
        private int tri_id;
        private bool gen;

        private float[] quad_points = new float[] 
        {
            -0.5f, -0.5f, 0.0f, 0.0f,   0.5f, -0.5f, 1.0f, 0.0f,   0.5f, 0.5f, 1.0f, 1.0f,   -0.5f, 0.5f, 0.0f, 1.0f
        };
        private uint[] rectangleindicies = new uint[] 
        {
            0, 1, 2, 0, 2, 3
        };
        private GenericWorld parent;

        public GenericWorld ParentWorld
        {
            get
            {
                return parent;
            }
        }

        private static readonly DrawPass[] DEFAULT_PASSES = new DrawPass[] {
            new DrawNoAlpha(),
            new DrawNoAlphaLight(),
            new DrawAlpha()
        };

        private bool passes_dirty = true;
        private List<DrawPass> DrawPasses = new List<DrawPass>(DEFAULT_PASSES.ToList<DrawPass>());

        public GenericRenderJob(GenericWorld parent)
        {
            Screen.Camera = new OpenGL3Camera();
            this.parent = parent;
        }

        private void CullLights(Sprite sprite)
        {
            if (sprite.IgnoreLights)
            {
                sprite.dynamicLights.Clear();
                sprite.Lights.Clear();
                return;
            }

            foreach (Light light in parent.dynamicLights)
            {
                if (light.Intensity == 0 || light.Radius == 0)
                    continue;
                float Y = light.Y + 18f;
                float xmin = light.X - (light.Radius) - 8;
                float xmax = light.X + (light.Radius) + 8;
                float ymin = Y - (light.Radius) - 8;
                float ymax = Y + (light.Radius) + 8;
                if (sprite.X + (sprite.Width / 2f) >= xmin && sprite.X - (sprite.Width / 2f) <= xmax && sprite.Y + (sprite.Height / 2f) >= ymin && sprite.Y - (sprite.Height / 2f) <= ymax)
                {
                    sprite.dynamicLights.Add(light);
                }
            }
            if (!sprite.IsStatic)
            {
                foreach (Light light in parent.lights)
                {
                    if (light.Intensity == 0 || light.Radius == 0)
                        continue;
                    float Y = light.Y + 18f;
                    float xmin = light.X - (light.Radius);
                    float xmax = light.X + (light.Radius);
                    float ymin = Y - (light.Radius);
                    float ymax = Y + (light.Radius);
                    if (sprite.X + (sprite.Width / 2f) >= xmin && sprite.X - (sprite.Width / 2f) <= xmax && sprite.Y + (sprite.Height / 2f) >= ymin && sprite.Y - (sprite.Height / 2f) <= ymax)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
        }

        public void RegisterDrawPass(DrawPass pass)
        {
            lock (render_lock)
            {
                if (!DrawPasses.Contains(pass))
                {
                    DrawPasses.Add(pass);
                    passes_dirty = true;
                }
            }
        }

        DrawBatch[] CreateCulledBatches()
        {
            DrawBatch[] batches = new DrawBatch[DrawPasses.Count];
            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = new DrawBatch();
                DrawBatch batch = batches[i];
                DrawPass pass = DrawPasses[i];

                pass.SetupBatch(ref batch);
            }

            Batch.ForEach(delegate(Sprite sprite)
            {
                if (!sprite.IsOffScreen && sprite.IsVisible)
                {
                    CullLights(sprite);
                    for (int i = 0; i < batches.Length; i++)
                    {
                        if (DrawPasses[i].MeetsRequirements(sprite))
                            batches[i].Add(sprite);
                    }
                }
            });

            float width = Screen.Settings.GameSize.Width;
            float height = Screen.Settings.GameSize.Height;

            float cx = -Screen.Camera.X;
            float cy = Screen.Camera.Y;

            float cull_width = 380f * (Screen.Camera.Z / 100f);
            float cull_height = 256f * (Screen.Camera.Z / 100f);
            cull_width *= (Screen.Settings.GameSize.Width / 1024);
            cull_height *= (Screen.Settings.GameSize.Height / 720);
            cull_width /= 2f;
            cull_height /= 2f;
            foreach (Layer layer in parent.Layers)
            {
                if (!layer.IsTileLayer)
                    continue;
                float ex = cx + (cull_width + (3f * 16f));
                float ey = cy + cull_height;
                float sx = cx - cull_width;
                float sy = cy - cull_height;

                int s_i_x = Math.Max((int)(sx / 16f), 0);
                int s_i_y = Math.Max((int)Math.Ceiling((sy - 8f) / 16f), 0);

                int e_i_x = Math.Max((int)(ex / 16f), 0);
                int e_i_y = Math.Max((int)Math.Ceiling((ey - 8f) / 16f), 0);


                for (int x = s_i_x; x <= e_i_x; x++)
                {
                    for (int y = s_i_y; y < e_i_y; y++)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null)
                            continue;

                        CullLights(sprite);
                        for (int i = 0; i < batches.Length; i++)
                        {
                            if (DrawPasses[i].MeetsRequirements(sprite))
                                batches[i].Add(sprite);
                        }
                    }
                }
            }

            return batches;
        }

        public override void PerformJob()
        {
            lock (render_lock)
            {
                if (!gen)
                {
                    gen = true;
                    OnFirstRun();
                }

                if (passes_dirty)
                {
                    DrawPasses.Sort();
                    foreach (DrawPass pass in DrawPasses)
                    {
                        if (!pass.Initialized)
                            pass.Init(this);
                    }
                    passes_dirty = false;
                }

                DrawBatch[] batches = CreateCulledBatches();

                for (int i = 0; i < batches.Length; i++)
                {
                    DrawBatch batch = batches[i];
                    DrawPass pass = DrawPasses[i];

                    pass.PrepareForDraw();

                    batch.ForEach(pass.DrawSprite);

                    pass.PostDraw();

                    batch.Clear();
                }

                batches = null;
            }
        }

        protected void OnFirstRun()
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
        }
    }
    
    public class OpenGL3Camera : Camera
    {
        public OpenGL3Camera()
        {
            Z = 1f;
        }

        public override bool IsOutsideCamera(float X, float Y, float Width, float Height)
        {
            Y = -Y;
            float temp = Screen.Camera.Z / 100f;
            float temp2 = 7f / temp;
            float temp3 = 64f * temp;
            return
                (X + Width) + Screen.Camera.X < -temp3 - (Screen.Settings.GameSize.Width / temp2) ||
                Screen.Camera.X + (X + Width) > temp3 + (Screen.Settings.GameSize.Width / temp2) ||
                (Y + Height) + Screen.Camera.Y < -temp3 - (Screen.Settings.GameSize.Height / temp2) ||
                Screen.Camera.Y + (Y + Height) > temp3 + (Screen.Settings.GameSize.Height / temp2);
        }
    }

    public abstract class DrawPass : IComparable<DrawPass>
    {
        public abstract bool MeetsRequirements(Sprite sprite);

        public abstract void DrawSprite(Shader shader, Texture texture, Sprite sprite);

        public abstract void PrepareForDraw();

        public abstract void PostDraw();

        public abstract void SetupBatch(ref DrawBatch batch);

        public abstract void OnInit();

        public abstract int Order { get; }

        private GenericRenderJob parent;

        internal void Init(GenericRenderJob parent)
        {
            this.parent = parent;
            OnInit();
            Initialized = true;
        }

        public bool Initialized { get; private set; }

        public GenericRenderJob ParentJob
        {
            get
            {
                return parent;
            }
        }

        public int CompareTo(DrawPass pass)
        {
            if (pass == null) return 1;

            return Order.CompareTo(pass.Order);
        }
    }
}
