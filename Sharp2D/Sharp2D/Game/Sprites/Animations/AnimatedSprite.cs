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
using Sharp2D.Core.Utils;
using Sharp2D.Game.Worlds;

namespace Sharp2D.Game.Sprites.Animations
{
    public abstract class AnimatedSprite : Sprite, ILogical
    {
        public Animation CurrentlyPlayingAnimation { get; internal set; }
        private long LastTick;

        public AnimationHolder Animations { get; private set; }

        public AnimatedSprite Parent { get; private set; }

        public abstract string Name { get; }

        public override float X
        {
            get
            {
                return base.X;
            }
            set
            {
                float dif = value - base.X;
                
                base.X = value;

                foreach (AnimatedSprite child in children)
                {
                    if (child is NullAnimatedSprite) //Empty animation?
                        continue;

                    child.X += dif;
                }
            }
        }

        public override float Y
        {
            get
            {
                return base.Y;
            }
            set
            {
                float dif = value - base.Y;

                base.Y = value;

                foreach (AnimatedSprite child in children)
                {
                    if (child is NullAnimatedSprite) //Empty animation?
                        continue;

                    child.Y += dif;
                }
            }
        }

        private List<AnimatedSprite> children = new List<AnimatedSprite>();

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
            if (Parent == null) //Only load json if we have no parent
                LoadJSON();
        }

        private void LoadJSON()
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

                children = SetupChildren(Animations);

                Animations[0].Playing = true;
                if (Animations[0].Animations.Animations.Count > 0)
                {
                    Animations[0].Animations[0].Playing = true;
                }
            }
        }

        public override void OnAddedToWorld(World w)
        {
            base.OnAddedToWorld(w);

            if (w is SpriteWorld)
            {
                SpriteWorld sw = (SpriteWorld)w;
                foreach (AnimatedSprite ani in children)
                {
                    if (ani is NullAnimatedSprite)
                        continue;

                    sw.AddSprite(ani);
                }
            }
        }

        public override void OnRemovedFromWorld(World w)
        {
            base.OnRemovedFromWorld(w);

            if (w is SpriteWorld)
            {
                SpriteWorld sw = (SpriteWorld)w;
                foreach (AnimatedSprite ani in children)
                {
                    if (ani is NullAnimatedSprite)
                        continue;

                    sw.RemoveSprite(ani);
                }
            }
        }

        protected override void OnDispose()
        {
            if (Animations != null)
                Animations.Dispose();
        }

        private List<AnimatedSprite> SetupChildren(AnimationHolder animations)
        {
            List<AnimatedSprite> temp = new List<AnimatedSprite>();
            Assembly assembly = Assembly.GetEntryAssembly();
            foreach (string key in animations.Animations.Keys) //To follow this, let's assume this is the "walking left" animation
            {
                Animation ani = animations.Animations[key];

                ani.Owner = this;

                if (ani.Animations == null)
                    continue;

                foreach (string ckey in ani.Animations.Animations.Keys) //To follow this, let's assume this is the "hat" animation for "walking left" animation
                {
                    //ckey = "hat"
                    Animation child_animation = ani.Animations[ckey]; //Same as ani.Animations.Animations[ckey];
                    //Get the animation object for "hat"

                    if (!child_animation.IsEmpty)
                    {

                        Type st = assembly.GetType(child_animation.SpriteFullName); //Get the type for the FullSpriteName
                        AnimatedSprite sprite = (AnimatedSprite)Activator.CreateInstance(st); //Create a new instance of the child sprite

                        child_animation.Owner = sprite; //Set the owner of the "hat" animation to the newly created sprite
                        sprite.Animations = child_animation.Animations; //Set the animations for the hat sprite to the children animations of the "hat" animation.
                        sprite.Parent = this; //Set the parent of the hat sprite to this
                        sprite.Visible = false; //Make sure this sprite isn't visible by default
                        foreach (World w in ContainingWorlds) //For every world this sprite is in
                        {
                            if (w is Worlds.SpriteWorld) //If it's a sprite world
                            {
                                ((Worlds.SpriteWorld)w).AddSprite(sprite); //Add the "hat" sprite to the world
                            }
                        }

                        float xadd = 0f, yadd = 0f;
                        Origin origin = child_animation.OriginType;
                        if ((origin & Origin.Left) != 0)
                        {
                            xadd = -(child_animation.Width / 2f);
                        }
                        if ((origin & Origin.Right) != 0)
                        {
                            xadd = child_animation.Width / 2f;
                        }
                        if ((origin & Origin.Top) != 0)
                        {
                            yadd = child_animation.Height / 2f;
                        }
                        if ((origin & Origin.Bottom) != 0)
                        {
                            yadd = -(child_animation.Height / 2f);
                        }

                        //Move the "hat" sprite to the default center of the sprite
                        sprite.X = X + xadd;
                        sprite.Y = Y + yadd;

                        //Offset the "hat" sprite
                        sprite.X += child_animation.XOffset;
                        sprite.Y += child_animation.YOffset;

                        temp.Add(sprite); //Add the "hat" sprite as a children of this sprite

                        sprite.SetupChildren(sprite.Animations); //Setup the children of "hat" animation, and put all of it's children in a list

                        //Nevermind, don't do that
                        //temp.AddRange(temp2); //Add all of children of the "hat" sprite as our children as well.
                    }
                    else
                    {
                        temp.Add(new NullAnimatedSprite()); //This is an empty animation, fill it with a null sprite
                    }
                }
            }

            return temp; //Return the new sprites created
        }
    }

    public class AnimationHolder
    {
        [JsonProperty(PropertyName="width")]
        internal int width;

        [JsonProperty(PropertyName="height")]
        internal int height;

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
            if (_animations != null) //todo temp workaround
                _animations.Clear();
            _animations = null;
        }
    }
}
