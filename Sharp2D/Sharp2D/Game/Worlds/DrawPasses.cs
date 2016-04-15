using Sharp2D.Core;
using Sharp2D.Core.Graphics.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Sharp2D.Game.Worlds
{
    #region Draw No Alpha Sprites
    public class DrawNoAlpha : DrawPass
    {
        private Shader _ambiantShader;

        public override bool MeetsRequirements(Sprite sprite)
        {
            return sprite.Texture != null && !sprite.HasAlpha;
        }

        public override void DrawSprite(Shader shader, Texture texture, Sprite sprite)
        {
            if (sprite.FirstRun)
            {
                sprite.Display();
                sprite.FirstRun = false;
            }

            if (shader != null)
                shader.Use();

            if (texture != null && sprite.Texture.ID != texture.ID)
                sprite.Texture.Bind();
            else if (texture != null)
                texture.Bind();

            sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

            float width = sprite.Width;
            float height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                height = -height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                width = -width;

            _ambiantShader.Uniforms.SetUniform(sprite.ShaderColor, _ambiantShader.Uniforms["tint"]);
            _ambiantShader.Uniforms.SetUniform(sprite.Scale, _ambiantShader.Uniforms["spriteScale"]);
            _ambiantShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, width, height), _ambiantShader.Uniforms["spritePos"]);
            _ambiantShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), _ambiantShader.Uniforms["texCoordPosAndScale"]);
            _ambiantShader.Uniforms.SetUniform(sprite.Layer, _ambiantShader.Uniforms["spriteDepth"]);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        public override void SetupBatch(SpriteBatch batch) { }

        public override void PrepareForDraw()
        {
            var aspect = Screen.Settings.WindowAspectRatio;

            _ambiantShader.Use();

            _ambiantShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), _ambiantShader.Uniforms["camPosAndScale"]);
            _ambiantShader.Uniforms.SetUniform(aspect.X / aspect.Y, _ambiantShader.Uniforms["screenRatioFix"]);

            _ambiantShader.Uniforms.SetUniform(ParentJob.ParentWorld.AmbientShaderColor, _ambiantShader.Uniforms["brightness"]);
        }

        public override void OnInit()
        {
            _ambiantShader = new Shader("Sharp2D.Resources.sprite_amb.vert", "Sharp2D.Resources.sprite_amb.frag");
            _ambiantShader.LoadAll();
            _ambiantShader.CompileAll();
            //GL.BindAttribLocation(_ambiantShader.ProgramID, GenericRenderJob.PosLocation, "posattrib");
            //GL.BindAttribLocation(_ambiantShader.ProgramID, GenericRenderJob.TexcoordLocation, "tcattrib");
            _ambiantShader.LinkAll();
        }

        public override int Order
        {
            get { return 0; }
        }

        public override void PostDraw()
        {
            GL.DepthMask(false);
        }
    }
    #endregion

    #region Draw No Alpha Sprites With Lights
    public class DrawNoAlphaLight : DrawPass
    {
        private Shader _lightShader;

        public override bool MeetsRequirements(Sprite sprite)
        {
            return sprite.Texture != null && !sprite.HasAlpha && sprite.LightCount > 0;
        }

        public override void DrawSprite(Shader shader, Texture texture, Sprite sprite)
        {
            if (sprite.LightCount == 0)
                return;

            if (sprite.FirstRun)
            {
                sprite.Display();
                sprite.FirstRun = false;
            }

            if (shader != null)
                shader.Use();

            if (texture != null && sprite.Texture.ID != texture.ID)
                sprite.Texture.Bind();
            else if (texture != null)
                texture.Bind();

            sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

            float width = sprite.Width;
            float height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                height = -height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                width = -width;

            _lightShader.Uniforms.SetUniform(sprite.ShaderColor, _lightShader.Uniforms["tint"]);
            _lightShader.Uniforms.SetUniform(sprite.Scale, _lightShader.Uniforms["spriteScale"]);
            _lightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, width, height), _lightShader.Uniforms["spritePos"]);
            _lightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), _lightShader.Uniforms["texCoordPosAndScale"]);
            _lightShader.Uniforms.SetUniform(sprite.Layer, _lightShader.Uniforms["spriteDepth"]);

            lock (sprite.light_lock)
            {
                foreach (Light light in sprite.Lights)
                {
                    _lightShader.Uniforms.SetUniform(light.ShaderColor, _lightShader.Uniforms["lightcolor"]);
                    _lightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), _lightShader.Uniforms["lightdata"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
                foreach (Light light in sprite.dynamicLights)
                {
                    _lightShader.Uniforms.SetUniform(light.ShaderColor, _lightShader.Uniforms["lightcolor"]);
                    _lightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), _lightShader.Uniforms["lightdata"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }

                sprite.dynamicLights.Clear();
                if (!sprite.IsStatic)
                {
                    sprite.Lights.Clear();
                }
            }
        }

        public override void PrepareForDraw()
        {
            var aspect = Screen.Settings.WindowAspectRatio;

            _lightShader.Use();

            _lightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), _lightShader.Uniforms["camPosAndScale"]);
            _lightShader.Uniforms.SetUniform(aspect.X / aspect.Y, _lightShader.Uniforms["screenRatioFix"]);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }

        public override void SetupBatch(SpriteBatch batch)
        {
            var drawBatch = batch as DrawBatch;
            if (drawBatch != null)
                drawBatch.Type = 1;
        }

        public override void OnInit()
        {
            _lightShader = new Shader("Sharp2D.Resources.sprite_light.vert", "Sharp2D.Resources.sprite_light.frag"); //TODO Change files

            _lightShader.LoadAll();
            _lightShader.CompileAll();
            //GL.BindAttribLocation(_lightShader.ProgramID, GenericRenderJob.PosLocation, "posattrib");
            //GL.BindAttribLocation(_lightShader.ProgramID, GenericRenderJob.TexcoordLocation, "tcattrib");
            _lightShader.LinkAll();
        }

        public override int Order
        {
            get { return 1; }
        }

        public override void PostDraw()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
    }
    #endregion

    #region Draw Sprites with Alpha
    public class DrawAlpha : DrawPass
    {
        private static Shader _alphaLightShader;
        public override bool MeetsRequirements(Sprite sprite)
        {
            return sprite.Texture != null && sprite.HasAlpha;
        }

        public override void DrawSprite(Shader shader, Texture texture, Sprite sprite)
        {
            if (sprite.FirstRun)
            {
                sprite.Display();
                sprite.FirstRun = false;
            }

            if (shader != null)
                shader.Use();

            if (texture != null && sprite.Texture.ID != texture.ID)
                sprite.Texture.Bind();
            else if (texture != null)
                texture.Bind();

            sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

            float width = sprite.Width;
            float height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                height = -height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                width = -width;

            _alphaLightShader.Uniforms.SetUniform(sprite.ShaderColor, _alphaLightShader.Uniforms["tint"]);
            _alphaLightShader.Uniforms.SetUniform(sprite.Scale, _alphaLightShader.Uniforms["spriteScale"]);
            _alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, width, height), _alphaLightShader.Uniforms["spritePos"]);
            _alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), _alphaLightShader.Uniforms["texCoordPosAndScale"]);
            _alphaLightShader.Uniforms.SetUniform(sprite.Layer, _alphaLightShader.Uniforms["spriteDepth"]);

            _alphaLightShader.Uniforms.SetUniform(1f, _alphaLightShader.Uniforms["ambientmult"]);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            _alphaLightShader.Uniforms.SetUniform(0f, _alphaLightShader.Uniforms["ambientmult"]);

            lock (sprite.light_lock)
            {
                Light light = null;
                if (sprite.Lights.Count > 0)
                {
                    light = sprite.Lights[0];
                }
                else if (sprite.dynamicLights.Count > 0)
                {
                    light = sprite.dynamicLights[0];
                }

                if (light != null)
                {
                    _alphaLightShader.Uniforms.SetUniform(light.ShaderColor, _alphaLightShader.Uniforms["lightcolor"]);
                    _alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), _alphaLightShader.Uniforms["lightdata"]);
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
            }

            if (sprite.LightCount <= 1)
            {
                sprite.dynamicLights.Clear();
                if (!sprite.IsStatic)
                {
                    sprite.Lights.Clear();
                }
                return;
            }

            lock (sprite.light_lock)
            {
                int i = 0;
                if (sprite.Lights.Count > 0) //If the sprite had static lights
                {
                    i = 1; //Then the first light has already been applied
                }
                for (; i < sprite.Lights.Count; i++)
                {
                    Light light = sprite.Lights[i];
                    _alphaLightShader.Uniforms.SetUniform(light.ShaderColor, _alphaLightShader.Uniforms["lightcolor"]);
                    _alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), _alphaLightShader.Uniforms["lightdata"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }

                i = 0;
                if (sprite.Lights.Count == 0) //If the sprite had no static lights
                {
                    i = 1; //Then the first dynamic light has already been applied 
                }
                for (; i < sprite.dynamicLights.Count; i++)
                {
                    Light light = sprite.dynamicLights[i];
                    _alphaLightShader.Uniforms.SetUniform(light.ShaderColor, _alphaLightShader.Uniforms["lightcolor"]);
                    _alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), _alphaLightShader.Uniforms["lightdata"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }

                sprite.dynamicLights.Clear();
                if (!sprite.IsStatic)
                {
                    sprite.Lights.Clear();
                }
            }
        }

        public override void PrepareForDraw()
        {
            var aspect = Screen.Settings.WindowAspectRatio;
            _alphaLightShader.Use();
            _alphaLightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), _alphaLightShader.Uniforms["camPosAndScale"]);
            _alphaLightShader.Uniforms.SetUniform(aspect.X / aspect.Y, _alphaLightShader.Uniforms["screenRatioFix"]);

            _alphaLightShader.Uniforms.SetUniform(ParentJob.ParentWorld.AmbientShaderColor, _alphaLightShader.Uniforms["ambient"]);


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public override void PostDraw()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(true);
        }

        public override void SetupBatch(SpriteBatch batch)
        {
            var drawBatch = batch as DrawBatch;
            if (drawBatch != null)
                drawBatch.Type = 2;
        }

        public override void OnInit()
        {
            _alphaLightShader = new Shader("Sharp2D.Resources.sprite_light_alpha.vert", "Sharp2D.Resources.sprite_light_alpha.frag");
            _alphaLightShader.LoadAll();
            _alphaLightShader.CompileAll();
            //GL.BindAttribLocation(_alphaLightShader.ProgramID, GenericRenderJob.PosLocation, "posattrib");
            //GL.BindAttribLocation(_alphaLightShader.ProgramID, GenericRenderJob.TexcoordLocation, "tcattrib");
            _alphaLightShader.LinkAll();
        }

        public override int Order
        {
            get { return 2; }
        }
    }
#endregion
}
