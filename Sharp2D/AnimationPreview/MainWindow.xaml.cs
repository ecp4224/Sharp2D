using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sharp2D.Core.Graphics;
using Sharp2D.Game.Sprites;
using Sharp2D.Game.Sprites.Animations;
using AnimationPreview.Preview;
using Sharp2D.Game.Worlds;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace AnimationPreview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        string json;
        string image;
        TempSprite sprite;
        EmptyWorld world;
        FileSystemWatcher watcher;
        Thread updater;
        bool run = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SpriteRenderJob.SetDefaultJob<OpenGL3SpriteRenderJob>();

            Screen.DisplayScreenAsync();

            updater = new Thread(new ThreadStart(delegate
            {
                string otext = "";
                while (run)
                {
                    try
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            if (otext != code.Text)
                            {
                                Button_Click(null, null);

                                otext = code.Text;
                            }
                        }));

                        Thread.Sleep(1000);
                    }
                    catch
                    {

                    }
                }
            }));
            updater.Start();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            run = false;
            updater.Interrupt();

            Screen.TerminateScreen();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(json) && !string.IsNullOrWhiteSpace(code.Text))
            {
                File.WriteAllText(json, code.Text);
                if (sprite != null)
                {
                    sprite.ClearAnimations();
                }
            }

            json = json_path.Text;
            image = sheet_path.Text;

            if (sprite == null)
            {
                sprite = new TempSprite();
            }
            if (world == null)
            {
                world = new EmptyWorld();
                world.Load();
                world.Display();
            }
            if (watcher == null)
            {
                WatchImageFile(image);
            }

            sprite.TexPath = image;
            sprite.JsonPath = json;
            
            if (world.Sprites.Contains(sprite))
            {
                world.RemoveSprite(sprite);
            }
            world.AddSprite(sprite);

            code.Text = File.ReadAllText(json);

            //sprite.Y = 600;

            sheet_preview.Source = ToBitmapSource(sprite.Texture.Bitmap);

            Screen.Camera.Z = 200;

            animations.Items.Clear();

            for (int row = 0; row < sprite.Animations.Rows; row++)
            {
                animations.Items.Add(sprite.Animations[row].Name);
            }

            //Screen.Camera.Y = 630f;
        }

        void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (animations.SelectedIndex != -1)
            {
                sprite.Animations[(string)animations.SelectedItem].Play();
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private void WatchImageFile(string path)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = new FileInfo(image).Directory.FullName;

            watcher.Changed += watcher_Changed;

            watcher.NotifyFilter = NotifyFilters.LastWrite;

            watcher.EnableRaisingEvents = true;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == image)
            {
                sprite.TexPath = image;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    sheet_preview.Source = ToBitmapSource(sprite.Texture.Bitmap);
                }));
            }
        }

        public static ImageSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            IntPtr hBitmap = source.GetHbitmap();
            ImageSource isource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //source.Dispose();
            DeleteObject(hBitmap);

            return isource;
        }
    }
}
