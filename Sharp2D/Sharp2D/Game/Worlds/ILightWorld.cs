using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Logic;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public interface ILightWorld
    {
        IList<Light> Lights { get; }
        void UpdateSpriteLights(Sprite sprite);
    }
}
