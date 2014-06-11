using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Sprites.Animations
{
    public abstract class AnimatedSprite : Sprite, ILogical
    {
        public Animation CurrentlyPlayingAnimation { get; internal set; }
        private long LastTick;

        public AnimationHolder Animations { get; private set; }

        public abstract string Name { get; }

        public virtual void Update()
        {
            if (CurrentlyPlayingAnimation != null)
            {
                if (LastTick + CurrentlyPlayingAnimation.Speed >= Screen.TickCount)
                {
                    LastTick = Screen.TickCount;
                    if (!CurrentlyPlayingAnimation.Reverse)
                        CurrentlyPlayingAnimation.CurrentStep++;
                    else
                        CurrentlyPlayingAnimation.CurrentStep--;
                }
                TexCoords = CurrentlyPlayingAnimation.CurrentTexCoords;
            }
        }

        protected override void OnLoad()
        {
            string json = null;
            if (File.Exists("animations/" + Name + ".conf"))
            {
                json = File.ReadAllText("animations/" + Name + ".conf");
            }
            else
            {
                Assembly asm = Assembly.GetEntryAssembly();

                Stream stream = asm.GetManifestResourceStream(Name + ".conf");
                if (stream == null)
                    return;
                StreamReader reader = new StreamReader(stream);
                json = reader.ReadToEnd();

                reader.Close();
                stream.Close();

                reader.Dispose();
                stream.Dispose();
            }

            if (json != null)
            {
                Animations = JsonConvert.DeserializeObject<AnimationHolder>(json);
                Width = Animations.width;
                Height = Animations.height;

                Animations[0].Owner = this;

                Animations[0].Playing = true;
            }
        }

        protected override void OnDispose()
        {
            if (Animations != null)
                Animations.Dispose();
        }
    }

    public class AnimationHolder
    {
        [JsonProperty(PropertyName="width")]
        internal float width;

        [JsonProperty(PropertyName="height")]
        internal float height;

        [JsonProperty(PropertyName = "animations")]
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();


        public Animation this[int index]
        {
            get
            {
                foreach (string name in _animations.Keys)
                {
                    if (_animations[name].Row == index)
                        return _animations[name];
                }
                return null;
            }
        }

        public Animation this[string name]
        {
            get
            {
                return _animations[name];
            }
        }

        internal void Dispose()
        {
            if (_animations != null) //todo temp workaround
                _animations.Clear();
            _animations = null;
        }
    }
}
