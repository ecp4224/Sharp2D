using System;
using System.Collections.Generic;
using Sharp2D;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Sharp2D.Game;
using Sharp2D.Game.Worlds;
using Sharp2D.Core.Interfaces;

namespace Sharp2D.UI
{
    /// <summary>
    /// Root container that owns and renders a UI tree inside a <see cref="GenericWorld"/>.
    /// </summary>
    public class UICanvas : UIContainer
    {
        private readonly GenericWorld _world;
        private readonly GuiRenderJob _guiRenderJob;
        private readonly HashSet<Sprite> _registeredSprites = new HashSet<Sprite>();
        private bool _layoutRequested = true;
        private bool _lastLeftDown;

        public GenericWorld World
        {
            get
            {
                return _world;
            }
        }

        public float BaseLayer { get; set; } = 1000f;

        public UICanvas(GenericWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _guiRenderJob = world.GuiRenderJob ?? new GuiRenderJob(world);
            if (!_world.HasJob(_guiRenderJob))
            {
                _world.AddRenderJob(_guiRenderJob);
            }

            var size = Screen.Settings.GameSize;
            Size = new Vector2(size.Width, size.Height);

            AttachToCanvas(this);
        }

        public void Update(double deltaSeconds)
        {
            if (_layoutRequested || RequiresLayout)
            {
                Arrange(Vector2.Zero);
                _layoutRequested = false;
            }

            var mouse = Input.Mouse.GetMovement();
            bool leftDown = Input.IsMouseButtonDown(MouseButton.Left);
            var input = new UIInputState(mouse.Position, leftDown, leftDown && !_lastLeftDown, !leftDown && _lastLeftDown);
            _lastLeftDown = leftDown;

            var context = new UIUpdateContext(deltaSeconds, input);
            base.Update(context);
        }

        internal override void AttachToCanvas(UICanvas canvas)
        {
            base.AttachToCanvas(canvas);
        }

        internal void RequestLayout()
        {
            _layoutRequested = true;
        }

        internal void RegisterSprite(UIElement owner, Sprite sprite)
        {
            if (_registeredSprites.Add(sprite))
            {
                sprite.SpacePlacement = SpaceType.Screen;
                sprite.IgnoreLights = true;
                sprite.NeverClip = true;
                sprite.Layer = BaseLayer + owner.ZIndex;
                _guiRenderJob.AddSprite(sprite);
            }
        }

        internal void RemoveRegisteredSprite(Sprite sprite)
        {
            if (_registeredSprites.Remove(sprite))
            {
                _guiRenderJob.RemoveSprite(sprite);
            }
        }

        internal void UpdateSpriteLayers(UIElement element)
        {
            var targetLayer = BaseLayer + element.ZIndex;
            foreach (var sprite in element.RegisteredSprites)
            {
                sprite.Layer = targetLayer;
            }
        }

        public override void Dispose()
        {
            foreach (var sprite in _registeredSprites)
            {
                _guiRenderJob.RemoveSprite(sprite);
            }
            _registeredSprites.Clear();

            base.Dispose();
        }
    }
}
