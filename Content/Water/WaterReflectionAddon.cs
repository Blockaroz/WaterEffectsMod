using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Water;

public class WaterReflectionAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Water;

    public override Color LiquidColor => Color.Blue;

    public override bool AddToColorRendering => false;

    public override bool HasVisuals => true;

    public override bool HasAudio => false;

    public static RenderTarget2D reflectionTarget;
    public static RenderTarget2D reflectionTargetSwap;

    public override void DrawTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);

        Effect cutout = AllAssets.ColorCutoutEffect.Value;
        cutout.Parameters["uCutoutColor"].SetValue(LiquidColor.ToVector4());
        cutout.CurrentTechnique.Passes[0].Apply();

        Main.spriteBatch.Draw(LiquidAddonSystem.liquidOverlayTarget, Vector2.Zero, LiquidAddonSystem.liquidOverlayTarget.Frame(), Color.White, 0, Vector2.Zero, 1, Main.GameViewMatrix.Effects, 0);

        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);

        int width = LiquidUtils.DefaultTargetWidth;
        int height = LiquidUtils.DefaultTargetHeight;
        if (reflectionTarget == null || reflectionTargetSwap == null || width != currentWidth || height != currentHeight)
        {
            reflectionTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            reflectionTargetSwap = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            currentWidth = width;
            currentHeight = height;
            return;
        }

        if (WaterConfig.ReflectionsEnabled)
        {
            Main.instance.GraphicsDevice.SetRenderTarget(reflectionTargetSwap);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            LiquidUtils.GetAreaForDrawing(out int left, out int right, out int top, out int bottom);

            LiquidUtils.DrawReflectionMapInArea(left, right, top, bottom, WaterConfig.Instance.reflectionBlockDepth);

            Main.instance.GraphicsDevice.SetRenderTarget(reflectionTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);

            Main.spriteBatch.Draw(reflectionTargetSwap, Vector2.Zero, reflectionTarget.Frame(), Color.White, 0, Vector2.Zero, 1f, Main.GameViewMatrix.Effects, 0);

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(null);

            if (!Main.drawToScreen && overlayTarget != null && reflectionTarget != null)
            {
                Filters.Scene["WaterDistortion"].GetShader().UseImage(AllAssets.Noise[2].Value);

                Effect effect = Filters.Scene["WaterEffects:Reflections"].GetShader().Shader;
                effect.Parameters["uImageSize"].SetValue(Main.ScreenSize.ToVector2());
                effect.Parameters["uScreenCutout"].SetValue(overlayTarget);
                effect.Parameters["uReflectionMap"].SetValue(reflectionTarget);
                effect.Parameters["uDepth"].SetValue(WaterConfig.Instance.reflectionBlockDepth);
                effect.Parameters["uClearness"].SetValue(_clarity);
            }
        }
    }

    public override void Draw()
    {
    }

    private float _clarity;

    public override void Update()
    {
        if (!Filters.Scene["WaterEffects:Reflections"].IsActive() && WaterConfig.ReflectionsEnabled)
            Filters.Scene.Activate("WaterEffects:Reflections", default);
        if (Filters.Scene["WaterEffects:Reflections"].IsActive() && !WaterConfig.ReflectionsEnabled)
            Filters.Scene.Deactivate("WaterEffects:Reflections", default);

        Filters.Scene["WaterEffects:Reflections"].GetShader().UseOpacity(WaterConfig.ReflectionsEnabled ? 1f : 0f);

        float clarityTarget = 0.6f;

        if (Main.bloodMoon)
            clarityTarget *= 0.5f;

        if (Main.LocalPlayer.ZoneShimmer)
            clarityTarget = 0.7f;

        if (Main.LocalPlayer.ZoneWaterCandle)
            clarityTarget *= 0.65f;
        if (Main.LocalPlayer.ZonePeaceCandle)
            clarityTarget *= 1.35f;

        _clarity = MathHelper.Lerp(_clarity, clarityTarget, 0.1f);
    }
}
