using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Lava;

public class LavaAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Lava;

    public override Color LiquidColor => Color.Orange;

    public override void DrawTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

        Effect cutout = AllAssets.ColorCutoutEffect.Value;
        cutout.Parameters["uCutoutColor"].SetValue(LiquidColor.ToVector4());
        cutout.CurrentTechnique.Passes[0].Apply();

        Main.spriteBatch.Draw(LiquidAddonSystem.liquidOverlayTarget, Vector2.Zero, Color.White);

        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);
    }

    public override void Draw()
    {
        if (!Main.drawToScreen && overlayTarget != null)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            //Effect lavaEffect = AllAssets.LavaCausticEffect.Value;
            //lavaEffect.Parameters["uScreenSize"].SetValue(Main.ScreenSize.ToVector2());
            //lavaEffect.Parameters["uScreenPosition"].SetValue(Main.screenPosition * new Vector2(1f / Main.screenWidth, 1f / Main.screenWidth));
            //lavaEffect.Parameters["uTexture0"].SetValue(AllAssets.Noise[1].Value);
            //lavaEffect.Parameters["uTexture1"].SetValue(AllAssets.Noise[0].Value);
            //lavaEffect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly / 600f % 1f);
            //lavaEffect.Parameters["uColor"].SetValue(new Color(210, 30, 0, 30).ToVector4() * 0.7f);
            //lavaEffect.Parameters["uColor2"].SetValue(new Color(255, 120, 10, 20).ToVector4());

            //lavaEffect.Parameters["uSize"].SetValue(new Vector2(1f, 1.5f) * 0.75f);
            //lavaEffect.CurrentTechnique.Passes[0].Apply();

            //Main.spriteBatch.Draw(overlayTarget, Vector2.Zero, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }
}
