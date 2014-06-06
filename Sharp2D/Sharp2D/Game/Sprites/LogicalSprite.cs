using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Logic;

namespace Sharp2D.Game.Sprites
{
    public abstract class LogicalSprite : Sprite, ILogical
    {
        public abstract void Update();
    }
}