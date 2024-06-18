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

    public override Color LiquidColor => LiquidAddonSystem.liquidColors[LiquidType];

    public override bool AddToColorRendering => false;

    //TODO: Make static?
    public RenderTarget2D reflectionTarget;

    public override void DrawTarget()
    {
        base.DrawTarget();

        if (reflectionTarget == null || Main.screenWidth != currentWidth || Main.screenHeight != currentHeight)
        {
            reflectionTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            currentWidth = Main.screenWidth;
            currentHeight = Main.screenHeight;
            return;
        }

        Main.instance.GraphicsDevice.SetRenderTarget(reflectionTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        int screenLeft = (int)Math.Floor(Main.screenPosition.X / 16f - 1);
        int screenRight = (int)Math.Ceiling((Main.screenPosition.X + Main.screenWidth) / 16f + 1);
        int screenTop = (int)Math.Floor(Main.screenPosition.Y / 16f - 1);
        int screenBottom = (int)Math.Ceiling((Main.screenPosition.Y + Main.screenHeight) / 16f + 1);

        LiquidUtils.DrawReflectionMapInArea(screenLeft, screenRight, screenTop, screenBottom, WaterConfig.Instance.waterReflectionBlockDepth);

        Main.instance.GraphicsDevice.SetRenderTarget(null);
    }

    public override void Draw()
    {
        Filters.Scene["WaterDistortion"].GetShader().UseImage(AllAssets.Noise[2].Value);

        if (!Main.drawToScreen && overlayTarget != null && reflectionTarget != null)
        {
            Effect effect = Filters.Scene["WaterEffects:Reflections"].GetShader().Shader;
            effect.Parameters["uImageSize"].SetValue(Main.ScreenSize.ToVector2());
            effect.Parameters["uScreenImage"].SetValue(reflectionTarget);
            effect.Parameters["uScreenCutout"].SetValue(overlayTarget);
            effect.Parameters["uDepth"].SetValue(WaterConfig.Instance.waterReflectionBlockDepth);
            effect.Parameters["uClearness"].SetValue(0.2f);
        }
    }

    private float _clarity;

    public override void Update()
    {
        if (!Filters.Scene["WaterEffects:Reflections"].IsInUse())
        {
            Filters.Scene["WaterEffects:Reflections"] = new Filter(new ScreenShaderData(AllAssets.ReflectionEffect[0], "ShaderPass"), EffectPriority.VeryLow);
            Filters.Scene.Activate("WaterEffects:Reflections", default);
        }

        Filters.Scene["WaterEffects:Reflections"].GetShader().UseOpacity(WaterConfig.Instance.waterReflectionsEnabled ? 1f : 0f);
    }
}
