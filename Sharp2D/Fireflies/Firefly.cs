using OpenTK.Windowing.GraphicsLibraryFramework;
using Sharp2D;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game;
using SkiaSharp;

namespace Fireflies
{
    public class Firefly : Light, ILogical
    {
        private bool litup = false;
        private long duration;
        private long start;
        private long lastChange = 0;
        private long nextChange = 0;

        private float real_y;
        private double tick = 0;
        private float height = 20;
        private int direction = 1;
        private float speed = 3f;

        private static readonly Random random = new Random();

        public void Update()
        {
            if (Input.IsKeyDown(Keys.Escape))
            {
                Screen.TerminateScreen();
                return;
            }

            Lighting();
            Movement();
        }

        private void Movement()
        {
            tick += 0.8;
            Y = real_y + (float)(Math.Cos(tick * (1f / 8f)) * height);

            X += direction * ((speed * (1 / (10 - (Radius + 1)))) + (float)Math.Sin(tick * (1 / random.Next(4, 8))) * 4f);
        }

        private void Lighting()
        {
            if (Screen.TickCount >= lastChange + nextChange)
            {
                speed = random.Next(1, 4);
                litup = !litup;
                duration = random.Next(800);
                start = lastChange = Screen.TickCount;
                nextChange = duration + random.Next(-300, 800);
            }

            if (litup && Intensity < 3f)
            {
                Intensity = ease(0, 3f, duration, Screen.TickCount - start);
            }
            else if (!litup && Intensity > 0f)
            {
                Intensity = ease(3f, 0, duration, Screen.TickCount - start);
                if (Intensity == 0)
                {
                    height = random.Next(5, 25);
                    real_y += random.Next(-8, 8);
                }
                if (Intensity == 0 && (random.NextDouble() < 0.3 || X > 40f * 16f || X < 4f * 16f))
                    direction = -direction;
            }
        }

        public Firefly() : base(0, 0, 0f, 10f, SKColors.Gold, LightType.DynamicPointLight)
        {
            
        }

        public override void Load()
        {
            base.Load();

            nextChange = random.Next(2000);
            lastChange = Screen.TickCount;

            tick = random.Next();

            real_y = Y;

            Intensity = 0f;
            Color = SKColors.Gold;

            Radius = random.Next(4, 10);
        }

        //Code taken from: https://code.google.com/p/replicaisland/source/browse/trunk/src/com/replica/replicaisland/Lerp.java?r=5
        //Because I'm a no good dirty scrub
        public static float ease(float start, float target, float duration, float timeSinceStart)
        {
            float value = start;
            if (timeSinceStart > 0.0f && timeSinceStart < duration)
            {
                float range = target - start;
                float percent = timeSinceStart / (duration / 2.0f);
                if (percent < 1.0f)
                {
                    value = start + ((range / 2.0f) * percent * percent * percent);
                }
                else
                {
                    float shiftedPercent = percent - 2.0f;
                    value = start + ((range / 2.0f) *
                            ((shiftedPercent * shiftedPercent * shiftedPercent) + 2.0f));
                }
            }
            else if (timeSinceStart >= duration)
            {
                value = target;
            }
            return value;
        }

        public void Dispose()
        {
        }
    }
}
