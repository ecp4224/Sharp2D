using Sharp2D;

namespace AnimationPreview.Preview
{
    public class TempSprite : PhysicsSprite
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
                return GetFirstModule<AnimationModule>().AnimationConfigPath;
            }
            set
            {
                sheet_path = value;
            }
        }

        public AnimationModule AnimationModule { get; private set; }

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

        protected override void OnLoad()
        {
            AnimationModule = AttachModule<AnimationModule>();
            AnimationModule.AnimationConfigPath = sheet_path;

            base.OnLoad();
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
