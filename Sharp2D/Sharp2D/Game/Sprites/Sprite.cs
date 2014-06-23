using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;
using OpenTK;
using Sharp2D.Core.Logic;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Game.Sprites
{
    public abstract class Sprite : IDisposable
    {
        ~Sprite()
        {
            Sharp2D.Core.Utils.Logger.Debug("Finalized called for Sprite " + this.ToString());
            Dispose();
        }

        internal bool FirstRun = true;
        public bool Loaded { get; private set; }
        public bool Visible { get; set; }
        private Texture _texture;
        private Shader _shader;
        public List<SpriteRenderJob> ContainingJobs
        {
            get
            {
                World current = CurrentWorld;
                if (current is Game.Worlds.SpriteWorld)
                {
                    List<SpriteRenderJob> toReturn = new List<SpriteRenderJob>();
                    List<SpriteRenderJob> jobs = ((Game.Worlds.SpriteWorld)current).SpriteRenderJobs;
                    foreach (SpriteRenderJob job in jobs)
                    {
                        if (job.HasSprite(this))
                            toReturn.Add(job);
                    }

                    return toReturn;
                }
                return new List<SpriteRenderJob>();
            }
        }
        public Shader Shader
        {
            get
            {
                return _shader;
            }
            set
            {
                List<SpriteRenderJob> temp = ContainingJobs;

                foreach (SpriteRenderJob job in temp) job.RemoveSprite(this);

                _shader = value;

                foreach (SpriteRenderJob job in temp) job.AddSprite(this);
            }
        }
        public Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                List<SpriteRenderJob> temp = ContainingJobs;

                foreach (SpriteRenderJob job in temp) job.RemoveSprite(this);

                _texture = value;

                foreach (SpriteRenderJob job in temp) job.AddSprite(this);
            }
        }
        public Vector2 Position { get { return new Vector2(X, Y); } }
        internal readonly List<World> _worlds = new List<World>();
        public IList<World> ContainingWorlds
        {
            get
            {
                return _worlds.AsReadOnly();
            }
        }
        /// <summary>
        /// Get the world the sprite is currently in. This world is the current World being displayed on the screen. To get all the Worlds this sprite is in, use ContainingWorlds
        /// </summary>
        public World CurrentWorld
        {
            get
            {
                if (ContainingWorlds.Count == 0)
                    return null;
                return ContainingWorlds.Where(w => w.Displaying).First();
            }
        }
        public TexCoords TexCoords;
        private float _rot;
        public float Rotation
        {
            get { return _rot; }
            set
            {
                while (value > 360f)
                    value -= 360f;
                while (value < 0f)
                    value += 360f;

                _rot = value;
            }
        }
        public virtual float Width { get; set; }
        public virtual float Height { get; set; }
        public bool IsOffScreen
        {
            get
            {
                return Screen.Camera.IsOutsideCamera(X, Y, Width, Height);
            }
        }
        public virtual float X { get; set; }

        public virtual float Y { get; set; }

        public void Load()
        {
            Visible = true;

            OnLoad();
            if (TexCoords == null && Texture != null)
            {
                TexCoords = new Sharp2D.Core.Utils.TexCoords(Width, Height, Texture);
            }
            else if (Texture == null)
                throw new InvalidOperationException("This sprite has no texture! A sprite MUST have a texture!");
            Loaded = true;
        }

        public void Unload()
        {
            OnUnload();
            Loaded = false;
        }

        public void Dispose()
        {
            if (Loaded)
                Unload();
            OnDispose();
        }

        public void Display()
        {
            OnDisplay();
            if (Texture != null && Texture.ID == -1)
                Texture.Create();
        }

        public void PrepareDraw()
        {
            BeforeDraw();
        }

        public virtual void OnAddedToWorld(World w)
        {

        }

        public virtual void OnRemovedFromWorld(World w)
        {

        }

        protected abstract void BeforeDraw();

        protected abstract void OnLoad();

        protected abstract void OnUnload();

        protected abstract void OnDispose();

        protected abstract void OnDisplay();

    }
}