using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Sharp2D.Core;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Sprites;

namespace Sharp2D
{
    public class AnimationModule : IModule
    {
        public static readonly AnimationModule Empty = new AnimationModule("empty"); //Blank module

        private Sprite _sprite; 
        
        public Animation CurrentlyPlayingAnimation { get; internal set; }
        public Animation ChildAnimationPlaying { get; internal set; }

        private long _lastTick;

        public AnimationHolder Animations { get; private set; }

        public AnimationModule Parent { get; private set; }

        public string AnimationConfigPath { get; set; }

        public string JsonResourcePath { get; private set; }

        private List<AnimationModule> _children = new List<AnimationModule>();

        private readonly bool _defaultConstruct;

        public AnimationModule()
        {
            _defaultConstruct = true;
        }

        public AnimationModule(string Name) : this("animations/" + Name + ".conf", Name + ".conf") { }

        public AnimationModule(string AnimationConfigPath, string JsonResourcePath)
        {
            this.AnimationConfigPath = AnimationConfigPath;
            this.JsonResourcePath = JsonResourcePath;
        }

        public void InitializeWith(Sprite sprite)
        {
            if (Owner != null)
                throw new InvalidOperationException("This module has already been initialized!");

            if (_defaultConstruct)
            {
                AnimationConfigPath = "animations/" + sprite.Name + ".conf";
                JsonResourcePath = sprite.Name + ".conf";
            }

            _sprite = sprite;

            sprite.Loaded += sprite_Loaded;
            if (sprite.IsLoaded)
                sprite_Loaded(sprite, new SpriteEvent(sprite));
        }

        public void OnUpdate()
        {
            if (CurrentlyPlayingAnimation == null) return;
            if (_lastTick == 0)
                _lastTick = Screen.TickCount;

            if (_lastTick + CurrentlyPlayingAnimation.Speed <= Screen.TickCount)
            {
                _lastTick = Screen.TickCount;
                if (CurrentlyPlayingAnimation.Playing)
                {
                    if (!CurrentlyPlayingAnimation.Reverse)
                        CurrentlyPlayingAnimation.CurrentStep++;
                    else
                        CurrentlyPlayingAnimation.CurrentStep--;
                }
            }
            Owner.TexCoords = CurrentlyPlayingAnimation.CurrentTexCoords;
        }

        void sprite_Loaded(object sender, EventArgs e)
        {
            if (Parent == null && Animations == null) //Only load json if we have no parent and if we haven't already loaded the json
                LoadJson();

            this.NotifyAll();
        }

        private void LoadJson()
        {
            string json;
            if (File.Exists(AnimationConfigPath))
            {
                json = File.ReadAllText(AnimationConfigPath);
            }
            else
            {
                Assembly asm = Assembly.GetEntryAssembly();

                Stream stream = asm.GetManifestResourceStream(JsonResourcePath);
                if (stream == null)
                    return;
                var reader = new StreamReader(stream);
                json = reader.ReadToEnd();

                reader.Close();
                stream.Close();

                reader.Dispose();
                stream.Dispose();
            }

            if (json == null) return;
            Animations = JsonConvert.DeserializeObject<AnimationHolder>(json);
            Animations[0].ModuleOwner = this;
            _sprite.Width = Animations[0].Width;
            _sprite.Height = Animations[0].Height;

            _children = SetupChildren(Animations);

            Animations[0].Playing = true;
        }

        public void ClearAnimations()
        {
            Animations = null;
            foreach (AnimationModule ani in _children)
            {
                ani.ClearAnimations();
            }
            _children.Clear();
        }

        private List<AnimationModule> SetupChildren(AnimationHolder animations)
        {
            var temp = new List<AnimationModule>();
            foreach (string key in animations.Animations.Keys) //To follow this, let's assume this is the "walking left" animation
            {
                Animation ani = animations.Animations[key];

                temp.AddRange(Setup(ref ani));
            }

            return temp; //Return the new sprites created
        }

        private List<AnimationModule> Setup(ref Animation ani, List<Animation> inheritStack = null)
        {
            var temp = new List<AnimationModule>();

            if (ani.setup_ran)
                return temp;

            if (inheritStack == null)
                inheritStack = new List<Animation>();

            Assembly assembly = Assembly.GetEntryAssembly();

            ani.ModuleOwner = this;

            if (ani.Animations == null)
                return temp;

            if (ani.InheritedAnimations != null)
            {
                if (inheritStack.Count > 0 && inheritStack.Contains(ani.InheritedAnimationOwner))
                    throw new InvalidOperationException("Loop inheritance detected!");
                if (!ani.InheritedAnimationOwner.setup_ran) //If the inherited animation hasn't been setup yet
                {
                    Animation inheritOwner = ani.InheritedAnimationOwner;
                    inheritStack.Add(ani); //Add ourself to the stack
                    Setup(ref inheritOwner, inheritStack); //Setup the inherited animation
                    inheritStack.Remove(ani); //Remove ourself from the stack
                }
                AnimationHolder t = ani.Animations;
                AnimationHolder.Combind(ani.InheritedAnimations, ref t);
            }

            foreach (string ckey in ani.Animations.Animations.Keys) //To follow this, let's assume this is the "hat" animation for "walking left" animation
            {
                //ckey = "hat"
                Animation child_animation = ani.Animations[ckey]; //Same as ani.Animations.Animations[ckey];
                //Get the animation object for "hat"

                child_animation.ParentAnimation = ani; //Set the parent of this child animation to this

                if (ani.InheritedAnimations == null) //Only inherit values if this animation didn't inherit animations from another animation
                {
                    Animation.Combind(ani, ref child_animation, false); //Inherit values from parent animation to this animation (exclude animations)
                }

                if (!child_animation.IsEmpty)
                {
                    ModuleSprite sprite;
                    AnimationModule spriteModule;
                    Type st = assembly.GetType(child_animation.SpriteFullName); //Get the type for the FullSpriteName
                    if (st == null)
                    {
                        //Assume it's a file path if no type was found
                        try
                        {
                            sprite = new BasicAnimatedSprite(child_animation.SpriteFullName);
                            spriteModule = new AnimationModule("basic_sprite");
                            sprite.AttachModule(spriteModule);
                        }
                        catch
                        {
                            //If there was an error making this sprite
                            //Just ignore it..
                            ani.setup_ran = true;

                            return temp;
                        }
                    }
                    else
                    {
                        sprite = (ModuleSprite)Activator.CreateInstance(st); //Create a new instance of the child sprite

                        if (sprite.ModuleExists<AnimationModule>())
                            spriteModule = sprite.GetModules<AnimationModule>()[0]; //There can only ever bee one animation module
                        else
                        {
                            spriteModule = new AnimationModule(sprite.ToString());
                            sprite.AttachModule(spriteModule);
                        }
                    }

                    child_animation.ModuleOwner = spriteModule; //Set the owner of the "hat" animation to the newly created sprite
                    spriteModule.Animations = child_animation.Animations; //Set the animations for the hat sprite to the children animations of the "hat" animation.
                    Owner.Attach(sprite);
                    sprite.IsVisible = false; //Make sure this sprite isn't visible by default
                    temp.Add(spriteModule); //Add the "hat" sprite as a children of this sprite

                    spriteModule.SetupChildren(spriteModule.Animations); //Setup the children of "hat" animation, and put all of it's children in a list

                    //Nevermind, don't do that
                    //temp.AddRange(temp2); //Add all of children of the "hat" sprite as our children as well.
                }
                else
                {
                    temp.Add(Empty); //This is an empty animation, fill it with a unused module
                }
            }

            ani.setup_ran = true;

            return temp;
        }

        public void AlignChildAnimation()
        {
            if (ChildAnimationPlaying == null)
                return;
            Sprite sprite = ChildAnimationPlaying.Owner;
            AnimationModule spriteModule = ChildAnimationPlaying.ModuleOwner;

            float X = Owner.X, Y = Owner.Y, Width = Owner.Width, Height = Owner.Height;

            //Default is center parent origin, center child origin
            float defaultx = X;
            float defaulty = Y;

            Placement porigin = ChildAnimationPlaying.ParentOriginType;
            if ((porigin & Placement.Left) != 0)
            {
                defaultx -= (Width / 2f);
            }
            if ((porigin & Placement.Right) != 0)
            {
                defaultx += Width / 2f;
            }
            if ((porigin & Placement.Top) != 0)
            {
                defaulty -= Height / 2f;
            }
            if ((porigin & Placement.Bottom) != 0)
            {
                defaulty += (Height / 2f);
            }

            float xadd = 0f, yadd = 0f;
            Placement origin = ChildAnimationPlaying.OriginType;
            if ((origin & Placement.Left) != 0)
            {
                xadd = -(ChildAnimationPlaying.Width / 2f);
            }
            if ((origin & Placement.Right) != 0)
            {
                xadd = ChildAnimationPlaying.Width / 2f;
            }
            if ((origin & Placement.Top) != 0)
            {
                yadd = ChildAnimationPlaying.Height / 2f;
            }
            if ((origin & Placement.Bottom) != 0)
            {
                yadd = -(ChildAnimationPlaying.Height / 2f);
            }

            //Move the "hat" sprite to the default origin of the sprite
            sprite.X = defaultx - xadd;
            sprite.Y = defaulty + yadd;

            //Offset the "hat" sprite
            sprite.X += ChildAnimationPlaying.XOffset;
            sprite.Y += ChildAnimationPlaying.YOffset;

            spriteModule.AlignChildAnimation();
        }

        public Sprite Owner
        {
            get { return _sprite; }
        }

        public string ModuleName
        {
            get { return "Animations"; }
        }


        public ModuleRules Rules
        {
            get { return ModuleRules.OnePerSprite; }
        }

        public void Dispose()
        {
            if (Animations != null)
                Animations.Dispose();
            _children.Clear();
        }
    }

    public class BasicAnimatedSprite : ModuleSprite
    {
        public BasicAnimatedSprite(string path)
        {
            Texture = Texture.NewTexture(path);
            Texture.LoadTextureFromFile();
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;
        }

        public override string Name
        {
            get { return "basic_sprite"; }
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDisplay()
        {
        }

        protected override void OnLoad()
        {
        }
    }

    public class AnimationHolder
    {
        [JsonProperty(PropertyName = "width")]
        internal int width;

        [JsonProperty(PropertyName = "height")]
        internal int height;

        [JsonProperty(PropertyName = "animations")]
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        public int Rows
        {
            get
            {
                return _animations.Keys.Count;
            }
        }

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

        public static void Combind(AnimationHolder inherited, ref AnimationHolder @override)
        {
            var temp = new Dictionary<string, Animation>();
            foreach (string key in inherited._animations.Keys)
            {
                temp.Add(key, (Animation)inherited._animations[key].Clone());
            }

            foreach (string key in @override._animations.Keys)
            {
                if (temp.ContainsKey(key))
                {
                    Animation inherited_animation = temp[key];
                    Animation override_animation = @override._animations[key];

                    Animation.Combind(inherited_animation, ref override_animation);

                    temp.Remove(key);
                    temp.Add(key, override_animation);
                }
                else
                {
                    temp.Add(key, @override._animations[key]);
                }
            }

            @override._animations = temp;
        }

        internal Dictionary<string, Animation> Animations
        {
            get
            {
                return _animations;
            }
            set
            {
                _animations = value;
            }
        }

        internal void Dispose()
        {
            if (_animations != null)
                _animations.Clear();
            _animations = null;
        }
    }
}
