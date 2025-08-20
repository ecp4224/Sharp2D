using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using System.IO;
using SkiaSharp;

namespace Sharp2D
{
    public class Texture : ICloneable
    {
        private static Dictionary<string, Texture> cache = new Dictionary<string, Texture>();

        public static Texture NewTexture(string resource)
        {
            if (cache.ContainsKey(resource))
                return cache[resource];

            Texture texture = new Texture();
            texture.Name = resource;

            cache.Add(resource, texture);

            return texture;
        }

        public int ID { get; private set; }

        public int MinFilter { get; set; }

        public int MagFilter { get; set; }

        public int ImageWidth { get; private set; }

        public int ImageHeight { get; private set; }

        public int TextureWidth { get; private set; }

        public int TextureHeight { get; private set; }

        public SKColorType ColorType { get; set; } = SKColorType.Bgra8888;

        public bool Loaded
        {
            get
            {
                return Bitmap != null;
            }
        }

        public bool Created
        {
            get
            {
                return ID != -1;
            }
        }

        public bool HasAlpha { get; private set; }

        public string Name { get; private set; }

        public SKBitmap Bitmap { get; private set; }

        private Texture() { ID = -1; MinFilter = (int)TextureMinFilter.Nearest; MagFilter = (int)TextureMagFilter.Nearest; }

        public void LoadTextureFromResource()
        {
            Screen.ValidateOpenGLUnsafe("Texture.LoadTextureFromResource", true);
            Assembly extender = Assembly.GetEntryAssembly();
            Assembly sharp2d = this.GetType().Assembly;

            Stream stream = extender.GetManifestResourceStream(Name) ?? sharp2d.GetManifestResourceStream(Name);

            if (stream == null)
                throw new FileNotFoundException("Could not find resource " + Name);

            // Decode stream into SKBitmap
            var temp = SKBitmap.Decode(stream);
            var info = new SKImageInfo(temp.Width, temp.Height, ColorType, SKAlphaType.Premul);
            Bitmap = new SKBitmap(info);
            temp.CopyTo(Bitmap);
            temp.Dispose();
            HasAlpha = Bitmap.ContainsAlpha();
            ValidateSize();
        }

        public void LoadTextureFromFile()
        {
            Screen.ValidateOpenGLUnsafe("Texture.LoadTextureFromFile", true);

            SKBitmap temp;
            using (var fs = new FileStream(Name, FileMode.Open))
            {
                temp = SKBitmap.Decode(fs);
            }
            
            var info = new SKImageInfo(temp.Width, temp.Height, ColorType, SKAlphaType.Premul);
            Bitmap = new SKBitmap(info);
            temp.CopyTo(Bitmap);
            temp.Dispose();
            HasAlpha = Bitmap.ContainsAlpha();

            ValidateSize();
        }

        private void ValidateSize()
        {
            ImageWidth = Bitmap.Width;
            ImageHeight = Bitmap.Height;

            TextureWidth = ((ImageWidth & (ImageWidth - 1)) == 0 ? ImageWidth : 2);
            TextureHeight = ((ImageHeight & (ImageHeight - 1)) == 0 ? ImageHeight : 2);

            while (TextureWidth < ImageWidth) TextureWidth *= 2;

            while (TextureHeight < ImageHeight) TextureHeight *= 2;

            if (TextureWidth != ImageWidth || TextureHeight != ImageHeight)
            {
                // Create a new bitmap with the proper power-of-two dimensions.
                SKBitmap newBitmap = new SKBitmap(TextureWidth, TextureHeight, ColorType, SKAlphaType.Premul);
                using (var canvas = new SKCanvas(newBitmap))
                {
                    // Draw the original bitmap into the new one.
                    SKRect destRect = new SKRect(0, 0, ImageWidth, ImageHeight);
                    SKRect srcRect = new SKRect(0, 0, ImageWidth, ImageHeight);
                    canvas.DrawBitmap(Bitmap, srcRect, destRect);
                }
                Bitmap = newBitmap;
            }
        }

        public void Create()
        {
            if (Created)
                return;
            Screen.ValidateOpenGLSafe("Texture.Create");

            ID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

            
            // Get pixel data from SKBitmap using the configured color type.
            IntPtr pixelData = Bitmap.GetPixels();
            var format = ColorType == SKColorType.Bgra8888 ? PixelFormat.Bgra : PixelFormat.Rgba;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, format, PixelType.UnsignedByte, pixelData);
        }

        public void CreateOrUpdate()
        {
            Screen.ValidateOpenGLSafe("Texture.CreateOrupdate");

            if (!Created)
                Create();
            else
            {
                ValidateSize();

                GL.BindTexture(TextureTarget.Texture2D, ID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

                IntPtr pixelData = Bitmap.GetPixels();
                var format = ColorType == SKColorType.Bgra8888 ? PixelFormat.Bgra : PixelFormat.Rgba;
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0,
                    format, PixelType.UnsignedByte, pixelData);
            }
        }

        private static int _currentBind;
        public void Bind()
        {
            if (_currentBind == ID)
            {
                return;
            }

            if (!Created)
                return;

            Screen.ValidateOpenGLSafe("Texture.Bind");
            GL.BindTexture(TextureTarget.Texture2D, ID);
            _currentBind = ID;
        }

        public void ClearTexture()
        {
            if (!Loaded)
                throw new InvalidOperationException("Cannot clear a null texture!");

            using (var canvas = new SKCanvas(Bitmap))
            {
                canvas.Clear(SKColors.Transparent);
            }
        }

        public void BlankTexture(int width, int height)
        {
            Bitmap = new SKBitmap(width, height, ColorType, SKAlphaType.Premul);
            ValidateSize();
        }

        public Texture Clone()
        {
            var texture = new Texture
            {
                Name = Name,
                Bitmap = Bitmap.Copy(ColorType), // SKBitmap.Copy() creates a duplicate
                MagFilter = MagFilter,
                MinFilter = MinFilter,
                HasAlpha = HasAlpha,
                ImageHeight = ImageHeight,
                ImageWidth = ImageWidth,
                TextureWidth = TextureWidth,
                TextureHeight = TextureHeight,
                ColorType = ColorType
            };

            return texture;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
