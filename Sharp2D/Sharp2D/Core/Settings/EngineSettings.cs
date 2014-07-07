using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Settings
{
    public class EngineSettings : SaveableSettings
    {
        /// <summary>
        /// <para>Have Sharp2D use it's own embedded files for shaders and other files, or not.</para>
        /// <para>Sharp2D loads shader files from the "shader" folder when not using embedded resources</para>
        /// </summary>
        public bool PreferEmbeddedResources { get; set; }

        public EngineSettings(EngineSettings settings)
        {
            if (settings == null)
                return;

            PreferEmbeddedResources = settings.PreferEmbeddedResources;
        }

        public EngineSettings()
        {
            PreferEmbeddedResources = true;
        }

        protected override void OnLoad()
        {
        }

        protected override void OnSave()
        {
        }
    }
}
