using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Game.Sprites
{
    /// <summary>
    /// <para>This is a class that represents a sprite that is nothing. It is null, void. It was and forever will be nothing.</para>
    /// <para>This sprite can not be added to a world, it cannot be added to a render job, and it cannot be drawn. This is because it is null</para>
    /// <para>Use this object when you need to represent an empty sprite where null can't be used</para>
    /// </summary>
    public class NullSprite : Sprite
    {
        ~NullSprite()
        {
        }

        protected override void BeforeDraw()
        {
            throw new InvalidOperationException("This sprite can not be drawn, it's null!");
        }

        protected override void OnLoad()
        {
            throw new InvalidOperationException("This sprite can't be added to a render job, it's null!");
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDispose()
        {
        }

        protected override void OnDisplay()
        {
            throw new InvalidOperationException("This sprite can't be displayed, it's null!");
        }

        public override string Name
        {
            get { return "null"; }
        }
    }
}
