using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sharp2D.Core.Utils;

namespace Sharp2D.Game.Sprites.Animations
{
    public class Animation
    {
        [JsonProperty(PropertyName="row")]
        public int Row { get; private set; }

        [JsonProperty(PropertyName="framecount")]
        public int Frames { get; private set; }

        [JsonProperty(PropertyName="speed")]
        public long Speed { get; set; }

        [JsonIgnore]
        public AnimatedSprite Owner { get; internal set; }

        [JsonIgnore]
        private int _step;
        
        [JsonIgnore]
        public int CurrentStep
        {
            get
            {
                return _step;
            }
            set
            {
                _step = value;
                if (_step >= Frames)
                    _step = 0;
                if (_step < 0)
                    _step = Frames - 1;
            }
        }

        [JsonIgnore]
        private bool _play;

        [JsonIgnore]
        public bool Playing
        {
            get
            {
                return _play && Owner.CurrentlyPlayingAnimation == this;
            }
            set
            {
                if (Owner.CurrentlyPlayingAnimation != this)
                    Owner.CurrentlyPlayingAnimation = this;
                _play = value;
            }
        }

        [JsonIgnore]
        public bool Reverse { get; set; }

        [JsonIgnore]
        public TexCoords CurrentTexCoords
        {
            get
            {
                float y = Owner.Height * Row;
                float x = Owner.Width * CurrentStep;
                float width = x + Owner.Width;
                float height = y + Owner.Height;

                return new TexCoords(x, y, width, height, Owner.Texture);
            }
        }
    }
}
