using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;

namespace WaterEffectsMod;

public static class LiquidUtils
{
    public static void DrawAllLiquidInArea(int left, int right, int top, int bottom, int liquidType)
    {
    }

    public static BlendState BlendStateForReflectionMap => new BlendState()
    {
        ColorBlendFunction = BlendFunction.Max,
        ColorSourceBlend = Blend.SourceColor
    };

    public static void DrawReflectionMapInArea(int left, int right, int top, int bottom, int maxDepth)
    {
        Effect mapEffect = AllAssets.ReflectionMapEffect.Value;
        mapEffect.Parameters["uDepth"].SetValue(maxDepth);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendStateForReflectionMap, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, mapEffect, Main.Transform);

        Vector2 drawOffset = Main.screenPosition;

        for (int i = left; i < right; i++)
        {
            for (int j = top; j < bottom; j++)
            {
                if (!WorldGen.InWorld(i, j))
                    continue;                
                
                if (!WorldGen.InWorld(i, j - 1))
                    continue;

                int currentJ = j;

                if (j == top && Main.tile[i, j].LiquidAmount >= 255)
                {
                    while (true)
                    {
                        if (!WorldGen.InWorld(i, currentJ))
                            break;

                        if (!WorldGen.InWorld(i, currentJ - 1))
                            break;

                        if (IsSurfaceLiquid(i, currentJ))
                            break;

                        currentJ--;
                    }
                }

                if (IsSurfaceLiquid(i, currentJ))
                {
                    int poolDepth = 0;

                    while (true)
                    {
                        if (!WorldGen.InWorld(i, currentJ + poolDepth))
                            break;

                        if (Main.tile[i, currentJ + poolDepth].LiquidAmount <= 0)
                            break;

                        poolDepth++;
                    }

                    int liquidHeight = (int)Math.Clamp(Main.tile[i, currentJ].LiquidAmount / 255f * 16f, 2, 16) + 1;
                    int trueDepth = Math.Min(poolDepth * 16 + liquidHeight, maxDepth * 16);

                    Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - 20, currentJ * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 46, trueDepth), new Color(255, 255, 255, 0));

                    currentJ += poolDepth;
                }
            }
        }

        Main.spriteBatch.End();
    }

    public static bool IsSurfaceLiquid(int i, int j)
    {
        bool incomplete = Main.tile[i, j].LiquidAmount < 255 && Main.tile[i, j].LiquidAmount > 0;
        bool airAbove = Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j - 1].LiquidAmount <= 0 
            && (!Main.tileSolid[Main.tile[i, j - 1].TileType] || !Main.tile[i, j - 1].HasTile || Main.tile[i, j - 1].TileType == TileID.Glass);
        return incomplete || airAbove;
    }

    public static Color ReflectionDepthColor(float x)
    {
        return Color.White;
    }
}
