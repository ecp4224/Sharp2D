using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Sharp2D;
using Sharp2D.Core;

namespace Sharp2D.UI
{
    /// <summary>
    /// Base class for visual elements that can participate in the UI tree.
    /// </summary>
    public abstract class UIElement : IDisposable
    {
        private readonly List<Sprite> _sprites = new List<Sprite>();
        private Vector2 _position;
        private Vector2 _size;
        private float _zIndex;
        private bool _isVisible = true;
        private float _opacity = 1f;
        private bool _layoutDirty = true;
        private bool _isDisposed;
        private UIRect _bounds;

        public UIContainer Parent { get; private set; }
        public UICanvas Canvas { get; private set; }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    InvalidateLayout();
                }
            }
        }

        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    InvalidateLayout();
                }
            }
        }

        public float ZIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                if (Math.Abs(_zIndex - value) > float.Epsilon)
                {
                    _zIndex = value;
                    UpdateSpriteLayers();
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    UpdateSpriteVisibility();
                }
            }
        }

        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                value = Math.Clamp(value, 0f, 1f);
                if (Math.Abs(_opacity - value) > float.Epsilon)
                {
                    _opacity = value;
                    UpdateSpriteOpacity();
                }
            }
        }

        public UIRect Bounds
        {
            get
            {
                return _bounds;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        internal bool RequiresLayout
        {
            get
            {
                return _layoutDirty;
            }
        }

        internal IReadOnlyList<Sprite> RegisteredSprites
        {
            get
            {
                return _sprites.AsReadOnly();
            }
        }

        public virtual void Update(UIUpdateContext context)
        {
            if (!IsVisible)
            {
                return;
            }

            OnUpdate(context);
        }

        protected virtual void OnUpdate(UIUpdateContext context)
        {
        }

        protected void InvalidateLayout()
        {
            if (_layoutDirty)
            {
                return;
            }

            _layoutDirty = true;
            Parent?.OnChildLayoutInvalidated();
            Canvas?.RequestLayout();
        }

        internal void Arrange(Vector2 origin)
        {
            var finalSize = GetDesiredSize();
            var absolute = origin + _position;
            _bounds = new UIRect(absolute, finalSize);
            _layoutDirty = false;

            OnLayout(_bounds);
        }

        protected virtual Vector2 MeasureOverride()
        {
            return _size;
        }

        protected virtual void OnLayout(UIRect bounds)
        {
            foreach (var sprite in _sprites)
            {
                ApplySpriteBounds(sprite, bounds);
            }
            UpdateSpriteOpacity();
            UpdateSpriteVisibility();
        }

        internal void AttachToParent(UIContainer parent)
        {
            Parent = parent;
            AttachToCanvas(parent?.Canvas);
        }


        internal void DetachFromParent()
        {
            Parent = null;
        }
        internal virtual void AttachToCanvas(UICanvas canvas)
        {
            if (Canvas == canvas)
            {
                return;
            }

            Canvas = canvas;
            if (Canvas == null)
            {
                return;
            }

            foreach (var sprite in _sprites)
            {
                Canvas.RegisterSprite(this, sprite);
            }

            OnAttachedToCanvas();
            UpdateSpriteLayers();
            UpdateSpriteVisibility();
            UpdateSpriteOpacity();
        }

        internal virtual void DetachFromCanvas()
        {
            if (Canvas == null)
            {
                return;
            }

            foreach (var sprite in _sprites)
            {
                Canvas.UnregisterSprite(sprite);
            }

            OnDetachedFromCanvas();
            Canvas = null;
        }

        internal virtual void OnChildLayoutInvalidated()
        {
            InvalidateLayout();
        }

        protected virtual void OnAttachedToCanvas()
        {
        }

        protected virtual void OnDetachedFromCanvas()
        {
        }

        protected T RegisterSprite<T>(T sprite) where T : Sprite
        {
            if (!_sprites.Contains(sprite))
            {
                sprite.SpacePlacement = SpaceType.Screen;
                sprite.IgnoreLights = true;
                sprite.NeverClip = true;
                _sprites.Add(sprite);
                if (Canvas != null)
                {
                    Canvas.RegisterSprite(this, sprite);
                }
            }

            return sprite;
        }

        protected void UnregisterSprite(Sprite sprite)
        {
            if (!_sprites.Remove(sprite))
            {
                return;
            }

            Canvas?.RemoveRegisteredSprite(sprite);
        }

        protected void ApplySpriteBounds(Sprite sprite, UIRect bounds)
        {
            sprite.X = bounds.Center.X;
            sprite.Y = bounds.Center.Y;
            sprite.Width = bounds.Width;
            sprite.Height = bounds.Height;

            if (sprite.Texture != null)
            {
                sprite.TexCoords = new TexCoords(sprite.Texture.TextureWidth, sprite.Texture.TextureHeight, sprite.Texture);
            }
        }

        protected internal Vector2 GetDesiredSize()
        {
            if (_size != Vector2.Zero)
            {
                return _size;
            }

            return MeasureOverride();
        }

        protected void UpdateSpriteLayers()
        {
            if (Canvas == null)
            {
                return;
            }

            Canvas.UpdateSpriteLayers(this);
        }

        private void UpdateSpriteVisibility()
        {
            foreach (var sprite in _sprites)
            {
                sprite.IsVisible = _isVisible;
            }
        }

        private void UpdateSpriteOpacity()
        {
            foreach (var sprite in _sprites)
            {
                sprite.Alpha = _opacity;
            }
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            DetachFromCanvas();
            _sprites.Clear();
        }
    }
}
