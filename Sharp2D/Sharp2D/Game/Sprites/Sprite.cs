﻿using System;
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
    /// <summary>
    /// <para>A Sprite is an object that can be drawn by a <see cref="SpriteRenderJob"/>.</para>
    /// <para>A Sprite is a quad that can any width or height, but ALWAYS has a texture</para>
    /// </summary>
    public abstract partial class Sprite : IDisposable
    {
        ~Sprite()
        {
            Dispose();
        }

        internal bool FirstRun = true;
        
        /// <summary>
        /// Whether or not this Sprite has loaded
        /// </summary>
        public bool Loaded { get; private set; }
        
        /// <summary>
        /// Whether or not this Sprite is visible
        /// </summary>
        public bool Visible { get; set; }

        private Texture _texture;
        private Shader _shader;
        
        /// <summary>
        /// The <see cref="SpriteRenderJob"/>'s this Sprite belongs to.
        /// </summary>
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
        
        /// <summary>
        /// The Shader object this Sprite uses.
        /// </summary>
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

        /// <summary>
        /// The Texture object this Sprite is using.
        /// </summary>
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

        /// <summary>
        /// The position of this Sprite in the currently displaying world represented as a <see cref="Vector2"/>
        /// </summary>
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

        /// <summary>
        /// The current rotation of this Sprite in degrees. This value will always be between 0 - 360
        /// </summary>
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

        /// <summary>
        /// The width of this Sprite
        /// </summary>
        public virtual float Width { get; set; }

        /// <summary>
        /// The height of this Sprite
        /// </summary>
        public virtual float Height { get; set; }

        /// <summary>
        /// Whether or not this Sprite is off screen
        /// </summary>
        public bool IsOffScreen
        {
            get
            {
                return Screen.Camera.IsOutsideCamera(X, Y, Width, Height);
            }
        }

        /// <summary>
        /// The X coordinate of this Sprite in the currently displaying world
        /// </summary>
        public virtual float X { get; set; }

        /// <summary>
        /// The Y coordinate of this Sprite in the currently displaying world
        /// </summary>
        public virtual float Y { get; set; }

        /// <summary>
        /// The Layer this Sprite lives on in the currently displaying world. Note: Some <see cref="SpriteRenderJob"/>'s don't implement this variable
        /// </summary>
        public virtual float Layer { get; set; }

        /// <summary>
        /// Request the RenderJob to run the OnDisplay method again
        /// </summary>
        protected void RequestOnDisplay()
        {
            FirstRun = true;
        }

        /// <summary>
        /// Load the Sprite
        /// </summary>
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

        /// <summary>
        /// Unload the sprite
        /// </summary>
        public void Unload()
        {
            OnUnload();
            Loaded = false;
        }

        /// <summary>
        /// Dispose the sprite
        /// </summary>
        public void Dispose()
        {
            if (Loaded)
                Unload();
            OnDispose();
        }

        /// <summary>
        /// Display the Sprite
        /// </summary>
        public void Display()
        {
            OnDisplay();
            if (Texture != null && Texture.ID == -1)
                Texture.Create();
        }

        /// <summary>
        /// Prepare the Sprite to be drawn onto the screen
        /// </summary>
        public void PrepareDraw()
        {
            BeforeDraw();
        }

        /// <summary>
        /// This method is called when this Sprite has been added to a World
        /// </summary>
        /// <param name="w">The world this Sprite has been added to</param>
        public virtual void OnAddedToWorld(World w)
        {

        }

        /// <summary>
        /// This method is called when this Sprite has been removed from a World
        /// </summary>
        /// <param name="w">The world this Sprite has been removed from</param>
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