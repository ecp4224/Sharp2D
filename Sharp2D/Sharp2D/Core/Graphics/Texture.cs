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

namespace Sharp2D.Core.Graphics
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

        public string Name { get; private set; }

        public Bitmap Bitmap { get; private set; }

        private Texture() { ID = -1; MinFilter = (int)TextureMinFilter.Nearest; MagFilter = (int)TextureMagFilter.Nearest; }

        public void LoadTextureFromResource()
        {
            Assembly asm = Assembly.GetEntryAssembly();

            Stream stream = asm.GetManifestResourceStream(Name);

            Bitmap = new Bitmap(stream, false);
        }

        public void LoadTextureFromFile()
        {
            Bitmap = new Bitmap(Name, false);

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
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(Bitmap, new Point(0, 0));
                g.Dispose();
                Bitmap = bmp;
            }
        }

        public void Create()
        {
            ID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

            BitmapData bmp = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp.Scan0);

            Bitmap.UnlockBits(bmp);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }
    }
}
