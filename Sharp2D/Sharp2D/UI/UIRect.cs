using OpenTK.Mathematics;

namespace Sharp2D.UI
{
    /// <summary>
    /// Represents an axis-aligned rectangle in UI space.
    /// </summary>
    public readonly struct UIRect
    {
        public Vector2 Position { get; }
        public Vector2 Size { get; }

        public Vector2 Min
        {
            get
            {
                return Position;
            }
        }

        public Vector2 Max
        {
            get
            {
                return Position + Size;
            }
        }

        public Vector2 Center
        {
            get
            {
                return Position + (Size * 0.5f);
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }
        }

        public float Height
        {
            get
            {
                return Size.Y;
            }
        }

        public UIRect(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y;
        }

        public override string ToString()
        {
            return $"UIRect(Position={Position}, Size={Size})";
        }
    }
}
