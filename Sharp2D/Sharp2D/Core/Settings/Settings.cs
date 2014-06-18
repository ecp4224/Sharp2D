using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Settings
{
    public abstract class Setting
    {

        protected abstract void OnLoad();
        protected abstract void OnSave();

        public void Save(string Path)
        {
            Setting.Save(this, Path);
        }

        public static T Load<T>(string Path) where T : Setting
        {
            T settings = JsonConvert.DeserializeObject<T>(Path);
            settings.OnLoad();
            return settings;
        }

        public static T LoadOrSaveDefault<T>(string Path) where T : Setting
        {
            if (!System.IO.File.Exists(Path))
            {
                T @default = (T)Activator.CreateInstance(typeof(T));
                @default.Save(Path);
                return @default;
            }
            return Load<T>(Path);
        }

        public static void Save(Setting Settings, string Path)
        {
            Settings.OnSave();
            System.IO.File.WriteAllText(Path, JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }
    }
}
