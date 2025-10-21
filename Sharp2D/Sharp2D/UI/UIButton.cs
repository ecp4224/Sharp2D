using System;
using Sharp2D;
using OpenTK.Mathematics;
using SkiaSharp;

namespace Sharp2D.UI
{
    /// <summary>
    /// Interactive button composed of a colored background and a text label.
    /// </summary>
    public class UIButton : UIContainer
    {
        private Sprite _backgroundSprite;
        private readonly UILabel _label;
        private SKColor _backgroundColor = new SKColor(37, 37, 37);
        private SKColor _hoverColor = new SKColor(60, 60, 60);
        private SKColor _pressedColor = new SKColor(90, 90, 90);
        private bool _isHovered;
        private bool _isPressed;

        public event Action<UIButton> Clicked;

        public UIButton()
        {
            Size = new Vector2(160f, 48f);
            Padding = new UIPadding(12f, 8f, 12f, 8f);

            _label = new UILabel
            {
                AutoSize = false,
                HorizontalAlignment = UIHorizontalAlignment.Center,
                VerticalAlignment = UIVerticalAlignment.Center
            };
            _label.TextColor = SKColors.White;
            AddChild(_label);
        }

        public string Text
        {
            get
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
            }
        }

        public SKColor TextColor
        {
            get
            {
                return _label.TextColor;
            }
            set
            {
                _label.TextColor = value;
            }
        }

        public SKColor BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                UpdateBackgroundTint();
            }
        }

        public SKColor HoverColor
        {
            get
            {
                return _hoverColor;
            }
            set
            {
                _hoverColor = value;
                UpdateBackgroundTint();
            }
        }

        public SKColor PressedColor
        {
            get
            {
                return _pressedColor;
            }
            set
            {
                _pressedColor = value;
                UpdateBackgroundTint();
            }
        }

        public UILabel Label
        {
            get
            {
                return _label;
            }
        }

        protected override void OnAttachedToCanvas()
        {
            base.OnAttachedToCanvas();

            if (_backgroundSprite == null)
            {
                _backgroundSprite = RegisterSprite(Sprite.FromTexture(UITextureFactory.GetWhitePixel()));
                _backgroundSprite.IgnoreLights = true;
                _backgroundSprite.NeverClip = true;
                _backgroundSprite.TintColor = _backgroundColor;
            }
        }

        public override void Update(UIUpdateContext context)
        {
            base.Update(context);

            if (!IsVisible)
            {
                return;
            }

            bool hovered = Bounds.Contains(context.Input.MousePosition);
            if (hovered && context.Input.LeftPressed)
            {
                _isPressed = true;
            }

            if (!context.Input.IsLeftDown)
            {
                _isPressed = false;
            }

            if (_isPressed && context.Input.LeftReleased)
            {
                if (hovered)
                {
                    Clicked?.Invoke(this);
                }
                _isPressed = false;
            }

            _isHovered = hovered;
            UpdateBackgroundTint();
        }

        protected override void OnLayout(UIRect bounds)
        {
            if (_backgroundSprite != null)
            {
                ApplySpriteBounds(_backgroundSprite, bounds);
            }

            var contentWidth = System.Math.Max(0f, bounds.Width - Padding.Horizontal);
            var contentHeight = System.Math.Max(0f, bounds.Height - Padding.Vertical);
            var contentSize = new Vector2(contentWidth, contentHeight);

            _label.Position = Vector2.Zero;
            _label.Size = contentSize;

            base.OnLayout(bounds);
        }

        public override void Dispose()
        {
            base.Dispose();
            _backgroundSprite = null;
        }

        private void UpdateBackgroundTint()
        {
            if (_backgroundSprite == null)
            {
                return;
            }

            if (_isPressed)
            {
                _backgroundSprite.TintColor = _pressedColor;
            }
            else if (_isHovered)
            {
                _backgroundSprite.TintColor = _hoverColor;
            }
            else
            {
                _backgroundSprite.TintColor = _backgroundColor;
            }
        }
    }
}
