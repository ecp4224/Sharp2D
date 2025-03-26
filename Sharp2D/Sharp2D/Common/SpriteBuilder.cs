using System;
using System.Collections.Generic;
using Sharp2D.Core;
using SkiaSharp;

namespace Sharp2D.Common
{
    /*public class Test
    {
        public static void TestMethod()
        {

            SpriteBuilder builder = new SpriteBuilder();

            Sprite s = builder
                        .MakeAndEditRectangle(300, 100)
                            .MakeAndEditText("Test")
                                .Align(Position.Center)
                                .SetColor(Color.White)
                                .Parent()
                            .SetColor(Color.Black)
                            .BuildSprite();

            myWorld.AddSprite(s);
            
        }
    }*/



    public abstract class Builder
    {
        protected Queue<Action<SKCanvas>> actionQueue = new Queue<Action<SKCanvas>>();
        public int X, Y, Width, Height;
        public SKColor  Color = SKColors.Black;
        private Builder parent;

        public Builder(Builder parent)
        {
            this.parent = parent;
        }

        public RectangleObject MakeAndEditRectangle(int width, int height)
        {
            RectangleObject obj = new RectangleObject(width, height, this);
            AddAction(delegate(SKCanvas g)
            {
                obj.Build(g);
            });
            return obj;
        }

        public TextObject AddText(string text)
        {
            TextObject obj = new TextObject(text, this);
            AddAction(delegate(SKCanvas g)
            {
                obj.Build(g);
            });
            return obj;
        }

        public virtual Builder SetColor(SKColor color)
        {
            this.Color = color;

            return this;
        }

        public virtual Builder Align(Placement position)
        {
            if (parent != null)
            {
                int newX = parent.X;
                int newY = parent.Y;

                if ((position & Placement.Left) != 0)
                {
                    newX -= (parent.Width / 2);
                }
                if ((position & Placement.Right) != 0)
                {
                    newX += (parent.Width / 2);
                }
                if ((position & Placement.Top) != 0)
                {
                    newY -= (parent.Height / 2);
                }
                if ((position & Placement.Bottom) != 0)
                {
                    newY += (parent.Height / 2);
                }

                X = newX;
                Y = newY;
            }
            return this;
        }
        
        public Builder AddAction(Action<SKCanvas> action)
        {
            actionQueue.Enqueue(action);
            return this;
        }

        public virtual Builder Parent()
        {
            return parent;
        }

        public abstract void Build(SKCanvas g);
    }

    public class BuiltSprite : Sprite
    {
        public BuiltSprite(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
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

        public override string Name
        {
            get { return "wat"; }
        }
    }

    public class SpriteBuilder : Builder
    {
        public SpriteBuilder()
            : base(null)
        {

        }

        public Sprite BuildSprite(string spriteName, int Width, int Height)
        {
            BuiltSprite sprite = new BuiltSprite(Width, Height);



            Texture texture = Texture.NewTexture(spriteName + "_autobuild");
            texture.BlankTexture(Width, Height);

            // Get the SKBitmap from the texture.
            SKBitmap image = texture.Bitmap;

            // Use an SKCanvas to perform the queued drawing actions.
            using (var canvas = new SKCanvas(image))
            {
                while (actionQueue.Count > 0)
                {
                    actionQueue.Dequeue().Invoke(canvas);
                }
            }

            sprite.Texture = texture;
            
            return sprite;
        }

        public override void Build(SKCanvas g)
        {
            throw new NotImplementedException();
        }

        public override Builder Parent()
        {
            return this;
        }
    }

    public class RectangleObject : Builder
    {
        internal RectangleObject(int width, int height, Builder parent) : base(parent)
        {
            this.Width = width;
            this.Height = height;
        }

        public override void Build(SKCanvas canvas)
        {
            using (var paint = new SKPaint())
            {
                paint.Color = this.Color;
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                SKRect rect = new SKRect(X, Y, X + Width, Y + Height);
                canvas.DrawRect(rect, paint);
            }
        }
    }

    public class TextObject : Builder
    {
        private string text;
        private SKTextAlign  alignment = SKTextAlign.Left;
        internal TextObject(string text, Builder parent) : base(parent)
        {
            this.text = text;
        }

        public override Builder Align(Placement position)
        {
            if ((position & Placement.Left) != 0)
            {
                alignment = SKTextAlign.Left;
            }
            else if ((position & Placement.Right) != 0)
            {
                alignment = SKTextAlign.Right;
            }
            else if ((position & Placement.Center) != 0)
            {
                alignment = SKTextAlign.Center;
            }

            return this;
        }

        public override void Build(SKCanvas canvas)
        {
            // Basic implementation: draw text at (X, Y) with current Color.
            using (var paint = new SKPaint())
            {
                paint.Color = this.Color;
                paint.TextSize = 16; // Default text size; consider making this configurable.
                paint.IsAntialias = true;
                paint.TextAlign = alignment;
                // For simplicity, draw at (X, Y). More advanced rendering might measure text.
                canvas.DrawText(text, X, Y, paint);
            }
        }
    }
}
