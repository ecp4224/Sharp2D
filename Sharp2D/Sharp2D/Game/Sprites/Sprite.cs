﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;
using OpenTK;

namespace Sharp2D.Game.Sprites
{
    public abstract class Sprite : IDisposable
    {
        ~Sprite()
        {
            Dispose();
        }

        internal bool FirstRun = true;
        public bool Loaded { get; private set; }
        public Texture Texture { get; set; }
        public Vector2 Position { get { return new Vector2(X, Y); } }
        public TexCoords TexCoords;
        private float _rot;
        public float Rotation
        {
            get { return _rot; }
            set
            {
                while (value > 360f)
                    value -= 360f;
                while (value < 0f)
                    value += 360f;

                _rot = value;
            }
        }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsOffScreen
        {
            get
            {
                return (X + Width) - Screen.Camera.X < -32 || Math.Abs(Screen.Camera.X - (X - Width)) > 32 + (Screen.Settings.GameSize.Width / Screen.Camera.Z) || (Y + Height) - Screen.Camera.Y < -32 || Math.Abs(Screen.Camera.Y - (Y - Height)) > 32 + (Screen.Settings.GameSize.Height / Screen.Camera.Z);

            }
        }
        public virtual float X { get; set; }

        public virtual float Y { get; set; }

        public void Load()
        {
            OnLoad();
            Loaded = true;
        }

        public void Unload()
        {
            OnUnload();
            Loaded = false;
        }

        public void Dispose()
        {
            if (Loaded)
                Unload();
            OnDispose();
        }

        public abstract void OnLoad();

        public abstract void OnUnload();

        public abstract void OnDispose();

        public abstract void OnDisplay();

    }
}