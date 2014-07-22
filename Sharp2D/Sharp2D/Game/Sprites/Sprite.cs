using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;
using OpenTK;
using Sharp2D.Core.Logic;
using Sharp2D.Game.Worlds;
using Sharp2D.Core.Graphics.Shaders;
using System.Drawing;

namespace Sharp2D.Game.Sprites
{
    /// <summary>
    /// <para>A Sprite is an object that can be drawn by a <see cref="SpriteRenderJob"/>.</para>
    /// <para>A Sprite is a quad that can any width or height, but ALWAYS has a texture</para>
    /// </summary>
    public abstract partial class Sprite : IDisposable, IAttachable, IMoveable2d, IMoveable3d
    {
        ~Sprite()
        {
            Dispose();
        }

        internal bool FirstRun = true;
        internal object light_lock = new object();
        
        /// <summary>
        /// Whether or not this Sprite has loaded
        /// </summary>
        public bool Loaded { get; private set; }
        
        /// <summary>
        /// Whether or not this Sprite is visible
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// <para>This determines how this sprite is cached by the rendering system.</para>
        /// <para>This property should be set when the sprite is loaded, and after it's position has been set. The position should not be changed after this property has been set to true.</para>
        /// <para>This property defaults to false.</para>
        /// </summary>
        public bool IsStatic { get; set; }

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

        internal List<Light> Lights = new List<Light>();
        internal List<Light> dynamicLights = new List<Light>();

        internal int LightCount
        {
            get
            {
                return Lights.Count + dynamicLights.Count;
            }
        }
        private Vector2 location;

        /// <summary>
        /// A 2d vector that represents where the sprite is in the current world
        /// </summary>
        public Vector2 Vector2d
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        /// <summary>
        /// A 3d vector that represents where the sprite is in the current world, where Z is the Layer
        /// </summary>
        public Vector3 Vector3d
        {
            get
            {
                return new Vector3(X, Y, Layer);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
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

        private Vector2 size;

        /// <summary>
        /// Get the size of a the sprite as a vector. Where X is the width and Y is the height
        /// </summary>
        public virtual Vector2 Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// The width of this Sprite
        /// </summary>
        public virtual float Width
        {
            get
            {
                return size.X;
            }
            set
            {
                size.X = value;
            }
        }

        /// <summary>
        /// The height of this Sprite
        /// </summary>
        public virtual float Height
        {
            get
            {
                return size.Y;
            }
            set
            {
                size.Y = value;
            }
        }

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
        public virtual float X
        {
            get
            {
                return location.X;
            }
            set
            {
                float dif = value - location.X;

                float ox = location.X;
                location.X = value;

                foreach (IAttachable attached in _children)
                {
                    attached.X += dif;
                }

                if (IsStatic)
                {
                    World w = CurrentWorld;
                    if (ox != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        public virtual FlipState FlipState { get; set; }

        /// <summary>
        /// The Y coordinate of this Sprite in the currently displaying world
        /// </summary>
        public virtual float Y
        {
            get
            {
                return location.Y;
            }
            set
            {
                float dif = value - location.Y;

                float oy = location.Y;
                location.Y = value;

                foreach (IAttachable attached in _children)
                {
                    attached.Y += dif;
                }

                if (IsStatic)
                {
                    World w = CurrentWorld;
                    if (oy != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        public float Z
        {
            get
            {
                return z;
            }
            set
            {
                float dif = value - z;

                float oy = z;
                z = value;

                foreach (IAttachable attached in _children)
                {
                    if (attached is IMoveable3d)
                        ((IMoveable3d)attached).Z += dif;
                }

                if (IsStatic)
                {
                    World w = CurrentWorld;
                    if (oy != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        private float z = 0.1f;
        /// <summary>
        /// The Layer this Sprite lives on in the currently displaying world. Note: Some <see cref="SpriteRenderJob"/>'s don't implement this variable
        /// </summary>
        public virtual float Layer { get { return Z; } set { Z = value; } }

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
        internal void Display()
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


        private List<IAttachable> _children = new List<IAttachable>();
        private List<IAttachable> _parents = new List<IAttachable>();
        public IList<IAttachable> Children
        {
            get { return _children; }
        }

        public IList<IAttachable> Parents
        {
            get { return _parents; }
        }

        public void Attach(IAttachable ToAttach)
        {
            if (_children.Contains(ToAttach))
                throw new ArgumentException("This attachable object is already attached!");

            _children.Add(ToAttach);
            ToAttach.Parents.Add(this);
        }
    }

    [Flags]
    public enum FlipState
    {
        None = 0,
        Vertical = 1,
        Horizontal = 2,

        VerticalHorizontal = Vertical | Horizontal
    }
}