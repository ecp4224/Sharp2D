using OpenTK.Mathematics;

namespace Sharp2D.UI
{
    /// <summary>
    /// Describes the input state relevant to UI interactions for the current frame.
    /// </summary>
    public readonly struct UIInputState
    {
        public Vector2 MousePosition { get; }
        public bool IsLeftDown { get; }
        public bool LeftPressed { get; }
        public bool LeftReleased { get; }

        public UIInputState(Vector2 mousePosition, bool isLeftDown, bool leftPressed, bool leftReleased)
        {
            MousePosition = mousePosition;
            IsLeftDown = isLeftDown;
            LeftPressed = leftPressed;
            LeftReleased = leftReleased;
        }
    }

    /// <summary>
    /// Context provided to UI elements during their update passes.
    /// </summary>
    public readonly struct UIUpdateContext
    {
        public double DeltaSeconds { get; }
        public UIInputState Input { get; }

        public UIUpdateContext(double deltaSeconds, UIInputState input)
        {
            DeltaSeconds = deltaSeconds;
            Input = input;
        }
    }
}
