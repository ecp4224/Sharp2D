using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenTK;
using Sharp2D;
using Sharp2D.Game.Sprites;
using AnimationPreview.Preview;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Drawing;
using Ookii.Dialogs.Wpf;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        internal static MainWindow WINDOW;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WINDOW = this;

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
            //updater.Start();
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
                    sprite.AnimationModule.ClearAnimations();
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
            sprite.HitboxConfigPath = hitbox_path.Text;

            original_texture = null;
            
            if (world.Sprites.Contains(sprite))
            {
                world.RemoveSprite(sprite);
            }
            world.AddSprite(sprite);

            Screen.Camera.Z = 200;

            code.Text = File.ReadAllText(json);
        }

        public void CompleteReload()
        {
            Screen.ValidateOpenGLSafe("CompleteReload()");

            Dispatcher.BeginInvoke(new Action(delegate
            {
                sheet_preview.Source = ToBitmapSource(sprite.Texture.Bitmap);

                animations.Items.Clear();

                for (int row = 0; row < sprite.AnimationModule.Animations.Rows; row++)
                {
                    animations.Items.Add(sprite.AnimationModule.Animations[row].Name);
                }

                CASTab.IsEnabled = true;
                TabExport.IsEnabled = true;
                TabEditor.IsEnabled = true;
            }));
        }

        void box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (animations.SelectedIndex != -1)
            {
                sprite.AnimationModule.Animations[(string)animations.SelectedItem].Play();
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabEditor.IsSelected && !selected)
            {
                selected = true;
                if (original_texture == null)
                    original_texture = ToBitmapSource(sprite.Texture.Bitmap);
                
                sheet_editor.Source = original_texture;
                for (int row = 0; row < sprite.AnimationModule.Animations.Rows; row++)
                {
                    editor_animations.Items.Add(sprite.AnimationModule.Animations[row].Name);
                }
            }
            else if (!TabEditor.IsSelected)
            {
                selected = false;
                //TODO Unload more shit
                sheet_editor.Source = null;
                editor_animations.Items.Clear();
                Frames.Children.Clear();
            }

            if (TabExport.IsSelected && !export_selected)
            {
                export_selected = true;
                if (original_texture == null)
                    original_texture = ToBitmapSource(sprite.Texture.Bitmap);

                sheet_cutter.Source = original_texture;
                for (int row = 0; row < sprite.AnimationModule.Animations.Rows; row++)
                {
                    cutter_animations.Items.Add(sprite.AnimationModule.Animations[row].Name);
                }

                StartFrame.Value = 0;
                EndFrame.Value = 0;
            }
            else if (!TabExport.IsSelected)
            {
                export_selected = false;
                cutter_animations.Items.Clear();
                sheet_cutter.Source = null;
            }

            if (HitboxTab.IsSelected && !hitbox_selected)
            {
                RenderOptions.SetBitmapScalingMode(hitbox_editor, BitmapScalingMode.NearestNeighbor);
                hitbox_selected = true;

                for (int row = 0; row < sprite.AnimationModule.Animations.Rows; row++)
                {
                    hitbox_animations.Items.Add(sprite.AnimationModule.Animations[row].Name);
                }
                hitbox_animations.SelectedIndex = 0;
                HitboxSlider.Value = 0;
                HitboxSlider_OnValueChanged(null, null);
            }
            else if (!HitboxTab.IsSelected)
            {
                hitbox_selected = false;
                hitbox_animations.Items.Clear();
                hitbox_editor.Source = null;
            }
        }

        private Animation currently_editing_animation;
        private ImageSource original_texture;
        private int currently_editing_animation_row;
        private bool selected = false;
        private bool export_selected = false;
        private bool hitbox_selected = false;
        private int selectedIndex = -1;
        private void editor_animations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!TabEditor.IsSelected || editor_animations.SelectedIndex == -1)
                return;

            currently_editing_animation = sprite.AnimationModule.Animations[(string)editor_animations.SelectedItem];
            currently_editing_animation_row = editor_animations.SelectedIndex;

            var bitmap = new System.Drawing.Bitmap(sprite.Texture.Bitmap); //Clone it, we don't want to write on our texture

            var bluePen = new System.Drawing.Pen(System.Drawing.Color.Aqua, 2);

            var moduleSprite = currently_editing_animation.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();

                int x = 0;
                int y = 0;
                for (int i = 0; i < currently_editing_animation.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_editing_animation.Width * currently_editing_animation.Frames;
                float height = y + currently_editing_animation.Height;

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawLine(bluePen, x, y, width, y);
                    graphics.DrawLine(bluePen, width, y, width, height);
                    graphics.DrawLine(bluePen, x, height, width, height);
                    graphics.DrawLine(bluePen, x, y, x, height);
                }

                ImageSource @new = ToBitmapSource(bitmap);

                bitmap.Dispose();

                sheet_editor.Source = @new;

                //Create frames

                Frames.Children.Clear();
                bitmap = new System.Drawing.Bitmap(sprite.Texture.Bitmap); //Clone it, we don't want to write on our texture
                for (int i = 0; i < currently_editing_animation.Frames; i++)
                {
                    var result = new Bitmap(currently_editing_animation.Width, currently_editing_animation.Height);

                    x = currently_editing_animation.Width * i;
                    y = 0;
                    for (int z = 0; z < currently_editing_animation.Row; z++)
                    {
                        y += ownerModule.Animations[i].Height;
                    }

                    width = currently_editing_animation.Width;
                    height = currently_editing_animation.Height;

                    using (var g = Graphics.FromImage(result))
                    {
                        g.DrawImage(bitmap, new RectangleF(0f, 0f, width, height), new RectangleF(x, y, width, height), GraphicsUnit.Pixel);
                    }

                    var btn = new Button();
                    btn.Name = "b" + i;
                    btn.Width = 32;
                    btn.ContextMenu = FindResource("cmButton") as ContextMenu;
                    if (btn.ContextMenu != null)
                    {
                        btn.ContextMenu.PlacementTarget = btn;

                        btn.Click += delegate
                        {
                            try
                            {
                                selectedIndex = int.Parse(btn.Name.Substring(1));
                                btn.ContextMenu.IsOpen = !btn.ContextMenu.IsOpen;
                            }
                            catch
                            { }
                        };
                    }

                    var img = new System.Windows.Controls.Image {Source = ToBitmapSource(result)};
                    btn.Content = img;

                    result.Dispose();

                    Frames.Children.Add(btn);
                }
            }

            bitmap.Dispose();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private int target;
        private ProgressDialog progress;
        private Bitmap newImage;
        private void cm_before_Click(object sender, RoutedEventArgs e)
        {
            target = selectedIndex - 1;
            AddFrame();
        }

        private void cm_after_Click(object sender, RoutedEventArgs e)
        {
            target = selectedIndex;
            AddFrame();
        }

        private void cm_remove_Click(object sender, RoutedEventArgs e)
        {
            target = selectedIndex;
            RemoveFrame();
        }

        private void RemoveFrame()
        {
            progress = new ProgressDialog()
            {
                WindowTitle = "Removing frame",
                Text = "Please wait while the frame is removed...",
                ShowTimeRemaining = true,
                ShowCancelButton = false
            };
            progress.ProgressBarStyle = ProgressBarStyle.ProgressBar;
            progress.DoWork += progress2_DoWork;
            progress.Show();
        }

        private void AddFrame()
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "Image Files(*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png";

            if ((bool)dialog.ShowDialog(this))
            {
                newImage = new Bitmap(dialog.FileName, false);
                progress = new ProgressDialog()
                {
                    WindowTitle = "Adding frame",
                    Text = "Please wait while the frame is added...",
                    ShowTimeRemaining = true,
                    ShowCancelButton = false
                };
                progress.ProgressBarStyle = ProgressBarStyle.ProgressBar;
                progress.DoWork += progress_DoWork;
                progress.Show();
            }
        }

        void progress2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var moduleSprite = currently_editing_animation.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();

                float x = target * currently_editing_animation.Width;
                float y = 0;
                for (int i = 0; i < currently_editing_animation.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_editing_animation.Width * (currently_editing_animation.Frames - 1);
                float height = y + currently_editing_animation.Height;

                var result = new Bitmap((int)width, (int)height);
                int totalWidth = Math.Max(sprite.Texture.Bitmap.Width, result.Width);
                var final = new Bitmap(totalWidth, sprite.Texture.Bitmap.Height);

                using (var g = Graphics.FromImage(result))
                {
                    //Clear frame
                    System.Drawing.Drawing2D.CompositingMode omode = g.CompositingMode;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255, 255)))
                    {
                        g.FillRectangle(br, x, y, currently_editing_animation.Width, height);
                    }
                    g.CompositingMode = omode;

                    //Move current frames over
                    g.DrawImage(sprite.Texture.Bitmap, new RectangleF(x, 0, width, currently_editing_animation.Height), new RectangleF(x + currently_editing_animation.Width, y, width, height), GraphicsUnit.Pixel);
                    progress.ReportProgress(25, null, "Moving frames over");

                    //Place all frames behind it
                    if (x > 0)
                    {
                        g.DrawImage(sprite.Texture.Bitmap, new RectangleF(0f, 0, x, height), new RectangleF(0f, y, x, height), GraphicsUnit.Pixel);
                    }
                }

                using (var finalG = Graphics.FromImage(final))
                {
                    //Draw entire texture to bitmap
                    finalG.DrawImage(sprite.Texture.Bitmap, new RectangleF(0, 0, sprite.Texture.Bitmap.Width, sprite.Texture.Bitmap.Height), new RectangleF(0, 0, sprite.Texture.Bitmap.Width, sprite.Texture.Bitmap.Height), GraphicsUnit.Pixel);

                    //Fill animation with nothing
                    finalG.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255, 255)))
                    {
                        finalG.FillRectangle(br, x, y, width + currently_editing_animation.Width, height);
                    }

                    //Place new animation
                    finalG.DrawImage(result, new RectangleF(0, y, width, height), new RectangleF(0, 0, result.Width, result.Height), GraphicsUnit.Pixel);
                }

                progress.ReportProgress(75, null, "Saving image..");
                final.Save(this.image);

                progress.ReportProgress(100, null, "Reloading..");
                Dispatcher.Invoke(new Action(delegate
                {
                    RewriteJsonForFrames(-1);
                    Button_Click(null, null);
                    Preview.IsSelected = true;
                    Thread.Sleep(1500);
                    TabEditor.IsSelected = true;
                }));
                result.Dispose();
            }
        }

        void progress_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var moduleSprite = currently_editing_animation.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();
                float x = (target + 1) * currently_editing_animation.Width;
                float y = 0;
                for (int i = 0; i < currently_editing_animation.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_editing_animation.Width * currently_editing_animation.Frames;
                float height = y + currently_editing_animation.Height;

                var result = new Bitmap((int)width + currently_editing_animation.Width, (int)height);
                int totalWidth = Math.Max(sprite.Texture.Bitmap.Width, result.Width);
                var final = new Bitmap(totalWidth, sprite.Texture.Bitmap.Height);

                using (var g = Graphics.FromImage(result))
                {
                    //Move current frames over
                    g.DrawImage(sprite.Texture.Bitmap, new RectangleF(x + currently_editing_animation.Width, 0, width, currently_editing_animation.Height), new RectangleF(x, y, width, height), GraphicsUnit.Pixel);
                    progress.ReportProgress(25, null, "Moving frames over");

                    //Place new image
                    g.DrawImage(newImage, new RectangleF(x, 0, currently_editing_animation.Width, currently_editing_animation.Height), new RectangleF(0, 0, currently_editing_animation.Width, currently_editing_animation.Height), GraphicsUnit.Pixel);
                    progress.ReportProgress(50, null, "Placing new frame");

                    //Place all frames behind it
                    if (x > 0)
                    {
                        g.DrawImage(sprite.Texture.Bitmap, new RectangleF(0f, 0, x, height), new RectangleF(0f, y, x, height), GraphicsUnit.Pixel);
                    }
                }

                using (var finalG = Graphics.FromImage(final))
                {
                    //Draw entire texture to bitmap
                    finalG.DrawImage(sprite.Texture.Bitmap, new RectangleF(0, 0, sprite.Texture.Bitmap.Width, sprite.Texture.Bitmap.Height), new RectangleF(0, 0, sprite.Texture.Bitmap.Width, sprite.Texture.Bitmap.Height), GraphicsUnit.Pixel);

                    //Fill animation with nothing
                    finalG.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255, 255)))
                    {
                        finalG.FillRectangle(br, x, y, width + currently_editing_animation.Width, height);
                    }

                    //Place new animation
                    finalG.DrawImage(result, new RectangleF(0, y, width + currently_editing_animation.Width, height), new RectangleF(0, 0, result.Width, result.Height), GraphicsUnit.Pixel);
                }

                progress.ReportProgress(75, null, "Saving image..");
                final.Save(this.image);

                progress.ReportProgress(100, null, "Reloading..");
                Dispatcher.Invoke(new Action(delegate
                {
                    RewriteJsonForFrames(1);
                    Button_Click(null, null);
                    Preview.IsSelected = true;
                    Thread.Sleep(1500);
                    TabEditor.IsSelected = true;
                }));
                result.Dispose();
            }
        }

        private void RewriteJsonForFrames(int toAdd)
        {
            JObject obj = JObject.Parse(code.Text);

            var animation = obj["animations"][currently_editing_animation.Name].Value<JObject>();
            int frames = animation["framecount"].Value<int>();
            JProperty newProperty = new JProperty("framecount", (frames + toAdd));
            var value = animation.Property("framecount");
            value.Replace(newProperty);

            //frames += toAdd;

            var json = obj.ToString(Formatting.Indented);

            code.Text = json;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "Config Files(*.conf;*.json)|*.conf;*.json";

            if ((bool)dialog.ShowDialog(this))
            {
                json_path.Text = dialog.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "Image Files(*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png";

            if ((bool)dialog.ShowDialog(this))
            {
                sheet_path.Text = dialog.FileName;
            }
        }

        private void cm_replace_Click(object sender, RoutedEventArgs e)
        {
            target = selectedIndex;
            ReplaceFrame();
        }

        private void ReplaceFrame()
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "Image Files(*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png";

            if ((bool)dialog.ShowDialog(this))
            {
                newImage = new Bitmap(dialog.FileName, false);
                progress = new ProgressDialog()
                {
                    WindowTitle = "Removing frame",
                    Text = "Please wait while the frame is removed...",
                    ShowTimeRemaining = true,
                    ShowCancelButton = false
                };
                progress.ProgressBarStyle = ProgressBarStyle.ProgressBar;
                progress.DoWork += progress3_DoWork;
                progress.Show();
            }
        }

        void progress3_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var moduleSprite = currently_editing_animation.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();
                float x = target * currently_editing_animation.Width;
                float y = 0;
                for (int i = 0; i < currently_editing_animation.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_editing_animation.Width;
                float height = y + currently_editing_animation.Height;

                progress.ReportProgress(25, null, "Cloning sprite texture to bitmap...");
                var result = new Bitmap(sprite.Texture.Bitmap); //Clone it

                using (var g = Graphics.FromImage(result))
                {
                    progress.ReportProgress(50, null, "Drawing onto bitmap..");
                
                    System.Drawing.Drawing2D.CompositingMode omode = g.CompositingMode;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    using (var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255, 255)))
                    {
                        g.FillRectangle(br, x, y, width, height);
                    }
                    g.CompositingMode = omode;

                    g.DrawImage(newImage, new RectangleF(x, y, width, height), new RectangleF(0f, 0f, width, height), GraphicsUnit.Pixel);
                }

                progress.ReportProgress(75, null, "Saving image..");
                result.Save(image);

                progress.ReportProgress(100, null, "Reloading..");
                Dispatcher.Invoke(new Action(delegate
                {
                    Button_Click(null, null);
                    Preview.IsSelected = true;
                    Thread.Sleep(1500);
                    TabEditor.IsSelected = true;
                }));
                result.Dispose();
            }
        }

        private string savePath;
        private int start, end;
        private void extBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog {Filter = "PNG File|*.png"};

            var showDialog = dialog.ShowDialog(this);
            if (showDialog != null && (bool)showDialog)
            {
                savePath = dialog.FileName;
                
                if (!savePath.EndsWith(".png"))
                    savePath += ".png";

                if (StartFrame.Value != null)
                {
                    start = (int)StartFrame.Value;
                    if (EndFrame.Value != null) end = (int)EndFrame.Value;
                }
                progress = new ProgressDialog
                {
                    WindowTitle = "Exporting frames",
                    Text = "Please wait while the frames are exported...",
                    ShowTimeRemaining = true,
                    ShowCancelButton = false,
                    ProgressBarStyle = ProgressBarStyle.ProgressBar
                };
                progress.DoWork += progress4_DoWork;
                progress.Show();
            }
        }

        void progress4_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var moduleSprite = currently_exporting.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();
                progress.ReportProgress(25, null, "Cutting..");
                int x = (start * currently_exporting.Width);
                int y = 0;
                for (int i = 0; i < currently_exporting.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_exporting.Width * (end + 1);
                float height = y + currently_exporting.Height;

                var result = new Bitmap((int)(currently_exporting.Width * ((end - start) + 1)), currently_exporting.Height);

                using (var g = Graphics.FromImage(result))
                {
                    progress.ReportProgress(50, null, "Cropping..");
                    g.DrawImage(sprite.Texture.Bitmap, new RectangleF(0, 0, width, currently_exporting.Height), new RectangleF(x, y, width, height), GraphicsUnit.Pixel);
                }

                progress.ReportProgress(75, null, "Saving..");
                result.Save(savePath);

                progress.ReportProgress(100, null, null);
                result.Dispose();
            }

            Thread.Sleep(2000);
        }

        private Animation currently_exporting;
        private int currently_exporting_row;
        private void StartFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (!TabExport.IsSelected || cutter_animations.SelectedIndex == -1)
                return;

            currently_exporting = sprite.AnimationModule.Animations[(string)cutter_animations.SelectedItem];
            currently_exporting_row = cutter_animations.SelectedIndex;

            var numericUpDown = sender as MahApps.Metro.Controls.NumericUpDown;
            if (numericUpDown != null)
                if (numericUpDown.Value != null) numericUpDown.Value = (int)(numericUpDown.Value);

            EndFrame.Minimum = (double)StartFrame.Value;
            StartFrame.Maximum = EndFrame.Maximum = (int)currently_exporting.Frames - 1;
            StartFrame.Minimum = 0;

            var bitmap = new System.Drawing.Bitmap(sprite.Texture.Bitmap); //Clone it, we don't want to write on our texture

            var bluePen = new System.Drawing.Pen(System.Drawing.Color.Aqua, 2);

            var moduleSprite = currently_exporting.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();

                int x = ((int)StartFrame.Value * currently_exporting.Width);
                int y = 0;
                for (int i = 0; i < currently_exporting.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_exporting.Width * ((int)EndFrame.Value + 1);
                float height = y + currently_exporting.Height;

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawLine(bluePen, x, y, width, y);
                    graphics.DrawLine(bluePen, width, y, width, height);
                    graphics.DrawLine(bluePen, x, height, width, height);
                    graphics.DrawLine(bluePen, x, y, x, height);
                }
            }

            ImageSource @new = ToBitmapSource(bitmap);

            bitmap.Dispose();

            sheet_cutter.Source = @new;
        }

        private void cutter_animations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!TabExport.IsSelected || cutter_animations.SelectedIndex == -1)
                return;

            currently_exporting = sprite.AnimationModule.Animations[(string)cutter_animations.SelectedItem];
            currently_exporting_row = cutter_animations.SelectedIndex;

            StartFrame.Value = 0;
            StartFrame.Maximum = EndFrame.Maximum = (int)currently_exporting.Frames - 1;
            EndFrame.Value = EndFrame.Maximum;

            var bitmap = new System.Drawing.Bitmap(sprite.Texture.Bitmap); //Clone it, we don't want to write on our texture

            var bluePen = new System.Drawing.Pen(System.Drawing.Color.Aqua, 2);

            var moduleSprite = currently_exporting.Owner as ModuleSprite;
            if (moduleSprite != null)
            {
                var ownerModule = moduleSprite.GetFirstModule<AnimationModule>();

                int x = ((int)StartFrame.Value * currently_exporting.Width);
                int y = 0;
                for (int i = 0; i < currently_exporting.Row; i++)
                {
                    y += ownerModule.Animations[i].Height;
                }

                float width = currently_exporting.Width * (((int)EndFrame.Value - (int)StartFrame.Value) + 1);
                float height = y + currently_exporting.Height;

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawLine(bluePen, x, y, width, y);
                    graphics.DrawLine(bluePen, width, y, width, height);
                    graphics.DrawLine(bluePen, x, height, width, height);
                    graphics.DrawLine(bluePen, x, y, x, height);
                }
            }

            ImageSource @new = ToBitmapSource(bitmap);

            bitmap.Dispose();

            sheet_cutter.Source = @new;
        }

        private Animation currently_hitboxing;
        private int currently_hitboxing_row;
        private void hitbox_animations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!HitboxTab.IsSelected || hitbox_animations.SelectedIndex == -1)
                return;

            currently_hitboxing = sprite.AnimationModule.Animations[(string) hitbox_animations.SelectedItem];
            currently_hitboxing_row = hitbox_animations.SelectedIndex;

            HitboxSlider.Maximum = currently_hitboxing.Frames - 1;
            HitboxSlider.Value = 0;
            HitboxSlider_OnValueChanged(null, null);
        }

        private void HitboxSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sprite.ChangeHitbox(currently_hitboxing.Name + HitboxSlider.Value);

            UpdateImage();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog {Filter = "Config Files(*.conf;*.json)|*.conf;*.json"};

            if ((bool)dialog.ShowDialog(this))
            {
                hitbox_path.Text = dialog.FileName;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            sprite.Hitbox = new Hitbox(sprite.Hitbox.Name, new List<Vector2>());
            UpdateImage();
        }

        private void UpdateImage()
        {
            var bitmap = new Bitmap(sprite.Texture.Bitmap);

            var result = new Bitmap(currently_hitboxing.Width, currently_hitboxing.Height);

            int x = (int)(HitboxSlider.Value * currently_hitboxing.Width);
            int y = 0;
            for (int i = 0; i < currently_hitboxing.Row; i++)
            {
                y += sprite.AnimationModule.Animations[i].Height;
            }

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.DrawImage(bitmap, new RectangleF(0, 0, currently_hitboxing.Width, currently_hitboxing.Height), new RectangleF(x, y, currently_hitboxing.Width, currently_hitboxing.Height), GraphicsUnit.Pixel);

                Hitbox hitbox = sprite.Hitbox;
                if (hitbox != null)
                {
                    var bluePen = new System.Drawing.Pen(System.Drawing.Color.Aqua, 2);

                    for (int i = 0; i < hitbox.Vertices.Count; i++)
                    {
                        int next = i + 1;
                        if (next >= hitbox.Vertices.Count)
                            next = 0;

                        graphics.DrawLine(bluePen, hitbox.Vertices[i].X, hitbox.Vertices[i].Y, hitbox.Vertices[next].X, hitbox.Vertices[next].Y);
                    }
                }
            }

            bitmap.Dispose();

            ImageSource @new = ToBitmapSource(result);

            result.Dispose();

            hitbox_editor.Source = @new;
        }

        private void Hitbox_editor_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!moved)
            {
                var point = e.GetPosition(hitbox_editor);

                point.X *= (sprite.Width / hitbox_editor.ActualWidth);
                point.Y *= (sprite.Height / hitbox_editor.ActualHeight);

                sprite.Hitbox.Vertices.Add(new Vector2((float)point.X, (float)point.Y));

                UpdateImage(); 
            }

            down = false;
            moved = false;
        }

        private bool down = false;
        private bool moved = false;
        private void Hitbox_editor_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            down = true;
        }

        private void Hitbox_editor_OnMouseMove_(object sender, MouseEventArgs e)
        {
            if (down)
            {
                moved = true;
                var point = e.GetPosition(hitbox_editor);

                point.X *= (sprite.Width / hitbox_editor.ActualWidth);
                point.Y *= (sprite.Height / hitbox_editor.ActualHeight);

                sprite.Hitbox.Vertices.Add(new Vector2((float)point.X, (float)point.Y));

                UpdateImage(); 
            }
        }

        private void Hitbox_editor_OnMouseLeave(object sender, MouseEventArgs e)
        {
            down = false;
            moved = false;
        }
    }

    internal sealed class HitboxContainer
    {
        [JsonProperty(PropertyName = "hitboxes")]
        public List<HitboxHolder> Hitboxes { get; set; }
    }

    internal sealed class HitboxHolder
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "vertices")]
        public List<float> Vertices { get; set; }
    }
}
