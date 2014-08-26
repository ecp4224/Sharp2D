using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Sharp2D
{
    public class Texture
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

        public Bitmap Bitmap { get; private set; }

        private Texture() { ID = -1; MinFilter = (int)TextureMinFilter.Nearest; MagFilter = (int)TextureMagFilter.Nearest; }

        public void LoadTextureFromResource()
        {
            Screen.ValidateOpenGLUnsafe("Texture.LoadTextureFromResource", true);
            Assembly extender = Assembly.GetEntryAssembly();
            Assembly sharp2d = this.GetType().Assembly;

            Stream stream = extender.GetManifestResourceStream(Name);
            if (stream == null)
                stream = sharp2d.GetManifestResourceStream(Name);

            Bitmap = new Bitmap(stream, false);

            HasAlpha = Bitmap.ContainsAlpha();

            ValidateSize();
        }

        public void LoadTextureFromFile()
        {
            Screen.ValidateOpenGLUnsafe("Texture.LoadTextureFromFile", true);
            
            using (var fs = new System.IO.FileStream(Name, System.IO.FileMode.Open))
            {
                Bitmap = new Bitmap(fs);
            }

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
                Bitmap bmp = new Bitmap(TextureWidth, TextureHeight);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                g.DrawImage(Bitmap, new RectangleF(0f, 0f, ImageWidth, ImageHeight), new RectangleF(0f, 0f, ImageWidth, ImageHeight), GraphicsUnit.Pixel);
                g.Dispose();
                Bitmap = bmp;
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

            BitmapData bmp = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp.Scan0);

            Bitmap.UnlockBits(bmp);
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

                BitmapData bmp = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp.Scan0);

                Bitmap.UnlockBits(bmp);
            }
        }

        private bool CheckAlphaChannel(BitmapData bmpData)
        {
            unsafe
            {
                byte* ptrAlpha = ((byte*)bmpData.Scan0.ToPointer()) + 3;
                for (int i = bmpData.Width * bmpData.Height; i > 0; --i)  // prefix-- should be faster
                {
                    if (*ptrAlpha < 255)
                        return true;

                    ptrAlpha += 4;
                }
            }
            return false;
        }

        private static int currentBind;
        public void Bind()
        {
            if (currentBind == ID)
            {
                return;
            }

            if (!Created)
                return;

            Screen.ValidateOpenGLSafe("Texture.Bind");
            GL.BindTexture(TextureTarget.Texture2D, ID);
            currentBind = ID;
        }

        public void ClearTexture()
        {
            using (var g = System.Drawing.Graphics.FromImage(Bitmap))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255, 255)))
                {
                    g.FillRectangle(br, 0, 0, Bitmap.Width, Bitmap.Height);
                }
            }
        }

        public void BlankTexture(int Width, int Height)
        {
            Bitmap = new Bitmap(Width, Height);
            ValidateSize();
        }
    }
}
