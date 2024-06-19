using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public class LiquidAddonSystem : ModSystem
{
    public static List<LiquidAddon> liquidAddons;
    public static Dictionary<int, Color> liquidColors;
    public static List<int> blockTypesAllowsReflections;

    public override void Load()
    {
        liquidAddons = new List<LiquidAddon>();
        liquidColors = new Dictionary<int, Color>();
        blockTypesAllowsReflections = new List<int>()
        {
            TileID.Glass,
            TileID.BreakableIce,
            TileID.MagicalIceBlock,
        };

        if (!Main.dedServ)
        {
            On_Main.CheckMonoliths += RenderAddonOverlays;
            IL_Main.DoDraw += AddDrawOverlay;
        }
    }

    private void AddDrawOverlay(ILContext il)
    {
        try
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchLdsfld<Main>("waterTarget"));
            c.TryGotoNext(i => i.MatchCallvirt<SpriteBatch>("Draw"));

            c.Index++;
            c.EmitDelegate(() =>
            {
                foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
                    addon.Draw();
            });
        }
        catch
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    public static RenderTarget2D liquidOverlayTargetSwap;
    public static RenderTarget2D liquidOverlayTarget;
    public static RenderTarget2D liquidEdgesMaskTarget;

    private int currentWidth;
    private int currentHeight;

    private void RenderAddonOverlays(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        if (!Main.drawToScreen && !Main.gameMenu)
        {
            if (liquidOverlayTarget == null || liquidOverlayTargetSwap == null || liquidEdgesMaskTarget == null || Main.screenWidth != currentWidth || Main.screenHeight != currentHeight)
            {
                liquidOverlayTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenWidth, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                liquidOverlayTargetSwap = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenWidth, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                liquidEdgesMaskTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenWidth, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                currentWidth = Main.screenWidth;
                currentHeight = Main.screenHeight;
                return;
            }

            LiquidUtils.GetAreaForDrawing(out int left, out int right, out int top, out int bottom);

            Main.instance.GraphicsDevice.SetRenderTarget(liquidOverlayTargetSwap);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

            Vector2 drawOffset = Main.screenPosition;
            HashSet<Point> tilesToDrawForMask = new HashSet<Point>();

            for (int j = top; j < bottom; j++)
            {
                for (int i = left; i < right; i++)
                {
                    if (!WorldGen.InWorld(i, j))
                        continue;

                    if (Main.tile[i, j].LiquidAmount > 0)
                    {
                        int liquidType = Main.tile[i, j].LiquidType;
                        int k = 0;
                        while (true)
                        {
                            bool stop = false;
                            if (!WorldGen.InWorld(i + k, j))
                            {
                                k--;
                                stop = true;
                            }

                            if (Main.tile[i + k, j].LiquidAmount <= 0 || Main.tile[i + k, j].LiquidType != liquidType || stop)
                            {
                                int liquidHeight = Math.Clamp((int)Math.Ceiling(Main.tile[i, j].LiquidAmount / 255f * 16f), 4, 16);
                                int size = k * 16;

                                if (WorldGen.InWorld(i, j - 1))
                                {
                                    const int extend = 4;
                                    if (liquidHeight >= 16 && (Main.tile[i, j - 1].LiquidAmount > 0 || WorldGen.SolidTile(i, j - 1)))
                                    {
                                        Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, size + 2 * extend, liquidHeight), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                        Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight - extend) - drawOffset, new Rectangle(0, 0, size, liquidHeight + 2 * extend), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                    }
                                    else
                                    {
                                        Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, size + 2 * extend, liquidHeight), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                        Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, size, liquidHeight + extend), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                    }
                                }

                                i += k - 1;

                                break;
                            }
                            k++;
                        }
                    }

                    if (Main.tile[i, j].HasTile)
                    {
                        int liquidHeight = 0;
                        int liquidType = -1;
                        bool foundValid = false;

                        if (!foundValid && WorldGen.InWorld(i - 1, j))
                        {
                            if (Main.tile[i - 1, j].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i - 1, j].LiquidType;
                                liquidHeight = Math.Clamp((int)Math.Ceiling(Main.tile[i - 1, j].LiquidAmount / 255f * 16f), 4, 16);
                                foundValid = true;
                            }
                        }

                        if (!foundValid && WorldGen.InWorld(i + 1, j))
                        {
                            if (Main.tile[i + 1, j].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i + 1, j].LiquidType;
                                liquidHeight = Math.Clamp((int)Math.Ceiling(Main.tile[i + 1, j].LiquidAmount / 255f * 16f), 4, 16);
                                foundValid = true;
                            }
                        }

                        if (!foundValid && WorldGen.InWorld(i, j - 1))
                        {
                            if (Main.tile[i, j - 1].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i, j - 1].LiquidType;
                                liquidHeight = 16;
                                foundValid = true;
                            }
                        }

                        if (!foundValid && WorldGen.InWorld(i, j + 1))
                        {
                            if (Main.tile[i, j + 1].LiquidAmount >= 255)
                            {
                                liquidType = Main.tile[i, j + 1].LiquidType;
                                liquidHeight = 16;
                                foundValid = true;
                            }
                        }

                        if (foundValid)
                        {
                            if (Main.tile[i, j].IsHalfBlock && liquidHeight > 8)
                                Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - 2, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 20, Math.Min(liquidHeight, 8) + 2), new VertexColors(liquidColors[liquidType]), Vector2.Zero, 1f, 0);
                            else if (Main.tile[i, j].Slope != 0)
                                Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(((int)Main.tile[i, j].Slope - 1) * 18, 16 - liquidHeight, 16, liquidHeight), new VertexColors(liquidColors[liquidType]), Vector2.Zero, 1f, 0);

                            if (WorldGen.SolidOrSlopedTile(i, j))
                                tilesToDrawForMask.Add(new Point(i, j));
                        }
                    }
                }
            }

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(liquidEdgesMaskTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

            foreach (Point point in tilesToDrawForMask)
            {
                if (WorldGen.InWorld(point.X, point.Y, 1))
                    LiquidUtils.DrawSingleTile(point.X, point.Y, drawOffset);
            }

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(liquidOverlayTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

            Effect mask = AllAssets.ImageMaskEffect.Value;
            mask.Parameters["uMaskImage"].SetValue(liquidEdgesMaskTarget);
            mask.Parameters["uMaskSize"].SetValue(liquidEdgesMaskTarget.Size());
            mask.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(liquidOverlayTargetSwap, Vector2.Zero, Color.White);

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(null);

            foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
                addon.CreateAndDrawTarget();
        }
    }

    public override void PostUpdateDusts()
    {
        foreach (LiquidAddon addon in liquidAddons)
            addon.Update();
    }
}
