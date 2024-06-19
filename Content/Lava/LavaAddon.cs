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
        base.DrawTarget();
    }

    public override void Draw()
    {
        if (!Main.drawToScreen && overlayTarget != null)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Main.spriteBatch.Draw(overlayTarget, Vector2.Zero, Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }
}
