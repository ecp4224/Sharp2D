using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Interfaces
{
    public interface IModule : IDisposable
    {
        void InitializeWith(Sprite sprite);

        void OnUpdate();

        Sprite Owner { get; }

        string ModuleName { get; }

        ModuleRules Rules { get; }
    }

    [Flags]
    public enum ModuleRules
    {
        None = 0,
        OnePerSprite = 1,
        RunOnce = 2
    }
}
