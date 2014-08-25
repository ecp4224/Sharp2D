using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharp2D.Core.Graphics.Shaders;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Sharp2D.Game.Worlds
{
    #region Draw No Alpha Sprites
    public class DrawNoAlpha : DrawPass
    {
        private Shader ambiantShader;

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

            float Width = sprite.Width;
            float Height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                Height = -Height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                Width = -Width;

            ambiantShader.Uniforms.SetUniform(sprite.Scale, ambiantShader.Uniforms["spriteScale"]);
            ambiantShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, Width, Height), ambiantShader.Uniforms["spritePos"]);
            float tsize = sprite.TexCoords.SquardSize;
            ambiantShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), ambiantShader.Uniforms["texCoordPosAndScale"]);
            ambiantShader.Uniforms.SetUniform(sprite.Layer, ambiantShader.Uniforms["spriteDepth"]);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        public override void SetupBatch(ref DrawBatch batch) { }

        public override void PrepareForDraw()
        {
            var aspect = Screen.Settings.WindowAspectRatio;

            ambiantShader.Use();

            ambiantShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), ambiantShader.Uniforms["camPosAndScale"]);
            ambiantShader.Uniforms.SetUniform(aspect.X / aspect.Y, ambiantShader.Uniforms["screenRatioFix"]);

            ambiantShader.Uniforms.SetUniform(ParentJob.ParentWorld.AmbientShaderColor, ambiantShader.Uniforms["brightness"]);
        }

        public override void OnInit()
        {
            ambiantShader = new Shader("Sharp2D.Resources.sprite_amb.vert", "Sharp2D.Resources.sprite_amb.frag");
            ambiantShader.LoadAll();
            ambiantShader.CompileAll();
            GL.BindAttribLocation(ambiantShader.ProgramID, GenericRenderJob.POS_LOCATION, "posattrib");
            GL.BindAttribLocation(ambiantShader.ProgramID, GenericRenderJob.TEXCOORD_LOCATION, "tcattrib");
            ambiantShader.LinkAll();
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
        private Shader lightShader;

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

            float Width = sprite.Width;
            float Height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                Height = -Height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                Width = -Width;

            lightShader.Uniforms.SetUniform(sprite.Scale, lightShader.Uniforms["spriteScale"]);
            lightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, Width, Height), lightShader.Uniforms["spritePos"]);
            float tsize = sprite.TexCoords.SquardSize;
            lightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), lightShader.Uniforms["texCoordPosAndScale"]);
            lightShader.Uniforms.SetUniform(sprite.Layer, lightShader.Uniforms["spriteDepth"]);

            lock (sprite.light_lock)
            {
                foreach (Light light in sprite.Lights)
                {
                    lightShader.Uniforms.SetUniform(light.ShaderColor, lightShader.Uniforms["lightcolor"]);
                    lightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), lightShader.Uniforms["lightdata"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
                foreach (Light light in sprite.dynamicLights)
                {
                    lightShader.Uniforms.SetUniform(light.ShaderColor, lightShader.Uniforms["lightcolor"]);
                    lightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), lightShader.Uniforms["lightdata"]);

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

            lightShader.Use();

            lightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), lightShader.Uniforms["camPosAndScale"]);
            lightShader.Uniforms.SetUniform(aspect.X / aspect.Y, lightShader.Uniforms["screenRatioFix"]);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }

        public override void SetupBatch(ref DrawBatch batch)
        {
            batch.type = 1;
        }

        public override void OnInit()
        {
            lightShader = new Shader("Sharp2D.Resources.sprite_light.vert", "Sharp2D.Resources.sprite_light.frag"); //TODO Change files

            lightShader.LoadAll();
            lightShader.CompileAll();
            GL.BindAttribLocation(lightShader.ProgramID, GenericRenderJob.POS_LOCATION, "posattrib");
            GL.BindAttribLocation(lightShader.ProgramID, GenericRenderJob.TEXCOORD_LOCATION, "tcattrib");
            lightShader.LinkAll();
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
        private static Shader alphaLightShader;
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

            float Width = sprite.Width;
            float Height = sprite.Height;

            if ((sprite.FlipState & FlipState.Vertical) != 0)
                Height = -Height;
            if ((sprite.FlipState & FlipState.Horizontal) != 0)
                Width = -Width;

            alphaLightShader.Uniforms.SetUniform(sprite.Scale, alphaLightShader.Uniforms["spriteScale"]);
            alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, Width, Height), alphaLightShader.Uniforms["spritePos"]);
            float tsize = sprite.TexCoords.SquardSize;
            alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), alphaLightShader.Uniforms["texCoordPosAndScale"]);
            alphaLightShader.Uniforms.SetUniform(sprite.Layer, alphaLightShader.Uniforms["spriteDepth"]);

            alphaLightShader.Uniforms.SetUniform(1f, alphaLightShader.Uniforms["ambientmult"]);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

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
                    alphaLightShader.Uniforms.SetUniform(light.ShaderColor, alphaLightShader.Uniforms["lightcolor"]);
                    alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), alphaLightShader.Uniforms["lightdata"]);
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
            }

            if (sprite.LightCount <= 1)
                return;

            alphaLightShader.Uniforms.SetUniform(0f, alphaLightShader.Uniforms["ambientmult"]);

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
                    alphaLightShader.Uniforms.SetUniform(light.ShaderColor, alphaLightShader.Uniforms["lightcolor"]);
                    alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), alphaLightShader.Uniforms["lightdata"]);

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
                    alphaLightShader.Uniforms.SetUniform(light.ShaderColor, alphaLightShader.Uniforms["lightcolor"]);
                    alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), alphaLightShader.Uniforms["lightdata"]);

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
            alphaLightShader.Use();
            alphaLightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), alphaLightShader.Uniforms["camPosAndScale"]);
            alphaLightShader.Uniforms.SetUniform(aspect.X / aspect.Y, alphaLightShader.Uniforms["screenRatioFix"]);

            alphaLightShader.Uniforms.SetUniform(ParentJob.ParentWorld.AmbientShaderColor, alphaLightShader.Uniforms["ambient"]);


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public override void PostDraw()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(true);
        }

        public override void SetupBatch(ref DrawBatch batch)
        {
            batch.type = 2;
        }

        public override void OnInit()
        {
            alphaLightShader = new Shader("Sharp2D.Resources.sprite_light_alpha.vert", "Sharp2D.Resources.sprite_light_alpha.frag");
            alphaLightShader.LoadAll();
            alphaLightShader.CompileAll();
            GL.BindAttribLocation(alphaLightShader.ProgramID, GenericRenderJob.POS_LOCATION, "posattrib");
            GL.BindAttribLocation(alphaLightShader.ProgramID, GenericRenderJob.TEXCOORD_LOCATION, "tcattrib");
            alphaLightShader.LinkAll();
        }

        public override int Order
        {
            get { return 2; }
        }
    }
#endregion
}
