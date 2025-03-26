using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sharp2D.Game
{
    public static class Input
    {
        private const string FilePath = "config/keys.conf";

        public static Keyboard Keyboard = new Keyboard();
        public static Mouse Mouse = new Mouse();
        private static MouseState _mouse;

        static Input()
        {
            if (!File.Exists(FilePath)) return;

            var lines = File.ReadAllLines(FilePath);

            foreach (var l in lines.Where(l => !l.Trim().StartsWith("#")))
            {
                var split = l.Split('=');

                if (split.Length != 2)
                {
                    Logger.Warn("Line \"" + l + "\" in keys.conf could not be parsed.");
                    continue;
                }

                var key = split[0].Trim();
                var value = split[1].Trim();
                ushort parsed;

                if (!UInt16.TryParse(value, out parsed))
                {
                    Logger.Warn("Value \"" + value + "\" in keys.conf is not a valid ushort.");
                    continue;
                }

                if (parsed < 130) { Keyboard.Keys.Add(key, (Keys) (parsed + 1)); } //Keyboard codes
                else if (parsed < 142) { Mouse.Buttons.Add(key, (MouseButton) (parsed - 130)); } //Mouse codes
                else { Logger.Warn("Value \"" + parsed + "\" is not a valid keycode."); } //Invalid codes
            }
        }
        
        internal static void Update(KeyboardState keyboard, MouseState mouse)
        {
            Keyboard._state = keyboard;
            Mouse._state = mouse;
        }

        public static bool IsKeyDown(Keys key)
        {
            return Keyboard.IsKeyDown(key);
        }

        public static bool IsKeyUp(Keys key)
        {
            return Keyboard.IsKeyReleased(key);
        }
        
        public static bool WasKeyPressed(Keys key)
        {
            return Keyboard.WasKeyPressed(key);
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return _mouse.IsButtonDown(button);
        }

        public static Vector2 MousePosition => _mouse.Position;
    }

    public struct KeyboardPressedState
    {
        public readonly bool IsPressed;
        public readonly bool IsDown;
        public readonly bool IsReleased;

        public KeyboardPressedState(bool isPressed, bool isDown, bool isReleased)
        {
            IsPressed = isPressed;
            IsDown = isDown;
            IsReleased = isReleased;
        }

        public KeyboardPressedState()
        {
        }
    }

    public sealed class Keyboard
    {
        internal KeyboardState _state;

        internal Dictionary<string, Keys> Keys = new Dictionary<string, Keys>();

        private static readonly KeyboardPressedState _defaultPressedState = new KeyboardPressedState();
        
        internal Keyboard()
        {
        }

        public KeyboardPressedState this[string keyName]
        {
            get
            {
                if (Keys.TryGetValue(keyName, out var key))
                {
                    bool isDown = _state.IsKeyDown(key);
                    bool isPressed = _state.IsKeyPressed(key);
                    bool isReleased = _state.IsKeyReleased(key);

                    return new KeyboardPressedState(isPressed, isDown, isReleased);
                }

                Logger.Warn("Invalid key name specified: \"" + keyName + "\".");
                return _defaultPressedState;
            }
        }

        public void SetDefaults(Dictionary<string, Keys> defaultMapping)
        {
            foreach (string key in defaultMapping.Keys.Where(key => !Keys.ContainsKey(key)))
            {
                Keys.Add(key, defaultMapping[key]);
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return _state.IsKeyDown(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return _state.IsKeyReleased(key);
        }

        public bool WasKeyPressed(Keys key)
        {
            return _state.IsKeyPressed(key);
        }
    }

    public class MouseMovementState
    {
        public readonly Vector2 Position;
        public readonly Vector2 PreviousPosition;
        public readonly Vector2 PositionDelta;
        public readonly Vector2 Scroll;
        public readonly Vector2 PreviousScroll;
        public readonly Vector2 ScrollDelta;

        public MouseMovementState(Vector2 position, Vector2 previousPosition, Vector2 positionDelta, Vector2 scroll, Vector2 previousScroll, Vector2 scrollDelta)
        {
            Position = position;
            PreviousPosition = previousPosition;
            PositionDelta = positionDelta;
            Scroll = scroll;
            PreviousScroll = previousScroll;
            ScrollDelta = scrollDelta;
        }

        public MouseMovementState()
        {
        }
    }

    public class MousePressedState : MouseMovementState
    {
        public readonly bool IsButtonPressed;
        public readonly bool IsButtonDown;
        public readonly bool IsButtonReleased;

        public MousePressedState(Vector2 position, Vector2 previousPosition, Vector2 positionDelta, Vector2 scroll, Vector2 previousScroll, Vector2 scrollDelta, bool isButtonPressed, bool isButtonDown, bool isButtonReleased) : base(position, previousPosition, positionDelta, scroll, previousScroll, scrollDelta)
        {
            IsButtonPressed = isButtonPressed;
            IsButtonDown = isButtonDown;
            IsButtonReleased = isButtonReleased;
        }

        public MousePressedState(MouseMovementState movement, bool isButtonPressed, bool isButtonDown, bool isButtonReleased) : base(movement.Position, movement.PreviousPosition, movement.PositionDelta, movement.ScrollDelta, movement.PreviousScroll, movement.ScrollDelta)
        {
            IsButtonPressed = isButtonPressed;
            IsButtonDown = isButtonDown;
            IsButtonReleased = isButtonReleased;
        }

        public MousePressedState()
        {
        }
    }

    public sealed class Mouse
    {
        internal MouseState _state;
        internal Dictionary<string, MouseButton> Buttons = new Dictionary<string, MouseButton>();
        private static readonly MousePressedState _defaultState = new MousePressedState();

        internal Mouse()
        {
        }

        public MousePressedState this[string buttonName]
        {
            get
            {
                if (Buttons.TryGetValue(buttonName, out var button))
                {
                    var isDown = _state.IsButtonDown(button);
                    var isPressed = _state.IsButtonPressed(button);
                    var isReleased = _state.IsButtonReleased(button);
                    var movement = GetMovement();

                    return new MousePressedState(movement, isPressed, isDown, isReleased);
                }

                Logger.Warn("Invalid button name specified: \"" + buttonName + "\".");
                return _defaultState;
            }
        }

        public void SetDefaults(Dictionary<string, MouseButton> defaultMapping)
        {
            foreach (string key in defaultMapping.Keys.Where(key => !Buttons.ContainsKey(key)))
            {
                Buttons.Add(key, defaultMapping[key]);
            }
        }

        public MouseMovementState GetMovement()
        {
            var position = _state.Position;
            var previousPosition = _state.PreviousPosition;
            var delta = _state.Delta;
            var scroll = _state.Scroll;
            var previousScroll = _state.PreviousScroll;
            var scrolLDelta = _state.ScrollDelta;

            return new MouseMovementState(position, previousPosition, delta, scroll, previousScroll, scrolLDelta);
        }

        public Vector2 GetMousePosition()
        {
            var state = GetMovement();

            return state.Position;
        }
    }
}