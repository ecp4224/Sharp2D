using System;
using System.Collections.Generic;
using Sharp2D.Core;
using Sharp2D.Fonts;
using Sharp2D.Render;
using SkiaSharp;

namespace Sharp2D.Text
{
    public static class TextBuilder
    {
        private static readonly Dictionary<string, SKSize> Sizes = new Dictionary<string, SKSize>();
        public static StaticTextSprite CreateTextSprite(string text)
        {
            return CreateTextSprite(text, SKColors.Black, SKTypeface.Default, 16);
        }

        public static StaticTextSprite CreateTextSprite(string text, SKColor color)
        {
            return CreateTextSprite(text, color, SKTypeface.Default, 16);
        }

        public static StaticTextSprite CreateTextSprite(string text, SKColor color, SKTypeface font, float fontSize)
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

            return new StaticTextSprite(texture.Name, Sizes[texture.Name], text)
            {
                Texture = texture,
                Width = texture.TextureWidth,
                Height = texture.TextureHeight
            };
        }
        

        public static TextSprite AddTextSprite(this GenericWorld world, SdfFont font, string text)
        {
            var job = world.GetJob<TextRenderJob>();
            if (job == null)
            {
                job = new TextRenderJob(world);

                if (!world.Loaded)
                {
                    world.OnWorldLoaded += WorldOnOnWorldLoaded;

                    void WorldOnOnWorldLoaded(object sender, EventArgs e)
                    {
                        world.AddRenderJob(job);
                        world.OnWorldLoaded -= WorldOnOnWorldLoaded;
                    }
                }
                else
                {
                    world.AddRenderJob(job);
                }
            }

            var textSprite = new TextSprite(font);
            textSprite.SetText(text);
            return textSprite;
        }
    }
}
