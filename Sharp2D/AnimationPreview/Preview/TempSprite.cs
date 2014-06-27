using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Sprites.Animations;
using Sharp2D.Core.Graphics;

namespace AnimationPreview.Preview
{
    public class TempSprite : AnimatedSprite
    {
        private string tex_path;
        private string sheet_path;

        public string TexPath
        {
            set
            {
                tex_path = value;
                Texture = Texture.NewTexture(tex_path);
                Texture.LoadTextureFromFile();
                RequestOnDisplay();
            }
            get
            {
                return tex_path;
            }
        }

        public string JsonPath
        {
            get
            {
                return JsonFilePath;
            }
            set
            {
                sheet_path = value;
            }
        }

        public override string JsonFilePath
        {
            get
            {
                return sheet_path;
            }
        }

        public TempSprite()
        {
        }

        public TempSprite(string tex_path)
        {
            TexPath = tex_path;
        }

        public override string Name
        {
            get { return "test"; }
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDisplay()
        {
            Texture.CreateOrUpdate();

            MainWindow.WINDOW.CompleteReload();
        }
    }
}
