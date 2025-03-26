using Sharp2D;

namespace Fireflies
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("How many should I make? ");
            FireflyWorld.FireflyCount = int.Parse(Console.ReadLine());
            
            Screen.DisplayScreen(() =>
            {
                MusicPlayer.Play("bg.ogg");
                
                var world = new FireflyWorld();
                world.Load();
                world.Camera.Z = 200;
                world.Camera.X = -(24 * 16f);
                world.Camera.Y = 18 * 16f;

                GlobalSettings.EngineSettings.ShowConsole = true;

                world.AmbientBrightness = 0.4f;

                world.Display();
            });
        }
    }
}
