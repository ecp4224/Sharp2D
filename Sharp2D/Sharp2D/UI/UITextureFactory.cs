using System.Collections.Generic;
using Sharp2D;
using SkiaSharp;

namespace Sharp2D.UI
{
    internal static class UITextureFactory
    {
        private static readonly object _lock = new object();
        private static Texture _whitePixel;
        private static readonly Dictionary<SKColor, Texture> _solidCache = new Dictionary<SKColor, Texture>();

        public static Texture GetWhitePixel()
        {
            if (_whitePixel != null)
            {
                return _whitePixel;
            }

            lock (_lock)
            {
                if (_whitePixel == null)
                {
                    var texture = Texture.NewTexture("__sharp2d_ui_white_pixel");
                    if (!texture.Loaded)
                    {
                        texture.BlankTexture(1, 1);
                        using (var canvas = new SKCanvas(texture.Bitmap))
                        {
                            canvas.Clear(SKColors.White);
                        }
                    }
                    _whitePixel = texture;
                }
            }

            return _whitePixel;
        }

        public static Texture GetSolidColor(SKColor color)
        {
            lock (_lock)
            {
                if (_solidCache.TryGetValue(color, out var cached))
                {
                    return cached;
                }

                var texture = Texture.NewTexture($"__sharp2d_ui_solid_{color.Red}_{color.Green}_{color.Blue}_{color.Alpha}");
                texture.BlankTexture(1, 1);
                using (var canvas = new SKCanvas(texture.Bitmap))
                {
                    canvas.Clear(color);
                }

                _solidCache[color] = texture;
                return texture;
            }
        }
    }
}
