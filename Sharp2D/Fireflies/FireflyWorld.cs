using Sharp2D;
using Sharp2D.Fonts;
using Sharp2D.Text;
using OpenTK.Mathematics;
using Sharp2D.Render;
using SkiaSharp;

namespace Fireflies
{
    public class FireflyWorld : GenericWorld
    {
        public static int FireflyCount = 100;
        public const int CloudCount = 100;
        private Light _lightingLight;

        private static readonly Random Rand = new Random();
        public override string Name
        {
            get { return "worlds/fireflys.json"; }
        }

        protected override void OnInitialDisplay()
        {
            base.OnInitialDisplay();

            Sprite moon = Sprite.FromImage("sprites/moon.png");
            moon.X = (3.5f * 16f) + (moon.Width / 2f);
            moon.Y = (7.5f * 16f) + (moon.Height / 2f) + 3f;
            moon.Layer = 0.5f;
            moon.IgnoreLights = true;
            moon.NeverClip = true;
            AddSprite(moon);

            var sprite = new BackgroundSprite();
            sprite.X = sprite.Width / 2f;
            sprite.Y = sprite.Height / 2f;
            sprite.Layer = 1;
            AddSprite(sprite);
            for (int i = 0; i < FireflyCount; i++)
            {
                var fly = new Firefly {X = Rand.Next(4, 40)*16f, Y = Rand.Next(8, 25)*16f};

                AddLogical(fly);

                AddLight(fly);
            }

            for (int i = 0; i < CloudCount; i++)
            {
                var c = new Cloud {X = Rand.Next(-10, 50)*16f, Y = 8f*16f, Layer = (float) Rand.NextDouble(), IgnoreLights = true, Alpha = (float) Rand.NextDouble()};
                AddSprite(c);
            }

            _lightingLight = new Light(this.PixelWidth / 2f, 0f, OriginalIntensity, 750f, new SKColor(36, 42, 255), LightType.StaticPointLight)
            {
                Intensity = 0f
            };

            AddLight(_lightingLight);
            AddLogical(Lighting);

            var font = SdfFont.Load("sprites/font.fnt", "sprites/font.png");
            _fpsText = this.AddTextSprite(font, "FPS: 0");
            _fpsText.SetScale(1f);
            _fpsText.SetColor(SKColors.Yellow);
            _fpsText.SetPosition(new Vector2(5f, PixelHeight - font.LineHeight));
            AddLogical(UpdateFps);
        }

        private bool _started = false;
        private int _count = 1;
        private int _cur = 0;
        private long _timeStarted;
        private long _duration;
        private long _wait = 2500;
        private long _lastEnd = CurrentTimeMillis();
        private const float OriginalIntensity = 20f;

        private TextSprite _fpsText;
        private int _fpsCounter;
        private long _fpsLast;

        private void UpdateFps()
        {
            _fpsCounter++;
            long now = CurrentTimeMillis();
            if (now - _fpsLast >= 1000)
            {
                _fpsText.SetText("FPS: " + _fpsCounter);
                _fpsCounter = 0;
                _fpsLast = now;
            }
        }

        private void Lighting()
        {
            if (CurrentTimeMillis() - _lastEnd >= _wait && Rand.NextDouble() > 0.95 && !_started)
            {
                _count = Rand.Next(1, 5);
                _started = true;
                _cur = 0;
            }
            else if (_started)
            {
                if (_cur < _count && _lightingLight.Intensity == 0f)
                {
                    _cur++;
                    _duration = Rand.Next(50, 150);
                    _timeStarted = CurrentTimeMillis();
                    _lightingLight.Intensity = 0.001f;
                }
                else if (_lightingLight.Intensity > 0f)
                {
                    long curTime = CurrentTimeMillis();

                    if (curTime <= _timeStarted + _duration)
                    {
                        long dif = curTime - _timeStarted;

                        float percent = (dif/(float)_duration);

                        _lightingLight.Intensity = OriginalIntensity*percent;
                    }
                    else if (_cur + 1 >= _count && curTime > _timeStarted + _duration)
                    {
                        long dif = curTime - (_timeStarted + _duration);

                        float percent = (dif / (float)(_duration * 4));

                        _lightingLight.Intensity = OriginalIntensity - (OriginalIntensity * percent);

                        if (dif < _duration*4) return;
                        _lightingLight.Intensity = 0f;
                        _started = false;
                        _wait = Rand.Next(500, 3500);
                        _lastEnd = CurrentTimeMillis();
                    }
                    else if (_cur + 1 < _count && curTime > _timeStarted + _duration)
                    {
                        _lightingLight.Intensity = 0f;
                    }
                }
            }
        }

        private static readonly DateTime Jan1St1970 = new DateTime
    (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
    }
}
