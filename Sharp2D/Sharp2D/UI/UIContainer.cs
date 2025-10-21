using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Sharp2D.UI
{
    public readonly struct UIPadding
    {
        public static UIPadding Zero
        {
            get
            {
                return new UIPadding(0f, 0f, 0f, 0f);
            }
        }

        public float Left { get; }
        public float Top { get; }
        public float Right { get; }
        public float Bottom { get; }

        public float Horizontal
        {
            get
            {
                return Left + Right;
            }
        }

        public float Vertical
        {
            get
            {
                return Top + Bottom;
            }
        }

        public UIPadding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    /// <summary>
    /// A UI element that can contain child elements and arrange them.
    /// </summary>
    public class UIContainer : UIElement
    {
        private readonly List<UIElement> _children = new List<UIElement>();
        private UIPadding _padding = UIPadding.Zero;

        public IReadOnlyList<UIElement> Children
        {
            get
            {
                return _children.AsReadOnly();
            }
        }

        public UIPadding Padding
        {
            get
            {
                return _padding;
            }
            set
            {
                if (!value.Equals(_padding))
                {
                    _padding = value;
                    InvalidateLayout();
                }
            }
        }

        public void AddChild(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (_children.Contains(element))
            {
                throw new InvalidOperationException("The element is already a child of this container.");
            }

            if (element.Parent != null)
            {
                throw new InvalidOperationException("The element already has a parent.");
            }

            _children.Add(element);
            element.AttachToParent(this);
            InvalidateLayout();
        }

        public bool RemoveChild(UIElement element)
        {
            if (element == null)
            {
                return false;
            }

            if (_children.Remove(element))
            {
                element.DetachFromCanvas();
                element.DetachFromParent();
                InvalidateLayout();
                return true;
            }

            return false;
        }

        public void ClearChildren()
        {
            foreach (var child in _children)
            {
                child.DetachFromCanvas();
                child.DetachFromParent();
            }
            _children.Clear();
            InvalidateLayout();
        }

        public override void Update(UIUpdateContext context)
        {
            base.Update(context);

            if (!IsVisible)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.Update(context);
            }
        }

        internal override void AttachToCanvas(UICanvas canvas)
        {
            base.AttachToCanvas(canvas);

            foreach (var child in _children)
            {
                child.AttachToCanvas(canvas);
            }
        }

        internal override void DetachFromCanvas()
        {
            foreach (var child in _children)
            {
                child.DetachFromCanvas();
            }

            base.DetachFromCanvas();
        }

        protected override Vector2 MeasureOverride()
        {
            var explicitSize = base.MeasureOverride();
            if (explicitSize != Vector2.Zero)
            {
                return explicitSize;
            }

            Vector2 max = Vector2.Zero;
            foreach (var child in _children)
            {
                var desired = child.GetDesiredSize();
                var extent = child.Position + desired;
                max.X = MathF.Max(max.X, extent.X);
                max.Y = MathF.Max(max.Y, extent.Y);
            }

            return new Vector2(max.X + _padding.Horizontal, max.Y + _padding.Vertical);
        }

        protected override void OnLayout(UIRect bounds)
        {
            base.OnLayout(bounds);

            var contentOrigin = bounds.Position + new Vector2(_padding.Left, _padding.Top);
            foreach (var child in _children)
            {
                var childOrigin = contentOrigin + child.Position;
                child.Arrange(childOrigin);
            }
        }

        internal override void OnChildLayoutInvalidated()
        {
            InvalidateLayout();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var child in _children)
            {
                child.Dispose();
            }
            _children.Clear();
        }
    }
}
