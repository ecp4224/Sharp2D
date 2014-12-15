using System.Collections.Generic;

namespace Sharp2D.Core.Interfaces
{
    public interface ILightWorld
    {
        IList<Light> Lights { get; }
        void UpdateSpriteLights(Sprite sprite);
    }
}
