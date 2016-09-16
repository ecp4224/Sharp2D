namespace Sharp2D.Core.Interfaces
{
    public interface INativeSystem
    {
        bool ToggleConsoleWindow(bool show);

        string SystemName { get; }
    }
}