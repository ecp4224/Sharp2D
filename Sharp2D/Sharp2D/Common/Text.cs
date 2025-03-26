using System.Collections.Generic;
using SkiaSharp;

namespace Sharp2D
{
    public static class Text
    {
        private static readonly Dictionary<string, SKSize> Sizes = new Dictionary<string, SKSize>();
        public static TextSprite CreateTextSprite(string text)
        {
            return CreateTextSprite(text, SKColors.Black, SKTypeface.Default, 16);
        }

        public static TextSprite CreateTextSprite(string text, SKColor color)
        {
            return CreateTextSprite(text, color, SKTypeface.Default, 16);
        }

        public static TextSprite CreateTextSprite(string text, SKColor color, SKTypeface font, float fontSize)
        {
            string textureName = "TEXT_" + text + "_FONT_" + font.FamilyName;
            Texture texture = Texture.NewTexture(textureName);

            if (!texture.Loaded)
            {
                // Set up SKPaint for drawing text.
                using (var paint = new SKPaint())
                {
                    paint.Color = color;
                    paint.Typeface = font;
                    paint.TextSize = fontSize;
                    paint.IsAntialias = true;

                    // Measure text width.
                    float textWidth = paint.MeasureText(text);
                    // Measure text height using font metrics.
                    SKFontMetrics fm;
                    paint.GetFontMetrics(out fm);
                    float textHeight = fm.Descent - fm.Ascent;

                    // Determine bitmap dimensions.
                    int bmpWidth = (int)System.Math.Ceiling(textWidth);
                    int bmpHeight = (int)System.Math.Ceiling(textHeight);

                    // Create an SKBitmap to draw the text.
                    using (var bitmap = new SKBitmap(bmpWidth, bmpHeight))
                    {
                        using (var canvas = new SKCanvas(bitmap))
                        {
                            // Clear with transparent background.
                            canvas.Clear(SKColors.Transparent);
                            // Draw the text.
                            // We use -fm.Ascent so that the top of the text aligns with Y=0.
                            canvas.DrawText(text, 0, -fm.Ascent, paint);
                        }

                        // Prepare the texture to have the correct size.
                        texture.BlankTexture(bmpWidth, bmpHeight);

                        // Copy our drawn bitmap to the texture.
                        using (var textureCanvas = new SKCanvas(texture.Bitmap))
                        {
                            textureCanvas.DrawBitmap(bitmap, 0, 0);
                        }

                        // Cache the measured size.
                        Sizes.Add(texture.Name, new SKSize(textWidth, textHeight));
                    }
                }
            }

            return new TextSprite(texture.Name, Sizes[texture.Name], text)
            {
                Texture = texture,
                Width = texture.TextureWidth,
                Height = texture.TextureHeight
            };
        }
    }

    public sealed class TextSprite : Sprite.BlankSprite
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

        public TextSprite(string name, SKSize stringSize, string text)
            : base(name)
        {
            StringSize = stringSize;
        }
    }
}
