using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Sharp2D.Core;

namespace Sharp2D.Common
{
    public class Test
    {
        public static void TestMethod()
        {
            /*
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
            */
        }
    }



    public abstract class Builder
    {
        protected Queue<Action<Graphics>> actionQueue = new Queue<Action<Graphics>>();
        public int X, Y, Width, Height;
        public Color Color = Color.Black;
        private Builder parent;

        public Builder(Builder parent)
        {
            this.parent = parent;
        }

        public RectangleObject MakeAndEditRectangle(int width, int height)
        {
            RectangleObject obj = new RectangleObject(width, height, this);
            AddAction(delegate(Graphics g)
            {
                obj.Build(g);
            });
            return obj;
        }

        public TextObject AddText(string text)
        {
            TextObject obj = new TextObject(text, this);
            AddAction(delegate(Graphics g)
            {
                obj.Build(g);
            });
            return obj;
        }

        public virtual Builder SetColor(Color color)
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
        
        public Builder AddAction(Action<Graphics> action)
        {
            actionQueue.Enqueue(action);
            return this;
        }

        public virtual Builder Parent()
        {
            return parent;
        }

        public abstract void Build(Graphics g);
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

            Bitmap image = texture.Bitmap;

            using (Graphics g = Graphics.FromImage(image))
            {
                while (base.actionQueue.Count > 0)
                {
                    base.actionQueue.Dequeue()(g);
                }
            }

            sprite.Texture = texture;

            return sprite;
        }

        public override void Build(Graphics g)
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

        public override void Build(Graphics g)
        {
            using (Brush b = new SolidBrush(base.Color))
            {
                g.FillRectangle(b, new Rectangle(X, Y, Width, Height));
            }
        }
    }

    public class TextObject : Builder
    {
        private string text;
        private StringAlignment alignment;
        internal TextObject(string text, Builder parent) : base(parent)
        {
            this.text = text;
        }

        public override Builder Align(Placement position)
        {
            if ((position & Placement.Left) != 0)
            {
                alignment = StringAlignment.Near;
            }
            else if ((position & Placement.Right) != 0)
            {
                alignment = StringAlignment.Far;
            }
            else if ((position & Placement.Center) != 0)
            {
                alignment = StringAlignment.Center;
            }

            return this;
        }

        public override void Build(Graphics g)
        {
            
        }
    }
}
