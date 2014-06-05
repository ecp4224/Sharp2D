using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Game.Graphics
{
    public class Texture
    {
        public int ID { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelInternalFormat InternalFormat { get; private set; }

        public PixelFormat Format { get; set; }

        private byte[] ImageData;

        public void LoadFromResource(string Name)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();

            Stream stream = asm.GetManifestResourceStream(Name);

            byte[] data = new byte[stream.Length];

            stream.Read(data, 0, (int)stream.Length);

            stream.Close();
            stream.Dispose();

            this.ImageData = data;
        }

        public void LoadFromFile(string FilePath)
        {
            this.ImageData = File.ReadAllBytes(FilePath);
        }

        public async void LoadFromResourceAsync(string Name)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream stream = asm.GetManifestResourceStream(Name))
                {
                    byte[] chunk = new byte[2024];
                    int read = 0;
                    while (true)
                    {
                        Task<int> result = stream.ReadAsync(chunk, 0, chunk.Length);
                        await result;
                        read = result.Result;

                        if (read > 0)
                        {
                            await ms.WriteAsync(chunk, 0, read);
                        }
                        else break;
                    }
                    stream.Close();
                    stream.Dispose();
                }
                data = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }
            this.ImageData = data;
        }

        public async void LoadFromFileAsync(string FilePath)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream stream = new FileStream(FilePath, FileMode.Open))
                {
                    byte[] chunk = new byte[2024];
                    int read = 0;
                    while (true)
                    {
                        Task<int> result = stream.ReadAsync(chunk, 0, chunk.Length);
                        await result;
                        read = result.Result;

                        if (read > 0)
                        {
                            await ms.WriteAsync(chunk, 0, read);
                        }
                        else break;
                    }
                    stream.Close();
                    stream.Dispose();
                }
                data = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }

            this.ImageData = data;
        }

        private void _fetchData()
        {
            if (ImageData == null || ImageData.Length == 0) return;
            MemoryStream stream = new MemoryStream();
            stream.Write(ImageData, 0, ImageData.Length);

            Bitmap _temp = new Bitmap(stream);
            Width = _temp.Width;
            Height = _temp.Height;
            InternalFormat = PixelInternalFormat.Rgba; //TODO Check if to use RGB or RGBA
        }

        public void Create()
        {
            Sharp2D.Core.Graphics.Screen.ValidateThreadState("Texture.Create");

            ID = GL.GenTexture();

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { 9728 });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { 9728 });

            PixelFormat type = Format != null ? Format : PixelFormat.Rgba;

            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, type, PixelType.UnsignedByte, ImageData);
        }
    }
}
