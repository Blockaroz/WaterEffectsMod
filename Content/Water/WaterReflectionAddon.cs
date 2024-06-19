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
        base.DrawTarget();

        if (reflectionTarget == null || reflectionTargetSwap == null || Main.screenWidth != currentWidth || Main.screenHeight != currentHeight)
        {
            reflectionTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            reflectionTargetSwap = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            currentWidth = Main.screenWidth;
            currentHeight = Main.screenHeight;
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

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

            if (Main.WaveQuality > 0)
            {
                Filters.Scene["WaterDistortion"].GetShader().Apply();
            }

            Main.spriteBatch.Draw(reflectionTargetSwap, Vector2.Zero, Color.White);

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(null);

            if (!Main.drawToScreen && overlayTarget != null && reflectionTarget != null)
            {
                Filters.Scene["WaterDistortion"].GetShader().UseImage(AllAssets.Noise[2].Value);

                if (!Filters.Scene["WaterEffects:Reflections"].IsActive())
                    Filters.Scene.Activate("WaterEffects:Reflections", default);

                Filters.Scene["WaterEffects:Reflections"].GetShader().UseOpacity(WaterConfig.ReflectionsEnabled ? 1f : 0f);

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
        float clarityTarget = 0.4f;

        if (Main.bloodMoon)
            clarityTarget = 0.15f;

        if (Main.LocalPlayer.ZoneShimmer)
            clarityTarget = 0.55f;

        if (Main.LocalPlayer.ZoneWaterCandle)
            clarityTarget *= 0.65f;
        if (Main.LocalPlayer.ZonePeaceCandle)
            clarityTarget *= 1.35f;

        _clarity = MathHelper.Lerp(_clarity, clarityTarget, 0.1f);
    }
}
