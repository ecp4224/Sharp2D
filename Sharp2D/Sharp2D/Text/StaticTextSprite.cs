using SkiaSharp;

namespace Sharp2D.Text
{
    public sealed class StaticTextSprite : Sprite.BlankSprite
    {
        public SKSize StringSize { get; private set; }

        public string Text { get; private set; }

        public float StringWidth
        {
            get { return StringSize.Width; }
        }

        public float StringHeight
        {
            get { return StringSize.Height; }
        }

        public StaticTextSprite(string name, SKSize stringSize, string text)
            : base(name)
        {
            StringSize = stringSize;
        }
    }
}