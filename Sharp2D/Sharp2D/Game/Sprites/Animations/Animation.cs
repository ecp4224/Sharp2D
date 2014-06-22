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
        [JsonProperty(PropertyName = "animations")]
        private Dictionary<string, Animation> animations;

        [JsonIgnore]
        private AnimationHolder holder;

        public AnimationHolder Animations
        {
            get
            {
                if (animations == null)
                {
                    animations = new Dictionary<string, Animation>();
                }

                if (holder == null)
                {
                    holder = new AnimationHolder();
                    holder.Animations = animations;
                }

                if (inherit_from != null && !string.IsNullOrWhiteSpace(inherit_from))
                {
                    return Owner.Animations[inherit_from].Animations;
                }
                return holder;
            }
        }

        [JsonProperty(PropertyName="origin_type")]
        private string origin_type;

        [JsonProperty(PropertyName="x_offset")]
        private float xoffset;

        [JsonProperty(PropertyName="y_offset")]
        private float yoffset;

        [JsonProperty(PropertyName="sprite")]
        private string sprite;

        [JsonIgnore]        
        public string SpriteFullName { get { return sprite; } }

        [JsonIgnore]
        public float XOffset { get { return xoffset; } }

        [JsonIgnore]
        public float YOffset { get { return yoffset; } }

        [JsonIgnore]
        private Origin type;

        [JsonIgnore]
        private bool type_set = false;

        [JsonIgnore]
        public Origin OriginType
        {
            get
            {
                if (!type_set)
                {
                    if (origin_type != null)
                    {
                        Type ot = typeof(Origin);

                        if (origin_type.Contains("|"))
                        {
                            Origin f, s;
                            string[] array = origin_type.Split('|');

                            Enum.TryParse<Origin>(array[0], true, out f);
                            Enum.TryParse<Origin>(array[1], true, out s);

                            type = f | s;
                        }
                        else
                        {
                            Enum.TryParse<Origin>(origin_type, true, out type);
                        }
                    }
                    else
                    {
                        type = Origin.Center;
                    }

                    type_set = true;
                }

                return type;
            }
        }

        [JsonProperty(PropertyName="inherit_animations")]
        private string inherit_from;

        [JsonProperty(PropertyName="row")]
        public int Row { get; private set; }

        [JsonProperty(PropertyName="framecount")]
        public int Frames { get; private set; }

        [JsonProperty(PropertyName="speed")]
        public long Speed { get; set; }

        [JsonProperty(PropertyName = "width")]
        private int width = -1;

        [JsonProperty(PropertyName = "height")]
        private int height = -1;

        [JsonProperty(PropertyName="empty")]
        public bool IsEmpty { get; private set; }

        [JsonIgnore]
        public int Width
        {
            get
            {
                if (width == -1)
                    return Owner.Animations.width;
                return width;
            }
        }

        [JsonIgnore]
        public int Height
        {
            get
            {
                if (height == -1)
                    return Owner.Animations.height;
                return height;
            }
        }

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
                if (value && Owner.CurrentlyPlayingAnimation != this)
                {
                    Owner.CurrentlyPlayingAnimation = this;
                    Owner.Width = Width;
                    Owner.Height = Height;
                    Owner.Visible = true;
                }
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
                float x = Width * CurrentStep;
                float y = 0f;
                for (int i = 0; i < Row; i++)
                {
                    y += Owner.Animations[i].Height;
                }
                y = Owner.Texture.TextureHeight - y;

                float width = x + Width;
                float height = y + Height;

                return new TexCoords(x, y, width, height, Owner.Texture);
            }
        }

        public Animation Play()
        {
            Playing = true;
            return this;
        }

        public Animation Pause()
        {
            Playing = false;
            return this;
        }
    }
}
