using Sharp2D;
using Sharp2D.Core.Graphics;
using Sharp2D.Game;
using SkiaSharp;

namespace SomeGame
{
    public sealed class Player : PhysicsSprite
    {
        private AnimationModule animations;
        private float xvel, yvel;
        public override string Name
        {
            get { return "player"; }
        }

        public Player()
        {
            NeverClip = true;
            Layer = -1f;

            Texture = Texture.NewTexture("sprites/player.png");
            Texture.LoadTextureFromFile();
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;

            animations = AttachModule<AnimationModule>();
            HitboxConfigPath = "sprites/player_hitbox.json";
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnDisplay()
        {
        }

        private int lastXFacing;
        private bool autoMove;
        private bool ignoreAll;
        private int autoMoveX, autoMoveY;

        private bool fadeOutStarted;
        private bool nextWorld;
        public override void Update()
        {
            base.Update();
            if (!ignoreAll)
            {
                int xadd = 0;
                int yadd = 0;

                if (!autoMove)
                {
                    if (Input.Keyboard["moveLeft"].IsDown)
                        xadd = 1;
                    if (Input.Keyboard["moveRight"].IsDown)
                        xadd = -1;
                    if (Input.Keyboard["moveUp"].IsDown)
                        yadd = -1;
                    if (Input.Keyboard["moveDown"].IsDown)
                        yadd = 1;
                }
                else
                {
                    xadd = autoMoveX;
                    yadd = autoMoveY;
                }

                if ((xadd != 0 || yadd != 0) && xvel == 0f && yvel == 0f &&
                    animations.CurrentlyPlayingAnimation.Name != "handToPocket")
                {
                    animations.Animations["handToPocket"].Reset().Play()
                        .OnAnimationComplete(() =>
                        {
                            xvel = xadd*3f;
                            yvel = yadd*3f;
                        });
                }
                else if ((xvel != 0f || yvel != 0f) && animations.CurrentlyPlayingAnimation.Name != "walk")
                {
                    animations.Animations["walk"].Reset().Play();
                }

                if (xadd == 0 && yadd == 0 && (xvel != 0 || yvel != 0))
                {
                    animations.Animations["pocketToHand"].Reset()
                        .Play()
                        .OnAnimationComplete(() => animations.Animations["idle"].Play());
                    xvel = 0;
                    yvel = 0;
                }
                else if ((xvel != 0 || yvel != 0))
                {
                    xvel = xadd*3f;
                    yvel = yadd*3f;
                    lastXFacing = xadd;
                }

                FlipState = lastXFacing < 0 ? FlipState.Horizontal : FlipState.None;

                X += xvel;
                Y += yvel;
            }

            if (Y > 54*32)
                Y = 54*32;

            if (!nextWorld)
            {
                if (Y < 5*32)
                {
                    autoMove = true;
                    autoMoveX = 0;
                    autoMoveY = -1;
                }

                if (Y < 0 && !fadeOutStarted)
                {
                    fadeOutStarted = true;
                    var world = CurrentWorld as GameWorld;
                    if (world != null)
                    {
                        world.FadeOut(() => new Thread(NextWorld).Start());
                    }
                }
            }
            else
            {
                if (X > 9*32 && X < 12*32 && Y <= 32*31)
                {
                    autoMove = true;
                    ignoreAll = true;
                    autoMoveX = 0;
                    autoMoveY = 0;

                    animations.Animations["faceFront"].Play().OnAnimationComplete(() =>
                    {
                        CurrentWorld.Camera.ClearFollowing();
                        CurrentWorld.Camera.PanTo(X, 26*32, PanType.Smooth, 3500);
                    });
                } 
            }
        }

        private void NextWorld()
        {
            var newworld = new EndWorld { AmbientBrightness = 0.0f };
            newworld.player = this;
            newworld.Load();
            newworld.Display();
            nextWorld = true;
            newworld.Camera.Follow2D(this);
            newworld.AddSprite(this);
            autoMove = false;
            newworld.Camera.Bounds = new SKRect(-12 * 32, 0, -11 * 32, 38 * 32);    
            newworld.Camera.X = X = 3 * 32f;
            newworld.Camera.Y = Y = 42 * 32f;
            newworld.Camera.Z = 200;
        }
    }
}
