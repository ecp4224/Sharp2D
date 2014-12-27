using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core;
using Sharp2D.Game.Sprites;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Game.Worlds
{
    public abstract class GenericRenderJob : BatchRenderJob
    {
        
        internal static object RenderLock = new object();

        public const int PosLocation = 0;
        public const int TexcoordLocation = 1;

        internal static int VaoId = -1;
        internal static int VboId = -1;
        internal static int TriId = -1;
        private static bool _gen;

        private static readonly float[] QuadPoints =
        {
            -0.5f, -0.5f, 0.0f, 0.0f,   0.5f, -0.5f, 1.0f, 0.0f,   0.5f, 0.5f, 1.0f, 1.0f,   -0.5f, 0.5f, 0.0f, 1.0f
        };
        private static readonly uint[] Rectangleindicies =
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

        public abstract DrawPass[] DefaultPasses { get; }
        private bool _passesDirty = true;
        private readonly List<DrawPass> _drawPasses = new List<DrawPass>();

        public List<DrawPass> DrawPasses
        {
            get { return _drawPasses; }
        } 

        protected GenericRenderJob(GenericWorld parent)
        {
            Screen.Camera = new OpenGL3Camera();
            _parent = parent;
            _drawPasses.AddRange(DefaultPasses.ToList());
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

        protected abstract SpriteBatch[] CreateCulledBatches();

        internal static void CheckVBO()
        {
            if (!_gen)
            {
                _gen = true;
                OnFirstRun();
            }
        }

        public override void PerformJob()
        {
            lock (RenderLock)
            {
                CheckVBO();

                if (_passesDirty)
                {
                    _drawPasses.Sort();
                    foreach (DrawPass pass in _drawPasses.Where(pass => !pass.Initialized))
                    {
                        pass.Init(this);
                    }
                    _passesDirty = false;
                }

                SpriteBatch[] batches = CreateCulledBatches();

                for (int i = 0; i < batches.Length; i++)
                {
                    SpriteBatch batch = batches[i];
                    DrawPass pass = _drawPasses[i];

                    batch.PrepareForDraw();
                    pass.PrepareForDraw();

                    batch.ForEach(pass.DrawSprite);

                    pass.PostDraw();

                    batch.Dispose();
                }
            }
        }

        protected static void OnFirstRun()
        {
            GL.GenVertexArrays(1, out VaoId);

            GL.BindVertexArray(VaoId);

            VboId = GL.GenBuffer();
            TriId = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboId);
            unsafe
            {
                fixed (float* data = QuadPoints)
                {
                    fixed (uint* tdata = Rectangleindicies)
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

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, TriId);
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

        public override bool IsOutsideCamera(float x, float y, float width, float height, float scale = 1)
        {
            y = -y;

            var aspect = Screen.Settings.WindowAspectRatio;
            var tempPos = new Vector2(x, y);
            var tempSize = new Vector2(width, height);

            tempPos = tempPos + (tempSize * scale);

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

        public abstract void SetupBatch(SpriteBatch batch);

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
