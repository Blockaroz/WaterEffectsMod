using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Water;

public class WaterAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Water;

    public override void Update()
    {
        if (Filters.Scene["WaterEffects:Reflections"] != null)
        {
            if (!Filters.Scene["WaterEffects:Reflections"].Active && WaterConfig.ReflectionsEnabled)
            {
                Filters.Scene.Activate("WaterEffects:Reflections", default);
                Filters.Scene.Deactivate("WaterDistortion");
            }

            if (Filters.Scene["WaterEffects:Reflections"].Active && !WaterConfig.ReflectionsEnabled)
                Filters.Scene.Deactivate("WaterEffects:Reflections", default);
        }

        if (LiquidRenderingSystem.targetsReady)
        {
            if (LiquidRenderingSystem.liquidTargets.TryGetValue(LiquidType, out var target))
            {
                Filters.Scene["WaterDistortion"]?.GetShader()
                    .UseImageScale(new Vector2(0.25f))
                    .UseImage(AllAssets.Texture_Noise[2].Value)
                    .UseImage(target, 2, SamplerState.PointClamp)
                    .UseTargetPosition(Vector2.Zero);
            }
        }
    }

    public override void DrawTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

        LiquidUtils.ApplyMask_Image(LiquidRenderingSystem.liquidTargets[LiquidType], null);

        Main.spriteBatch.Draw(LiquidRenderingSystem.liquidMapTarget, Vector2.Zero, Color.White);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        foreach (Point point in LiquidRenderingSystem.exposedSurfaces)
        {

        }
        
        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
    }

    public override void Draw()
    {
        //Main.spriteBatch.Draw(LiquidRenderingSystem.liquidMapTargetNoCut, Vector2.Zero, Color.White);
    }
}
