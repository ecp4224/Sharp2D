using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using GLPixelInternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using System.IO;
using System.Threading;
using SkiaSharp;

namespace Sharp2D
{
    public class Texture : ICloneable
    {
        public enum TextureState
        {
            Unloaded,
            Decoding,
            UploadPending,
            Failed,
            Created
        }

        public enum TextureSource
        {
            FileSystem,
            Resources
        }

        public enum TextureBitmapRetentionPolicy
        {
            Dispose,
            Keep
        }

        public enum TextureLoadingPolicy
        {
            LoadOnce,
            AllowReload,
        }
        
        public sealed class TextureStateChangedEventArgs : EventArgs {
            public TextureState OldState { get; init; }
            public TextureState NewState { get; init; }
        }
        
        private static ConcurrentDictionary<string, Texture> cache = new ConcurrentDictionary<string, Texture>();

        public static Texture NewTexture(string resource)
        {
            if (cache.ContainsKey(resource))
                return cache[resource];

            Texture texture = new Texture();
            texture.Name = resource;

            cache.TryAdd(resource, texture);

            return texture;
        }

        private TextureState _state;

        public TextureState State
        {
            get
            {
                return _state;
            }
            private set
            {
                var oldValue = _state;
                _state = value;
                
                StateChanged?.Invoke(this, new TextureStateChangedEventArgs()
                {
                    OldState = oldValue,
                    NewState = _state
                });
            }
        }

        public event EventHandler<TextureStateChangedEventArgs> StateChanged;

        private TextureSource _source;
        public TextureSource Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (State == TextureState.Unloaded)
                    _source = value;
                else
                    throw new AccessViolationException($"Cannot change texture source after its been loaded (in state {State})");
            }
        }

        public int ID { get; private set; }

        public int MinFilter { get; set; }

        public int MagFilter { get; set; }

        public int ImageWidth { get; private set; }

        public int ImageHeight { get; private set; }

        public int TextureWidth { get; private set; }

        public int TextureHeight { get; private set; }

        public SKColorType ColorType { get; set; } = SKColorType.Bgra8888;

        public TextureBitmapRetentionPolicy BitmapRetentionPolicy { get; set; } = TextureBitmapRetentionPolicy.Dispose;

        public TextureLoadingPolicy LoadingPolicy { get; set; } = TextureLoadingPolicy.LoadOnce;

        public GLPixelFormat PixelFormat
        {
            get
            {
                return ColorType switch
                {
                    SKColorType.Bgra8888 => GLPixelFormat.Bgra,
                    SKColorType.Rgba8888 => GLPixelFormat.Rgba,
                    SKColorType.Gray8 => GLPixelFormat.Red,
                    _ => GLPixelFormat.Rgba
                };
            }
        }

        public GLPixelInternalFormat PixelInternalFormat
        {
            get
            {
                return ColorType switch
                {
                    SKColorType.Gray8 => GLPixelInternalFormat.R8,
                    _ => GLPixelInternalFormat.Rgba
                };
            }
        }

        public bool Loaded
        {
            get
            {
                return State == TextureState.UploadPending || State == TextureState.Created || Bitmap != null;
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

        private SKBitmap _bitmap;

        public SKBitmap Bitmap
        {
            get
            {
                ValidateBitmapAccess();
                return _bitmap;
            }
            private set
            {
                ValidateBitmapAccess();
                _bitmap = value;
            }
        }

        private Texture() { ID = -1; MinFilter = (int)TextureMinFilter.Nearest; MagFilter = (int)TextureMagFilter.Nearest; }

        private void ValidateBitmapAccess()
        {
            if (BitmapRetentionPolicy == TextureBitmapRetentionPolicy.Dispose && State == TextureState.Created)
                throw new AccessViolationException(
                    "Bitmap retention policy set to dispose and texture already created");
        }
        
        private void _LoadTextureFromResource()
        {
            Assembly extender = Assembly.GetEntryAssembly();
            Assembly sharp2d = this.GetType().Assembly;

            SKBitmap temp;
            using (Stream stream = extender?.GetManifestResourceStream(Name) ?? sharp2d.GetManifestResourceStream(Name))
            {

                if (stream == null)
                    throw new FileNotFoundException("Could not find resource " + Name);

                // Decode stream into SKBitmap
                temp = SKBitmap.Decode(stream);
            }

            var info = new SKImageInfo(temp.Width, temp.Height, ColorType, SKAlphaType.Premul);
            Bitmap = new SKBitmap(info);
            temp.CopyTo(Bitmap);
            temp.Dispose();
            HasAlpha = Bitmap.ContainsAlpha();
            ValidateSize();
        }

        private void _LoadTextureFromFile()
        {

            SKBitmap temp;
            using (var fs = new FileStream(Name, FileMode.Open))
            {
                using (var codec = SKCodec.Create(fs))
                {
                    var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, ColorType, SKAlphaType.Premul);
                    Bitmap = new SKBitmap(info);
                    codec.GetPixels(info, Bitmap.GetPixels(out _));
                }
            }
            
            HasAlpha = Bitmap.ContainsAlpha();

            ValidateSize();
        }

        private void _LoadTexture()
        {
            switch (Source)
            {
                case TextureSource.FileSystem:
                    _LoadTextureFromFile();
                    break;
                case TextureSource.Resources:
                    _LoadTextureFromResource();
                    break;
            }

            State = TextureState.UploadPending;
        }

        private bool CheckLoadPolicy()
        {
            if (State == TextureState.Unloaded) return true;
            
            if (LoadingPolicy == TextureLoadingPolicy.LoadOnce)
            {
                Logger.Debug($"Skipping already loaded texture {Name}");
                return false;
            }

            State = TextureState.Unloaded;

            return true;
        }

        public void TryLoadTexture(TextureSource source, bool loadInBackground = true)
        {
            if (!CheckLoadPolicy())
                return;

            Source = source;
            LoadTexture(loadInBackground);
        }

        public void LoadTexture(bool loadInBackground = true)
        {
            Screen.ValidateOpenGLUnsafe("Texture.LoadTexture", true);
            
            if (!CheckLoadPolicy())
                return;

            State = TextureState.Decoding;
            
            if (!loadInBackground)
                _LoadTexture();
            else
            {
                ThreadPool.QueueUserWorkItem(_ => { _LoadTexture(); });
            }
        }
        
        [Obsolete("Use Source + LoadTexture or TryLoadTexture(Source) instead")]
        public void LoadTextureFromResource()
        {
            TryLoadTexture(TextureSource.Resources);
        }

        [Obsolete("Use Source + LoadTexture or TryLoadTexture(Source) instead")]
        public void LoadTextureFromFile()
        {
            TryLoadTexture(TextureSource.FileSystem);
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

                var old = Bitmap;
                Bitmap = newBitmap;
                old?.Dispose();
            }
        }

        private bool _isWatching;
        private void WatchTextureStateAndCreate()
        {
            if (_isWatching) return;
            
            this.StateChanged += CheckTextureReady;

            void CheckTextureReady(object sender, TextureStateChangedEventArgs e)
            {
                if (this.State != TextureState.UploadPending) return;
                    
                Screen.Invoke(Create);
                this.StateChanged -= CheckTextureReady;

                _isWatching = false;
            }

            _isWatching = true;
        }

        public void Create()
        {
            switch (State)
            {
                case TextureState.Created:
                    return;
                case TextureState.Failed:
                    throw new AccessViolationException("Upload for texture already failed");
                case TextureState.Unloaded:
                    LoadTexture();
                    WatchTextureStateAndCreate();
                    return;
                case TextureState.Decoding:
                    WatchTextureStateAndCreate();
                    return;
                case TextureState.UploadPending:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                Screen.ValidateOpenGLSafe("Texture.Create");

                ID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, ID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);


                // Get pixel data from SKBitmap using the configured color type.
                IntPtr pixelData = Bitmap.GetPixels();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Bitmap.Width, Bitmap.Height, 0,
                    PixelFormat, PixelType.UnsignedByte, pixelData);

                if (BitmapRetentionPolicy == TextureBitmapRetentionPolicy.Dispose)
                {
                    Bitmap.Dispose();
                    Bitmap = null;
                }
                
                State = TextureState.Created;
            }
            catch
            {
                State = TextureState.Failed;
                throw;
            }
        }

        public void CreateOrUpdate()
        {
            Screen.ValidateOpenGLSafe("Texture.CreateOrupdate");
            ValidateBitmapAccess();

            if (!Created)
                Create();
            else
            {
                ValidateSize();

                GL.BindTexture(TextureTarget.Texture2D, ID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

                IntPtr pixelData = Bitmap.GetPixels();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, Bitmap.Width, Bitmap.Height, 0,
                    PixelFormat, PixelType.UnsignedByte, pixelData);
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
            State = TextureState.UploadPending;
        }

        public Texture Clone(bool cloneBitmap = true)
        {
            var doCloneByRef = !cloneBitmap;
            var nullClone = false;
            if (cloneBitmap)
            {
                if (Bitmap == null)
                {
                    doCloneByRef = true;
                }
                else if (BitmapRetentionPolicy == TextureBitmapRetentionPolicy.Dispose && State == TextureState.Created)
                {
                    doCloneByRef = true;
                    nullClone = true;
                }
            }
            
            var texture = new Texture
            {
                Name = Name,
                Bitmap = doCloneByRef ? nullClone ? null : Bitmap : Bitmap.Copy(ColorType), // SKBitmap.Copy() creates a duplicate
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
