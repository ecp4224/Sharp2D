using System;

namespace Sharp2D
{
    public partial class Sprite
    {
        public static Sprite FromImage(string imagePath)
        {
            return new SimpleSprite(imagePath);
        }

        public static Sprite FromTexture(Texture texture)
        {
            if (!texture.Loaded)
                throw new InvalidOperationException("Cannot create sprite from null texture!");

            var sprite = new BlankSprite(texture.Name)
            {
                Texture = texture,
                Width = texture.TextureWidth,
                Height = texture.TextureHeight
            };

            return sprite;
        }

        public class BlankSprite : Sprite
        {
            private readonly string _name;

            public BlankSprite(string name)
            {
                _name = name;
            }

            public override string Name
            {
                get { return _name; }
            }

            protected override void BeforeDraw()
            {
            }

            protected override void OnLoad()
            {
            }

            protected override void OnUnload()
            {
            }

            protected override void OnDispose()
            {
            }

            protected override void OnDisplay()
            {
            }
        }

        public class SimpleSprite : Sprite
        {
            private string _imagePath;
            public string ImagePath
            {
                get
                {
                    return _imagePath;
                }
                set
                {
                    _imagePath = value;
                    Texture = Texture.NewTexture(_imagePath);
                    if (!Texture.Loaded)
                        Texture.LoadTextureFromFile();

                    Width = Texture.TextureWidth;
                    Height = Texture.TextureHeight;
                }
            }

            public SimpleSprite(string imagePath)
            {
                ImagePath = imagePath;
            }

            public override string Name
            {
                get { return "simple_sprite@" + _imagePath; }
            }

            protected override void BeforeDraw()
            {
            }

            protected override void OnLoad()
            {
            }

            protected override void OnUnload()
            {
            }

            protected override void OnDispose()
            {
            }

            protected override void OnDisplay()
            {
            }
        }
    }
}
