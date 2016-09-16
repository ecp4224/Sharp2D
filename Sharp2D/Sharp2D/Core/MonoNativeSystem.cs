using Sharp2D.Core.Interfaces;

namespace Sharp2D.Core
{
    public class MonoNativeSystem : INativeSystem
    {
        public bool ToggleConsoleWindow(bool show)
        {
            return false; //NOT SUPPORTED
        }

        public string SystemName
        {
            get { return "Mono"; }
        }
    }
}