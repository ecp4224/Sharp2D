using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Game.Sprites;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Game.Worlds
{
    public class DrawBatch : SpriteBatch
    {
        public List<Sprite> AlphaSprites = new List<Sprite>();
        public int Type;
        private int _drawCount;
        public int DrawCount
        {
            get {
                return Type == 0 ? Count : _drawCount;
            }
        }

        public override int Count
        {
            get {
                return Type != 2 ? base.Count : AlphaSprites.Count;
            }
        }

        public override void ForEach(Action<Shader, Texture, Sprite> callBack)
        {
            if (Type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (var sprite in AlphaSprites)
                {
                    callBack(sprite.Shader, sprite.Texture, sprite);
                }
            }
        }

        public override void ForEach(Action<Sprite> callBack)
        {
            if (Type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (var sprite in AlphaSprites)
                {
                    callBack(sprite);
                }
            }
        }

        public override void Add(Sprite sprite)
        {
            if (Type != 2)
            {
                base.Add(sprite);
            }
            else
            {
                AlphaSprites.Add(sprite);
            }
            if (Type == 1)
                _drawCount += sprite.Lights.Count;
            else
                _drawCount += sprite.Lights.Count + 1;
        }

        public override void Remove(Sprite sprite)
        {
            if (Type != 2)
            {
                base.Remove(sprite);
            }
            else
            {
                AlphaSprites.Remove(sprite);
                Order();
            }
        }

        public void PrepareForDraw()
        {
            if (Type == 2) Order();
        }

        private void Order()
        {
            if (Type == 2)
            {
                AlphaSprites.Sort();
            }
        }
    }
    public class GenericRenderJob : SpriteRenderJob
    {
        
        internal object RenderLock = new object();

        public const int PosLocation = 0;
        public const int TexcoordLocation = 1;

        private int _vaoId;
        private int _vboId;
        private int _triId;
        private bool _gen;

        private readonly float[] _quadPoints =
        {
            -0.5f, -0.5f, 0.0f, 0.0f,   0.5f, -0.5f, 1.0f, 0.0f,   0.5f, 0.5f, 1.0f, 1.0f,   -0.5f, 0.5f, 0.0f, 1.0f
        };
        private readonly uint[] _rectangleindicies =
        {
            0, 1, 2, 0, 2, 3
        };
        private readonly GenericWorld _parent;

        public GenericWorld ParentWorld
        {
            get
            {
                return _parent;
            }
        }

        private static readonly DrawPass[] DefaultPasses =
        {
            new DrawNoAlpha(),
            new DrawNoAlphaLight(),
            new DrawAlpha()
        };

        private bool _passesDirty = true;
        private readonly List<DrawPass> _drawPasses = new List<DrawPass>(DefaultPasses.ToList());

        public GenericRenderJob(GenericWorld parent)
        {
            Screen.Camera = new OpenGL3Camera();
            _parent = parent;
        }

        private void CullLights(Sprite sprite)
        {
            if (sprite.IgnoreLights)
            {
                sprite.dynamicLights.Clear();
                sprite.Lights.Clear();
                return;
            }

            float sx = sprite.X;
            float sy = sprite.Y;
            float sw = sprite.Width;
            float sh = sprite.Height;

            foreach (Light light in _parent.dynamicLights)
            {
                if (light.Intensity == 0 || light.Radius == 0)
                    continue;

                float Y = light.Y + 18f;
                float xmin = light.X - (light.Radius) - 8;
                float xmax = light.X + (light.Radius) + 8;
                float ymin = Y - (light.Radius) - 8;
                float ymax = Y + (light.Radius) + 8;
                if (sx + (sw / 2f) >= xmin && sx - (sw / 2f) <= xmax && sy + (sh / 2f) >= ymin && sy - (sh / 2f) <= ymax)
                {
                    sprite.dynamicLights.Add(light);
                }
            }
            if (sprite.IsStatic) return;
            foreach (Light light in _parent.lights)
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

        public void RegisterDrawPass(DrawPass pass)
        {
            lock (RenderLock)
            {
                if (!_drawPasses.Contains(pass))
                {
                    _drawPasses.Add(pass);
                    _passesDirty = true;
                }
            }
        }

        DrawBatch[] CreateCulledBatches()
        {
            var batches = new DrawBatch[_drawPasses.Count];
            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = new DrawBatch();
                DrawBatch batch = batches[i];
                DrawPass pass = _drawPasses[i];

                pass.SetupBatch(ref batch);
            }

            Batch.ForEach(delegate(Sprite sprite)
            {
                if (sprite.IsOffScreen || !sprite.IsVisible) return;
                CullLights(sprite);
                for (int i = 0; i < batches.Length; i++)
                {
                    if (_drawPasses[i].MeetsRequirements(sprite))
                        batches[i].Add(sprite);
                }
            });

            float cx = -Screen.Camera.X;
            float cy = Screen.Camera.Y;

            float cullWidth = 380f * (Screen.Camera.Z / 100f);
            float cullHeight = 256f * (Screen.Camera.Z / 100f);
            cullWidth *= (Screen.Settings.GameSize.Width / 1024f);
            cullHeight *= (Screen.Settings.GameSize.Height / 720f);
            cullWidth /= 2f;
            cullHeight /= 2f;
            foreach (Layer layer in _parent.Layers)
            {
                if (!layer.IsTileLayer)
                    continue;
                float ex = cx + (cullWidth + (3f * _parent.TileWidth));
                float ey = cy + cullHeight;
                float sx = cx - cullWidth;
                float sy = cy - cullHeight;

                int sIx = Math.Max((int)(sx / _parent.TileWidth), 0);
                int sIy = Math.Max((int)Math.Ceiling((sy - 8f) / _parent.TileHeight), 0);

                int eIx = Math.Max((int)(ex / _parent.TileWidth), 0);
                int eIy = Math.Max((int)Math.Ceiling((ey - 8f) / _parent.TileHeight), 0);


                for (int x = sIx; x <= eIx; x++)
                {
                    for (int y = sIy; y < eIy; y++)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null)
                            continue;

                        CullLights(sprite);
                        for (int i = 0; i < batches.Length; i++)
                        {
                            if (_drawPasses[i].MeetsRequirements(sprite))
                                batches[i].Add(sprite);
                        }
                    }
                }
            }

            return batches;
        }

        public override void PerformJob()
        {
            lock (RenderLock)
            {
                if (!_gen)
                {
                    _gen = true;
                    OnFirstRun();
                }

                if (_passesDirty)
                {
                    _drawPasses.Sort();
                    foreach (DrawPass pass in _drawPasses.Where(pass => !pass.Initialized))
                    {
                        pass.Init(this);
                    }
                    _passesDirty = false;
                }

                DrawBatch[] batches = CreateCulledBatches();

                for (int i = 0; i < batches.Length; i++)
                {
                    DrawBatch batch = batches[i];
                    DrawPass pass = _drawPasses[i];

                    batch.PrepareForDraw();
                    pass.PrepareForDraw();

                    batch.ForEach(pass.DrawSprite);

                    pass.PostDraw();

                    batch.Dispose();
                }
            }
        }

        protected void OnFirstRun()
        {
            Screen.ValidateOpenGLSafe("CreateVBOs");


            GL.GenVertexArrays(1, out _vaoId);

            GL.BindVertexArray(_vaoId);

            _vboId = GL.GenBuffer();
            _triId = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
            unsafe
            {
                fixed (float* data = _quadPoints)
                {
                    fixed (uint* tdata = _rectangleindicies)
                    {
                        IntPtr point = (IntPtr)data;
                        IntPtr tpoint = (IntPtr)tdata;
                        IntPtr size = (IntPtr)(4 * 4 * sizeof(float));
                        IntPtr tsize = (IntPtr)(6 * sizeof(uint));

                        GL.BufferData(BufferTarget.ArrayBuffer, size, point, BufferUsageHint.StaticDraw); //TODO Maybe don't use static draw

                        GL.EnableVertexAttribArray(PosLocation);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(PosLocation, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(0));

                        GL.EnableVertexAttribArray(TexcoordLocation);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(TexcoordLocation, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _triId);
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

        public override bool IsOutsideCamera(float X, float Y, float Width, float Height, float Scale)
        {
            Y = -Y;

            var aspect = Screen.Settings.WindowAspectRatio;
            var tempPos = new Vector2(X, Y);
            var tempSize = new Vector2(Width, Height);

            tempPos = tempPos + (tempSize * Scale);

            tempPos += new Vector2(this.X, this.Y);
            tempPos *= (1f/this.Z);

            tempPos.X /= (aspect.X/aspect.Y);

            return tempPos.X < 0 || tempPos.X > 1 ||
                   tempPos.Y < 0 || tempPos.Y > 1;
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

        internal void Init(GenericRenderJob parent)
        {
            ParentJob = parent;
            OnInit();
            Initialized = true;
        }

        public bool Initialized { get; private set; }

        public GenericRenderJob ParentJob { get; private set; }

        public int CompareTo(DrawPass pass)
        {
            return pass == null ? 1 : Order.CompareTo(pass.Order);
        }
    }
}
