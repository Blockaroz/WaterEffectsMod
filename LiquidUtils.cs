using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using WaterEffectsMod.Common;

namespace WaterEffectsMod;

public static class LiquidUtils
{
    public static void GetAreaForDrawing(out int left, out int right, out int top, out int bottom)
    {
        left = (int)Math.Floor(Main.screenPosition.X / 16f - 1);
        right = (int)Math.Ceiling((Main.screenPosition.X + Main.screenWidth) / 16f + 1);
        top = (int)Math.Floor(Main.screenPosition.Y / 16f - 2);
        bottom = (int)Math.Ceiling((Main.screenPosition.Y + Main.screenHeight) / 16f + 5);

        left = Math.Max(left, 4);
        right = Math.Min(right, Main.maxTilesX - 4);
        top = Math.Max(top, 4);
        bottom = Math.Min(bottom, Main.maxTilesY - 4);
    }

    public static void DrawSingleTile(int i, int j, Vector2 offset)
    {
        Tile tile = Main.tile[i, j];
        Main.instance.LoadTiles(tile.TileType);
        Texture2D tileTexture = TextureAssets.Tile[tile.TileType].Value;
        Rectangle tileFrame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
        Vector2 tilePos = new Vector2(i * 16f, j * 16f);
        VertexColors color = new VertexColors(Color.White);
        if ((tile.Slope == 0 || TileID.Sets.HasSlopeFrames[tile.TileType]) && !tile.IsHalfBlock)
        {
            if (!TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[tile.TileType] && (Main.tile[i - 1, j].IsHalfBlock || Main.tile[i + 1, j].IsHalfBlock))
            {
                int frameOff = 4;
                if (TileID.Sets.AllBlocksWithSmoothBordersToResolveHalfBlockIssue[tile.TileType])
                    frameOff = 2;

                if (Main.tile[i - 1, j].IsHalfBlock)
                {
                    Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(0f, 8f) - offset, new Rectangle(tile.TileFrameX, tile.TileFrameY + 8, 16, 8), color, Vector2.Zero, 1f, 0);
                    Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(frameOff, 0f) - offset, new Rectangle(tile.TileFrameX + frameOff, tile.TileFrameY, 16 - frameOff, 16), color, Vector2.Zero, 1f, 0);
                    Main.tileBatch.Draw(tileTexture, tilePos - offset, new Rectangle(144, 0, frameOff, 8), color, Vector2.Zero, 1f, 0);
                    if (frameOff == 2)
                        Main.tileBatch.Draw(tileTexture, tilePos - offset, new Rectangle(148, 0, 2, 2), color, Vector2.Zero, 1f, 0);
                }
                else if (Main.tile[i + 1, j].IsHalfBlock)
                {
                    Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(0f, 8f) - offset, new Rectangle(tile.TileFrameX, tile.TileFrameY + 8, 16, 8), color, Vector2.Zero, 1f, 0);
                    Main.tileBatch.Draw(tileTexture, tilePos - offset, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16 - frameOff, 16), color, Vector2.Zero, 1f, 0);
                    Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(16 - frameOff, 0f) - offset, new Rectangle(144 + (16 - frameOff), 0, frameOff, 8), color, Vector2.Zero, 1f, 0);
                    if (frameOff == 2)
                        Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(14f, 0f) - offset, new Rectangle(156, 0, 2, 2), color, Vector2.Zero, 1f, 0);
                }
            }
            else
            {
                Main.tileBatch.Draw(tileTexture, tilePos - offset, tileFrame, color, Vector2.Zero, 1f, 0);
            }
        }
        else if (tile.IsHalfBlock)
        {
            tilePos.Y += 8;
            tileFrame.Height -= 8;
            Main.tileBatch.Draw(tileTexture, tilePos - offset, tileFrame, color, Vector2.Zero, 1f, 0);
        }
        else
        {
            for (int iSlope = 0; iSlope < 8; iSlope++)
            {
                int num3 = iSlope * -2;
                int num4 = 16 - iSlope * 2;
                int num5 = 16 - num4;
                int num6;
                switch ((int)tile.Slope)
                {
                    case 1:
                        num3 = 0;
                        num6 = iSlope * 2;
                        num4 = 14 - iSlope * 2;
                        num5 = 0;
                        break;
                    case 2:
                        num3 = 0;
                        num6 = 16 - iSlope * 2 - 2;
                        num4 = 14 - iSlope * 2;
                        num5 = 0;
                        break;
                    case 3:
                        num6 = iSlope * 2;
                        break;
                    default:
                        num6 = 16 - iSlope * 2 - 2;
                        break;
                }
                Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(num6, iSlope * 2 + num3) - offset, new Rectangle(tile.TileFrameX + num6, tile.TileFrameY + num5, 2, num4), color, Vector2.Zero, 1f, 0);
            }

            int bottomOff = (int)tile.Slope <= 2 ? 14 : 0;
            Main.tileBatch.Draw(tileTexture, tilePos + new Vector2(0, bottomOff) - offset, new Rectangle(tile.TileFrameX, tile.TileFrameY + bottomOff, 16, 2), color, Vector2.Zero, 1f, 0);
        }
    }
    public static BlendState BlendStateForReflectionMap => new BlendState()
    {
        ColorBlendFunction = BlendFunction.Max,
        ColorSourceBlend = Blend.SourceColor
    };
    public static void DrawSurfaceMap(int left, int right, int top, int bottom, int maxDepth)
    {
        Effect mapEffect = AllAssets.Effect_ReflectionMap.Value;
        mapEffect.Parameters["uDepth"].SetValue(maxDepth);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendStateForReflectionMap, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, mapEffect);
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

                    Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - 14, currentJ * 16 + 16 - liquidHeight - 2) - drawOffset, new Rectangle(0, 0, 16 * 3 - 4, trueDepth), new Color(255, 255, 255, 0));

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
            && (!WorldGen.SolidOrSlopedTile(i, j - 1) || blockTypesAllowsReflections.Any(n => n == Main.tile[i, j - 1].TileType));
        return incomplete || airAbove;
    }

    public static readonly List<int> blockTypesAllowsReflections = new List<int>()
    {
        TileID.Glass,
        TileID.BreakableIce,
        TileID.MagicalIceBlock,
    };

    public static void ApplyMask_Color(Texture2D colorMask, Color color, bool alpha = false)
    {
        Effect mask = AllAssets.Effect_ImageMask.Value;
        mask.Parameters["uMaskAdd"].SetValue(1);
        mask.Parameters["uMaskSubtract"].SetValue(0);
        mask.Parameters["uMaskColor"].SetValue(colorMask);
        mask.Parameters["useAlpha"].SetValue(alpha);
        mask.Parameters["useColor"].SetValue(true);
        mask.Parameters["uColor"].SetValue(color.ToVector4());
        mask.CurrentTechnique.Passes[0].Apply();
    }

    public static void ApplyMask_Image(Texture2D add, Texture2D subtract, bool alpha = false)
    {
        Effect mask = AllAssets.Effect_ImageMask.Value;
        mask.Parameters["uMaskAdd"].SetValue(add);
        mask.Parameters["uMaskSubtract"].SetValue(subtract);
        mask.Parameters["uMaskColor"].SetValue(1);
        mask.Parameters["useAlpha"].SetValue(alpha);
        mask.Parameters["useColor"].SetValue(false);
        mask.CurrentTechnique.Passes[0].Apply();
    }    
    
    public static void ApplyMask_ImageColor(Texture2D add, Texture2D subtract, Texture2D colorMask, Color color, bool alpha = false)
    {
        Effect mask = AllAssets.Effect_ImageMask.Value;
        mask.Parameters["uMaskAdd"].SetValue(add);
        mask.Parameters["uMaskSubtract"].SetValue(subtract);
        mask.Parameters["uMaskColor"].SetValue(colorMask);
        mask.Parameters["useAlpha"].SetValue(alpha);
        mask.Parameters["useColor"].SetValue(true);
        mask.Parameters["uColor"].SetValue(color.ToVector4());
        mask.CurrentTechnique.Passes[0].Apply();
    }
}
