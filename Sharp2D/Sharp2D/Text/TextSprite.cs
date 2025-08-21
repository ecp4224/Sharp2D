using OpenTK.Mathematics;
using SkiaSharp;

namespace Sharp2D.Text
{
    /// <summary>
    /// Represents a drawable string using an SDF font.
    /// </summary>
    public class TextSprite
    {
        public TextMesh Mesh { get; } = new TextMesh();
        public SdfFont Font { get; }
        public Vector2 Position { get; private set; }
        public float Scale { get; private set; } = 1f;
        public bool UseKerning { get; private set; }
        public SKColor Color { get; private set; } = SKColors.White;
        public float Smoothing { get; set; } = 0.1f;
        public float Threshold { get; set; } = 0.5f;
        public float Z { get; set; } = 0.1f;
        
        private string _text = string.Empty;

        public TextSprite(SdfFont font)
        {
            Font = font;
        }

        public void SetText(string text)
        {
            _text = text;
            Mesh.SetText(Font, _text, Position, Scale, UseKerning);
        }

        public void SetColor(SKColor color)
        {
            Color = color;
        }

        public void SetScale(float scale)
        {
            Scale = scale;
            Mesh.SetText(Font, _text, Position, Scale, UseKerning);
        }

        public void SetPosition(Vector2 pos)
        {
            Position = new Vector2(pos.X, -pos.Y);
            Mesh.SetText(Font, _text, Position, Scale, UseKerning);
        }

        public void SetKerning(bool useKerning)
        {
            UseKerning = useKerning;
            Mesh.SetText(Font, _text, Position, Scale, UseKerning);
        }
    }
}
