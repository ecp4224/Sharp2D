using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Sprites;
using Sharp2D.Game.Worlds;

namespace Sharp2D
{
    /// <summary>
    /// <para>A Sprite is an object that can be drawn by a <see cref="BatchRenderJob"/>.</para>
    /// <para>A Sprite is a quad that can any width or height, but ALWAYS has a texture</para>
    /// </summary>
    public abstract partial class Sprite : IDisposable, IAttachable, IMoveable3d, IComparable<Sprite>
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
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// The name of this sprite.
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// Whether or not this Sprite is visible
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// <para>This determines how this sprite is cached by the rendering system.</para>
        /// <para>This property should be set when the sprite is loaded, and after it's position has been set. The position should not be changed after this property has been set to true.</para>
        /// <para>This property defaults to false.</para>
        /// </summary>
        public bool IsStatic { get; set; }

        private Texture _texture;
        private Shader _shader;

        public override string ToString()
        {
            return "Sharp2D_Sprite_" + Name;
        }
        
        /// <summary>
        /// The <see cref="BatchRenderJob"/>'s this Sprite belongs to.
        /// </summary>
        public List<BatchRenderJob> ContainingJobs
        {
            get
            {
                var world = CurrentWorld;
                if (world == null) return new List<BatchRenderJob>();
                var toReturn = new List<BatchRenderJob>();
                List<BatchRenderJob> jobs = world.BatchRenderJobs;
                foreach (BatchRenderJob job in jobs)
                {
                    if (job.HasSprite(this))
                        toReturn.Add(job);
                }

                return toReturn;
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
                float ox = X, oy = Y;
                X = value.X;
                Y = value.Y;
                Z = value.Z;

                if (Moved != null) Moved(this, new OnMoveableMoved(this, ox, oy));
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
                List<BatchRenderJob> temp = ContainingJobs;

                foreach (BatchRenderJob job in temp) job.RemoveSprite(this);

                _shader = value;

                foreach (BatchRenderJob job in temp) job.AddSprite(this);
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
                List<BatchRenderJob> temp = ContainingJobs;

                foreach (BatchRenderJob job in temp) job.RemoveSprite(this);

                _texture = value;

                foreach (BatchRenderJob job in temp) job.AddSprite(this);
            }
        }

        /// <summary>
        /// The position of this Sprite in the currently displaying world represented as a <see cref="Vector2"/>
        /// </summary>
        public Vector2 Position { get { return new Vector2(X, Y); } }
        internal readonly List<BatchJobWorld> _worlds = new List<BatchJobWorld>();
        public IList<BatchJobWorld> ContainingWorlds
        {
            get
            {
                return _worlds.AsReadOnly();
            }
        }

        /// <summary>
        /// Get the world the sprite is currently in. This world is the current World being displayed on the screen. To get all the Worlds this sprite is in, use ContainingWorlds
        /// </summary>
        public BatchJobWorld CurrentWorld
        {
            get
            {
                return ContainingWorlds.FirstOrDefault(w => w.Displaying);
            }
        }

        /// <summary>
        /// If true, this sprite will always be drawn, regardless if it's on screen or not. Otherwise the sprite will be clipped if IsOffScreen returns true.
        /// </summary>
        public bool NeverClip
        {
            get
            {
                return _neverclip;
            }
            set
            {
                _neverclip = value;
            }
        }

        private bool _neverclip = false;

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
                return !_neverclip && Screen.Camera.IsOutsideCamera(X, Y, Width, Height, Scale);
            }
        }

        private bool staticWarn = false;

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
                if (Moved != null) Moved(this, new OnMoveableMoved(this, ox, Y));

                foreach (IAttachable attached in _children)
                {
                    attached.X += dif;
                }

                if (IsStatic)
                {
                    if (!staticWarn)
                    {
                        Logger.Warn("The static sprite " + Name + " has been moved! This may cause unwanted results!");
                        staticWarn = true;
                    }
                    World w = CurrentWorld;
                    if (ox != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        private float _scale = 1f;
        /// <summary>
        /// The uniform scale/size of this sprite.
        /// </summary>
        public virtual float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        internal Vector4 ShaderColor = new Vector4(1f, 1f, 1f, 0f);
        private Color _color = Color.White;
        public Color TintColor
        {
            get { return _color; }
            set
            {
                _color = value;
                
                ShaderColor = new Vector4((_color.R / 255f), (_color.G / 255f), (_color.B / 255f), 1f - _color.A);
            }
        }

        public virtual FlipState FlipState { get; set; }
        
        /// <summary>
        /// If set to true, then this sprite will ignore ALL lights in the world.
        /// </summary>
        public virtual bool IgnoreLights { get; set; }

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

                if (Moved != null) Moved(this, new OnMoveableMoved(this, X, oy));

                foreach (IAttachable attached in _children)
                {
                    attached.Y += dif;
                }

                if (IsStatic)
                {
                    if (!staticWarn)
                    {
                        Logger.Warn("The static sprite " + Name + " has been moved! This may cause unwanted results!");
                        staticWarn = true;
                    }
                    World w = CurrentWorld;
                    if (oy != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        /// <summary>
        /// The Z coordinate of this Sprite in the currently displaying world
        /// </summary>
        public virtual float Z
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
                    if (!staticWarn)
                    {
                        Logger.Warn("The static sprite " + Name + " has been moved! This may cause unwanted results!");
                        staticWarn = true;
                    }
                    World w = CurrentWorld;
                    if (oy != value && CurrentWorld is ILightWorld)
                        ((ILightWorld)w).UpdateSpriteLights(this);
                }
            }
        }

        /// <summary>
        /// Determines how this Sprite will be drawn and how this Sprite will be placed.
        /// </summary>
        public virtual SpaceType SpacePlacement
        {
            get
            {
                return space;
            }
            set
            {
                space = value;
            }
        }

        private SpaceType space = SpaceType.World;
        private float z = 0.1f;
        /// <summary>
        /// The Layer this Sprite lives on in the currently displaying world. Note: Some <see cref="BatchRenderJob"/>'s don't implement this variable
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
            IsVisible = true;

            OnLoad();
            if (TexCoords == null && Texture != null)
            {
                TexCoords = new TexCoords(Width, Height, Texture);
            }
            else if (Texture == null)
                throw new InvalidOperationException("This sprite has no texture! A sprite MUST have a texture!");
            IsLoaded = true;

            if (Loaded != null)
                Loaded(this, new SpriteEvent(this));
        }

        /// <summary>
        /// Unload the sprite
        /// </summary>
        public void Unload()
        {
            OnUnload();
            IsLoaded = false;

            if (Unloaded != null)
                Unloaded(this, new SpriteEvent(this));
        }

        /// <summary>
        /// Dispose the sprite
        /// </summary>
        public void Dispose()
        {
            if (IsLoaded)
                Unload();
            OnDispose();

            if (Disposed != null)
                Disposed(this, new SpriteEvent(this));
        }

        /// <summary>
        /// Display the Sprite
        /// </summary>
        internal void Display()
        {
            OnDisplay();
            if (Texture != null && Texture.ID == -1)
                Texture.Create();

            if (Displayed != null)
                Displayed(this, new SpriteEvent(this));
        }

        /// <summary>
        /// Prepare the Sprite to be drawn onto the screen
        /// </summary>
        public void PrepareDraw()
        {
            BeforeDraw();

            if (Drawn != null)
                Drawn(this, new SpriteEvent(this));
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
        /// <summary>
        /// How transparent this Sprite object is. (Must be a value between 0-1)
        /// </summary>
        public float Alpha {
            get { return _color.A; }
            set
            {
                _color = Color.FromArgb((int) (value*255), _color);

                ShaderColor = new Vector4(_color.R / 255f, _color.G / 255f, _color.B / 255f, 1f - (_color.A / 255f));
            }
        }

        /// <summary>
        /// Whether or not this Sprite object contains alpha
        /// </summary>
        public virtual bool HasAlpha
        {
            get
            {
                return Alpha < 1f || Texture.HasAlpha;
            }
        }

        public int CompareTo(Sprite other)
        {
            if (Layer == other.Layer)
            {
                if (Texture == null)
                    return -1; //This should be drawn first
                else if (other.Texture == null)
                    return 1; //This should be drawn after other
                else
                    return Texture.ID - other.Texture.ID;
            }
            return (int)(Layer - other.Layer);
        }
    }

    public enum SpaceType
    {
        /// <summary>
        /// Screen Space means no lights will affect the sprite, and it will be drawn in screen space (where 0,0 is the top left of the screen)
        /// </summary>
        Screen,

        /// <summary>
        /// World Space means all lights will affect the sprite, and it will be drawn in world space (where 0,0 is the world origin)
        /// </summary>
        World
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