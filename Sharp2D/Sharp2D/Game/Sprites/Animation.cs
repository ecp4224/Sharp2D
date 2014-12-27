using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sharp2D;
using Sharp2D.Core;

namespace Sharp2D.Game.Sprites
{
    public class Animation : ICloneable
    {
        [JsonProperty(PropertyName = "animations")]
        private Dictionary<string, Animation> animations;

        [JsonIgnore]
        private AnimationHolder holder;

        [JsonIgnore]
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

                /*if (inherit_from != null && !string.IsNullOrWhiteSpace(inherit_from))
                {
                    return Owner.Animations[inherit_from].Animations;
                }*/
                return holder;
            }
        }

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Owner != null)
                {
                    foreach (string name in ModuleOwner.Animations.Animations.Keys)
                    {
                        if (ModuleOwner.Animations[name] == this)
                            return name;
                    }
                }
                return "";
            }
        }

        [JsonIgnore]
        public Animation this[string name]
        {
            get
            {
                return Animations[name];
            }
        }

        [JsonIgnore]
        public Animation this[int row]
        {
            get
            {
                return Animations[row];
            }
        }

        [JsonProperty(PropertyName="origin_type")]
        private string origin_type;

        [JsonProperty(PropertyName="parent_origin_type")]
        private string parent_origin_type;

        [JsonProperty(PropertyName="x_offset")]
        private float xoffset = -1;

        [JsonProperty(PropertyName="y_offset")]
        private float yoffset = -1;

        [JsonProperty(PropertyName="sprite")]
        private string sprite;

        [JsonIgnore]        
        public string SpriteFullName { get { return sprite; } }

        [JsonIgnore]
        public float XOffset { get { return xoffset; } }

        [JsonIgnore]
        public float YOffset { get { return yoffset; } }

        [JsonIgnore]
        public Animation ParentAnimation { get; internal set; }

        [JsonIgnore]
        private Placement type;

        [JsonIgnore]
        private Placement parent_type;

        [JsonIgnore]
        private bool type_set = false;
        
        [JsonIgnore]
        private bool parent_type_set = false;

        [JsonIgnore]
        public Placement OriginType
        {
            get
            {
                if (!type_set)
                {
                    if (origin_type != null)
                    {
                        type = ParseStringToOrigin(origin_type);
                    }
                    else
                    {
                        type = Placement.Center;
                    }

                    type_set = true;
                }

                return type;
            }
        }

        [JsonIgnore]
        internal bool setup_ran = false;

        [JsonIgnore]
        public Placement ParentOriginType
        {
            get
            {
                if (!parent_type_set)
                {
                    if (parent_origin_type != null)
                    {
                        parent_type = ParseStringToOrigin(parent_origin_type);
                    }
                    else
                    {
                        parent_type = Placement.Center;
                    }

                    parent_type_set = true;
                }

                return parent_type;
            }
        }

        [JsonProperty(PropertyName="inherit_animations")]
        private string inherit_from;

        [JsonIgnore]
        public AnimationHolder InheritedAnimations
        {
            get
            {
                if (inherit_from != null && !string.IsNullOrWhiteSpace(inherit_from))
                {
                    return ModuleOwner.Animations[inherit_from].Animations;
                }
                return null;
            }
        }

        [JsonIgnore]
        public Animation InheritedAnimationOwner
        {
            get
            {
                if (inherit_from != null && !string.IsNullOrWhiteSpace(inherit_from))
                {
                    return ModuleOwner.Animations[inherit_from];
                }
                return null;
            }
        }

        [JsonProperty(PropertyName = "row")]
        private int row = -1;

        public int Row { get { return row; } }

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
                if (Owner == null) { Console.WriteLine("Owner is null"); }
                if (ModuleOwner.Animations == null) { Console.WriteLine("Animations are null"); }
                if (width == -1)
                    return ModuleOwner.Animations.width;
                return width;
            }
        }

        [JsonIgnore]
        public int Height
        {
            get
            {
                if (height == -1)
                    return ModuleOwner.Animations.height;
                return height;
            }
        }

        [JsonIgnore]
        public AnimationModule ModuleOwner { get; internal set; }

        [JsonIgnore]
        public Sprite Owner
        {
            get
            {
                return ModuleOwner.Owner;
            }
        }

        [JsonIgnore]
        private int _step = -1;
        
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
                return _play && ModuleOwner.CurrentlyPlayingAnimation == this;
            }
            set
            {
                if (Owner == null) //This is a empty animation
                {
                    if (ParentAnimation.ModuleOwner.ChildAnimationPlaying != null)
                    {
                        if (ParentAnimation.Owner.CurrentWorld is Worlds.BatchJobWorld)
                        {
                            Worlds.BatchJobWorld w = (Worlds.BatchJobWorld)ParentAnimation.Owner.CurrentWorld;
                            w.RemoveSprite(ParentAnimation.ModuleOwner.ChildAnimationPlaying.Owner);
                        }
                    }
                    ParentAnimation.ModuleOwner.ChildAnimationPlaying = null;
                }
                else if (value && ModuleOwner.CurrentlyPlayingAnimation != this)
                {
                    ModuleOwner.CurrentlyPlayingAnimation = this;
                    Owner.Width = Width;
                    Owner.Height = Height;
                    Owner.IsVisible = true;
                    if (ParentAnimation != null)
                    {
                        if (ParentAnimation.ModuleOwner.ChildAnimationPlaying != null)
                        {
                            if (ParentAnimation.Owner.CurrentWorld is Worlds.BatchJobWorld)
                            {
                                Worlds.BatchJobWorld w = (Worlds.BatchJobWorld)ParentAnimation.Owner.CurrentWorld;
                                w.RemoveSprite(ParentAnimation.ModuleOwner.ChildAnimationPlaying.Owner);
                            }
                        }
                        ParentAnimation.ModuleOwner.ChildAnimationPlaying = this;
                        if (ParentAnimation.Owner.CurrentWorld is Worlds.BatchJobWorld)
                        {
                            Worlds.BatchJobWorld w = (Worlds.BatchJobWorld)ParentAnimation.Owner.CurrentWorld;
                            if (!w.Sprites.Contains(Owner))
                                w.AddSprite(Owner);
                        }
                        ParentAnimation.ModuleOwner.AlignChildAnimation();
                    }
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
                if (CurrentStep < 0)
                    CurrentStep = 0;
                float x = Width * CurrentStep;
                float y = 0f;
                for (int i = 0; i < Row; i++)
                {
                    y += ModuleOwner.Animations[i].Height;
                }
                y = Owner.Texture.TextureHeight - y;

                float width = x + Width;
                float height = y + Height;

                return new TexCoords(x, y, width, height, Owner.Texture);
            }
        }

        public Animation Play()
        {
            if (ParentAnimation != null)
                ParentAnimation.Play();
            Playing = true;
            return this;
        }

        public Animation Pause()
        {
            Playing = false;
            return this;
        }

        private static Placement ParseStringToOrigin(string origin_type)
        {
            Placement type;
            Type ot = typeof(Placement);

            if (origin_type.Contains("|"))
            {
                Placement f, s;
                string[] array = origin_type.Split('|');

                Enum.TryParse<Placement>(array[0], true, out f);
                Enum.TryParse<Placement>(array[1], true, out s);

                type = f | s;
            }
            else
            {
                Enum.TryParse<Placement>(origin_type, true, out type);
            }

            return type;
        }

        public Animation Clone()
        {
            Animation toReturn = new Animation();

            toReturn.animations = new Dictionary<string, Animation>();
            if (animations != null)
            {
                foreach (string key in animations.Keys)
                {
                    toReturn.animations.Add(key, (Animation)animations[key].Clone());
                }
            }

            toReturn.origin_type = origin_type;
            toReturn.parent_origin_type = parent_origin_type;
            toReturn.sprite = sprite;
            toReturn.width = width;
            toReturn.height = height;
            toReturn.inherit_from = inherit_from;
            toReturn.row = row;
            toReturn.Speed = Speed;
            toReturn.Reverse = Reverse;
            toReturn.xoffset = xoffset;
            toReturn.yoffset = yoffset;
            toReturn._step = _step;
            toReturn._play = _play;
            toReturn.IsEmpty = IsEmpty;
            toReturn.Frames = Frames;

            return toReturn;
        }

        public static void Combind(Animation inherit, ref Animation @override, bool includeAnimations = true)
        {
            //kill me alem ;_;

            if (@override.IsEmpty) //The overrider is an empty animation, don't inherit anything 
                return;

            if (includeAnimations)
            {
                if (inherit.animations != null)
                {
                    if (@override.animations == null)
                    {
                        //If the overrider doesn't have any animations, then give all of inherit's animations
                        //to the overrider
                        @override.animations = new Dictionary<string, Animation>();
                        foreach (string key in inherit.animations.Keys)
                        {
                            @override.animations.Add(key, inherit.animations[key].Clone()); //Be sure to clone the animation
                        }
                    }
                    else
                    {
                        //For every animation the override doesn't have, clone it and give it to the overrider
                        //For every animation the override does have, call Combind again with the 2 conflicting animations, then give the result to the overrider
                        foreach (string key in inherit.animations.Keys)
                        {
                            if (@override.animations.ContainsKey(key))
                            {
                                Animation inherit_animation = inherit.animations[key];
                                Animation override_animation = @override.animations[key];

                                Combind(inherit_animation, ref @override_animation);

                                @override.animations.Remove(key); //Replace the old animation
                                @override.animations.Add(key, override_animation); //With the new animation
                            }
                            else
                            {
                                @override.animations.Add(key, inherit.animations[key].Clone());
                            }
                        }
                    }
                }
            }

            /*
            Check against default for strings, if is default, then use inherited value
            Check for -1 against any variable that can be 0 (ex: xoffset, yoffset, width, height, row), if is -1, then use inherited value
            Check for default for Speed and Frames, these values should never be 0, if is default, then use inherited value
             */

            if (@override.origin_type.IsDefaultForType<string>())
            {
                @override.origin_type = inherit.origin_type;
            }
            if (@override.parent_origin_type.IsDefaultForType<string>())
            {
                @override.parent_origin_type = inherit.parent_origin_type;
            }
            if (@override.sprite.IsDefaultForType<string>())
            {
                @override.sprite = inherit.sprite;
            }
            if (@override.width == -1)
            {
                @override.width = inherit.width;
            }
            if (@override.height == -1)
            {
                @override.height = inherit.height;
            }
            if (@override.inherit_from.IsDefaultForType<string>())
            {
                @override.inherit_from = inherit.inherit_from;
            }
            if (@override.row == -1)
            {
                @override.row = inherit.row;
            }
            if (@override.Speed.IsDefaultForType<long>())
            {
                @override.Speed = inherit.Speed;
            }
            if (@override.xoffset == -1)
            {
                @override.xoffset = inherit.xoffset;
            }
            if (@override.yoffset == -1)
            {
                @override.yoffset = inherit.yoffset;
            }
            if (@override._step == -1)
            {
                @override._step = inherit._step;
            }
            if (@override.Frames.IsDefaultForType<int>())
            {
                @override.Frames = inherit.Frames;
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
