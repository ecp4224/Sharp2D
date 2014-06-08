﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Worlds
{
    public class GenericCamera : Camera
    {
        internal override void BeforeRender()
        {
            GL.Scale(Z, Z, 0f);
            GL.Translate(-X, -Y, 0f);
        }
    }
}