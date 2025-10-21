using OpenTK.Mathematics;
using Sharp2D.Text;
using SkiaSharp;

namespace Sharp2D.UI
{
    /// <summary>
    /// Displays text within the UI using a cached bitmap sprite.
    /// </summary>
    public class UILabel : UIElement
    {
        private StaticTextSprite _textSprite;
        private string _text = string.Empty;
        private SKColor _textColor = SKColors.White;
        private SKTypeface _typeface = SKTypeface.Default;
        private float _fontSize = 18f;

        private bool _autoSize = true;

        public bool AutoSize
        {
            get
            {
                return _autoSize;
            }
            set
            {
                if (_autoSize != value)
                {
                    _autoSize = value;
                    InvalidateLayout();
                }
            }
        }

        private UIHorizontalAlignment _horizontalAlignment = UIHorizontalAlignment.Left;
        private UIVerticalAlignment _verticalAlignment = UIVerticalAlignment.Center;

        public UIHorizontalAlignment HorizontalAlignment
        {
            get
            {
                return _horizontalAlignment;
            }
            set
            {
                if (_horizontalAlignment != value)
                {
                    _horizontalAlignment = value;
                    InvalidateLayout();
                }
            }
        }

        public UIVerticalAlignment VerticalAlignment
        {
            get
            {
                return _verticalAlignment;
            }
            set
            {
                if (_verticalAlignment != value)
                {
                    _verticalAlignment = value;
                    InvalidateLayout();
                }
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                value ??= string.Empty;
                if (_text != value)
                {
                    _text = value;
                    RebuildText();
                }
            }
        }

        public SKColor TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    RebuildText();
                }
            }
        }

        public SKTypeface Typeface
        {
            get
            {
                return _typeface;
            }
            set
            {
                if (_typeface != value)
                {
                    _typeface = value ?? SKTypeface.Default;
                    RebuildText();
                }
            }
        }

        public float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                if (System.Math.Abs(_fontSize - value) > float.Epsilon)
                {
                    _fontSize = value;
                    RebuildText();
                }
            }
        }

        protected override Vector2 MeasureOverride()
        {
            var explicitSize = base.MeasureOverride();
            if (!AutoSize && explicitSize != Vector2.Zero)
            {
                return explicitSize;
            }

            return GetTextSize();
        }

        protected override void OnLayout(UIRect bounds)
        {
            base.OnLayout(bounds);

            if (_textSprite == null)
            {
                return;
            }

            var measured = GetTextSize();
            var width = bounds.Width;
            var height = bounds.Height;

            var offsetX = _horizontalAlignment switch
            {
                UIHorizontalAlignment.Center => (width - measured.X) * 0.5f,
                UIHorizontalAlignment.Right => width - measured.X,
                _ => 0f
            };

            var offsetY = _verticalAlignment switch
            {
                UIVerticalAlignment.Center => (height - measured.Y) * 0.5f,
                UIVerticalAlignment.Bottom => height - measured.Y,
                _ => 0f
            };

            offsetX = System.Math.Max(0f, offsetX);
            offsetY = System.Math.Max(0f, offsetY);

            var textRect = new UIRect(bounds.Position + new Vector2(offsetX, offsetY), measured);
            ApplySpriteBounds(_textSprite, textRect);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_textSprite != null)
            {
                UnregisterSprite(_textSprite);
                _textSprite = null;
            }
        }

        private void RebuildText()
        {
            if (_textSprite != null)
            {
                UnregisterSprite(_textSprite);
                _textSprite = null;
            }

            if (string.IsNullOrEmpty(_text))
            {
                InvalidateLayout();
                return;
            }

            _textSprite = RegisterSprite(TextBuilder.CreateTextSprite(_text, _textColor, _typeface, _fontSize));
            _textSprite.IgnoreLights = true;
            _textSprite.NeverClip = true;

            InvalidateLayout();
        }

        private Vector2 GetTextSize()
        {
            if (_textSprite == null)
            {
                return Vector2.Zero;
            }

            return new Vector2(_textSprite.StringWidth, _textSprite.StringHeight);
        }
    }
}
