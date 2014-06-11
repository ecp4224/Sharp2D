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

        public abstract void OnLoad();
        public abstract void OnSave();
        
        public static T Load<T>(string Path) where T : Setting{
            if (!System.IO.File.Exists(Path))
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            
            T settings = JsonConvert.DeserializeObject<T>(Path);
            settings.OnLoad();
            return settings;
        }

        public static void Save(Setting Settings, string Path){
            Settings.OnSave();
            System.IO.File.WriteAllText(Path, JsonConvert.SerializeObject(Settings, Formatting.Indented));   
        }
    }
}
