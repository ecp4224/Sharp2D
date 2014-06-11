using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;
using OpenTK;
using Sharp2D.Core.Logic;

namespace Sharp2D.Game.Sprites
{
    public abstract class Sprite : IDisposable
    {
        ~Sprite()
        {
            Dispose();
        }

        internal bool FirstRun = true;
        public bool Loaded { get; private set; }
        public Texture Texture { get; set; }
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
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsOffScreen
        {
            get
            {
                return (X + Width) - Screen.Camera.X < -32 || Math.Abs(Screen.Camera.X - (X - Width)) > 32 + (Screen.Settings.GameSize.Width / Screen.Camera.Z) || (Y + Height) - Screen.Camera.Y < -32 || Math.Abs(Screen.Camera.Y - (Y - Height)) > 32 + (Screen.Settings.GameSize.Height / Screen.Camera.Z);

            }
        }
        public virtual float X { get; set; }

        public virtual float Y { get; set; }

        public void Load()
        {
            OnLoad();
            if (TexCoords == null && Texture != null)
            {
                TexCoords = new Sharp2D.Core.Utils.TexCoords(Width, Height, Texture);
            }
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

        protected abstract void OnLoad();

        protected abstract void OnUnload();

        protected abstract void OnDispose();

        protected abstract void OnDisplay();

    }
}