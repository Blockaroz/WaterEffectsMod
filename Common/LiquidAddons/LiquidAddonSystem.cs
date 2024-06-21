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
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common.LiquidAddons;

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
    public static RenderTarget2D tileMaskTarget;
    public static RenderTarget2D backWaterTargetFixed;

    private int currentWidth;
    private int currentHeight;

    private void RenderAddonOverlays(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        if (!Main.drawToScreen && !Main.gameMenu)
        {
            Vector2 drawOffset = Main.screenPosition;

            int width = LiquidUtils.DefaultTargetWidth;
            int height = LiquidUtils.DefaultTargetHeight;
            if (liquidOverlayTarget == null || liquidOverlayTargetSwap == null || tileMaskTarget == null || backWaterTargetFixed == null || width != currentWidth || height != currentHeight)
            {
                liquidOverlayTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                liquidOverlayTargetSwap = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                tileMaskTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                backWaterTargetFixed = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                currentWidth = width;
                currentHeight = height;
                return;
            }

            Main.instance.GraphicsDevice.SetRenderTarget(liquidOverlayTargetSwap);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin();

            LiquidUtils.GetAreaForDrawing(out int left, out int right, out int top, out int bottom);

            HashSet<Point> tileMaskPoints = new HashSet<Point>();

            for (int i = left; i < right; i++)
            {
                for (int j = top; j < bottom; j++)
                {
                    if (!WorldGen.InWorld(i, j))
                        continue;

                    if (Main.tile[i, j].LiquidAmount > 0 && liquidColors.ContainsKey(Main.tile[i, j].LiquidType))
                    {
                        int startAmount = Main.tile[i, j].LiquidAmount;
                        int startType = Main.tile[i, j].LiquidType;
                        int k = 0;
                        while (true)
                        {
                            bool stop = false;
                            if (!WorldGen.InWorld(i, j + k) || j + k > bottom)
                            {
                                k--;
                                stop = true;
                            }

                            if (Main.tile[i, j + k].LiquidAmount <= 0 || Main.tile[i, j + k].LiquidAmount != startAmount || Main.tile[i, j + k].LiquidType != startType || stop)
                            {
                                int liquidHeight = Math.Max(4, (int)Math.Ceiling(Main.tile[i, j].LiquidAmount / 255f * 16f));

                                int size = (k - 1) * 16;

                                const int extend = 4;
                                if (!LiquidUtils.IsSurfaceLiquid(i, j))
                                {
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16 + 2 * extend, size + liquidHeight), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight - extend) - drawOffset, new Rectangle(0, 0, 16, size + liquidHeight + 2 * extend), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                }
                                else
                                {
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16 + 2 * extend, size + liquidHeight), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16, size + liquidHeight + extend), new VertexColors(liquidColors[Main.tile[i, j].LiquidType]), Vector2.Zero, 1f, 0);
                                }

                                j += k - 1;

                                break;
                            }
                            k++;
                        }
                    }

                    if (WorldGen.SolidOrSlopedTile(i, j))
                    {
                        int liquidType = -1;
                        bool foundValid = false;

                        if (WorldGen.InWorld(i, j + 1))
                        {
                            if (Main.tile[i, j + 1].LiquidAmount >= 255)
                            {
                                liquidType = Main.tile[i, j + 1].LiquidType;
                                foundValid = true;
                            }
                        }

                        if (WorldGen.InWorld(i, j - 1))
                        {
                            if (Main.tile[i, j - 1].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i, j - 1].LiquidType;
                                foundValid = true;
                            }
                        }

                        if (WorldGen.InWorld(i - 1, j))
                        {
                            if (Main.tile[i - 1, j].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i - 1, j].LiquidType;
                                foundValid = true;
                            }
                        }

                        if (WorldGen.InWorld(i + 1, j))
                        {
                            if (Main.tile[i + 1, j].LiquidAmount > 0)
                            {
                                liquidType = Main.tile[i + 1, j].LiquidType;
                                foundValid = true;
                            }
                        }

                        if (foundValid && liquidColors.ContainsKey(liquidType))
                        {
                            Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16) - drawOffset, new Rectangle(0, 0, 16, 16), new VertexColors(liquidColors[liquidType]), Vector2.Zero, 1f, 0);
                            tileMaskPoints.Add(new Point(i, j));
                        }
                    }
                }
            }

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(tileMaskTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin();

            foreach (Point point in tileMaskPoints)
            {
                if (WorldGen.InWorld(point.X, point.Y, 1))
                    LiquidUtils.DrawSingleTile(point.X, point.Y, drawOffset);
            }

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(backWaterTargetFixed);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();

            Main.spriteBatch.Draw(Main.instance.backWaterTarget, Main.sceneBackgroundPos - Main.screenPosition, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(liquidOverlayTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
            Effect mask = AllAssets.Effect_ImageMask.Value;
            mask.Parameters["uMaskAdd"].SetValue(backWaterTargetFixed);
            mask.Parameters["uMaskSubtract"].SetValue(tileMaskTarget);
            mask.Parameters["inverse"].SetValue(false);
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
