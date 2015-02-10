using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D
{
    public static class Text
    {
        private static readonly Dictionary<string, SizeF> Sizes = new Dictionary<string, SizeF>();
        public static TextSprite CreateTextSprite(string text)
        {
            return CreateTextSprite(text, Color.Black, SystemFonts.DefaultFont);
        }

        public static TextSprite CreateTextSprite(string text, Color color)
        {
            return CreateTextSprite(text, color, SystemFonts.DefaultFont);
        }

        public static TextSprite CreateTextSprite(string text, Color color, Font font)
        {
            Texture texture = Texture.NewTexture("TEXT_" + text + "_FONT_" + font.Name);

            if (!texture.Loaded)
            {
                var bitmap = new Bitmap((int)((font.Size * text.Length) + 64), (int)(font.GetHeight() + 64));

                SizeF stringSize;
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var brush = new SolidBrush(color))
                    {
                        graphics.DrawString(text, font, brush, 0f, 0f);
                    }

                    stringSize = graphics.MeasureString(text, font);
                }

                texture.BlankTexture((int)stringSize.Width, (int)stringSize.Height);

                using (var g = Graphics.FromImage(texture.Bitmap))
                {
                    g.DrawImage(bitmap, new PointF(0, 0));
                }

                bitmap.Dispose();

                Sizes.Add(texture.Name, stringSize);
            }

            return new TextSprite(texture.Name, Sizes[texture.Name])
            {
                Texture = texture,
                Width = texture.TextureWidth,
                Height = texture.TextureHeight
            };
        }
    }

    public sealed class TextSprite : Sprite.BlankSprite
    {
        public SizeF StringSize { get; private set; }

        public float StringWidth
        {
            get { return StringSize.Width; }
        }

        public float StringHeight
        {
            get { return StringSize.Height; }
        }

        public TextSprite(string name, SizeF stringSize)
            : base(name)
        {
            StringSize = stringSize;
        }
    }
}
