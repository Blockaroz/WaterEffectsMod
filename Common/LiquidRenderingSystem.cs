﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.GameContent.Shaders;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public class LiquidRenderingSystem : ModSystem
{
    private static bool FixRendering => WaterConfig.Instance.fixLiquidRendering;

    public static Dictionary<int, Color> liquidColors;

    public static Color GetAvailableMappingColor()
    {
        liquidColors ??= new Dictionary<int, Color>();

        Color newColor = new Color(Main.rand.Next(0, 256), Main.rand.Next(0, 256), Main.rand.Next(0, 256), 255);

        //if the stars align and you get 2 of the same, try again
        while (liquidColors.Any(n => n.Value == newColor))
            newColor = new Color(Main.rand.Next(0, 256), Main.rand.Next(0, 256), Main.rand.Next(0, 256), 255);

        return newColor;
    }

    public static void AddLiquidMappingColor(int type)
    {
        liquidColors ??= new Dictionary<int, Color>();
        liquidColors.Add(type, GetAvailableMappingColor());
    }

    public static Color GetLiquidMappingColor(int type) => liquidColors[type];

    public static bool TryGetLiquidMappingColor(int type, out Color value)
    {
        value = Color.Transparent;
        if (liquidColors.ContainsKey(type))
        {
            value = liquidColors[type];
            return true;
        }
        return false;
    }

    public override void Load()
    {
        if (!Main.dedServ)
        {
            On_Main.CheckMonoliths += DrawLiquids;
            Main.OnRenderTargetsInitialized += InitTargets;
            Main.OnRenderTargetsReleased += ReleaseTargets;
            IL_Main.DoDraw += ReplaceDrawTarget;
        }
    }

    private void ReplaceDrawTarget(ILContext il)
    {
        try
        {
            ILCursor c = new ILCursor(il);

            c.TryGotoNext(i => i.MatchLdsfld<Main>("waterTarget"));
            c.TryGotoNext(i => i.MatchCallvirt<SpriteBatch>("Draw"));
            c.Index++;
            ILLabel label = il.DefineLabel(c.Next);
            c.TryGotoPrev(i => i.MatchLdsfld<Main>("waterTarget"));
            c.Emit(OpCodes.Pop);
            c.EmitDelegate(DrawWater);
            c.Emit(OpCodes.Br, label);
        }
        catch
        {
            MonoModHooks.DumpIL(Mod, il);
            Mod.Logger.Error("Water target was unable to be replaced.");
        }
    }

    private void DrawWater()
    {
        if (FixRendering && targetsReady)
        {
            foreach (int id in liquidTargets.Keys)
                Main.spriteBatch.Draw(liquidTargets[id], Vector2.Zero, Color.White);

            Main.spriteBatch.Draw(plantTarget, Vector2.Zero, Color.White);
        }
        else
            Main.spriteBatch.Draw(Main.waterTarget, Main.sceneWaterPos - Main.screenPosition, Color.White);

        LiquidAddonSystem.DrawAddons();
    }

    public static RenderTarget2D liquidMapTargetNoCut;
    public static RenderTarget2D liquidMapTarget;
    public static RenderTarget2D tileMaskTarget;
    public static RenderTarget2D backWaterTargetPosFixed;
    public static RenderTarget2D plantTarget;
    public static Dictionary<int, RenderTarget2D> liquidTargets;

    private static float _waterAlpha;
    private static float _shimmerAlpha;
    public static bool targetsReady;

    private void InitTargets(int width, int height)
    {
        try
        {
            liquidMapTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            liquidMapTargetNoCut = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            tileMaskTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            backWaterTargetPosFixed = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            plantTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            liquidColors = new Dictionary<int, Color>();
            liquidTargets = new Dictionary<int, RenderTarget2D>();

            for (int i = 0; i < LiquidID.Count; i++)
            {
                liquidColors.Add(i, GetAvailableMappingColor());
                liquidTargets.Add(i, new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None));
            }

            targetsReady = true;
        }
        catch (Exception ex)
        {
            Lighting.Mode = Terraria.Graphics.Light.LightMode.Retro;
            Console.WriteLine("Failed to create liquid rendering render targets. " + ex);
            targetsReady = false;
        }
    }

    private void ReleaseTargets()
    {
        targetsReady = false;
        liquidTargets = new Dictionary<int, RenderTarget2D>();

        try
        {
            liquidMapTarget?.Dispose();
            liquidMapTargetNoCut?.Dispose();
            tileMaskTarget?.Dispose();
            backWaterTargetPosFixed?.Dispose();
            plantTarget?.Dispose();
            foreach (RenderTarget2D target in liquidTargets.Values)
                target.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error disposing liquid rendering render targets. " + ex);
        }

        liquidMapTarget = null;
        liquidMapTargetNoCut = null;
        tileMaskTarget = null;
        backWaterTargetPosFixed = null;
        plantTarget = null;
    }

    private void DrawLiquids(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        if (!Main.drawToScreen && !Main.gameMenu && targetsReady)
        {
            Vector2 drawOffset = Main.screenPosition;

            Main.instance.GraphicsDevice.SetRenderTarget(liquidMapTargetNoCut);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin();

            LiquidUtils.GetAreaForDrawing(out int left, out int right, out int top, out int bottom);

            HashSet<Point> tileMaskPoints = new HashSet<Point>();
            HashSet<Point> waterPlants = new HashSet<Point>();

            for (int i = left; i < right; i++)
            {
                for (int j = top; j < bottom; j++)
                {
                    if (!WorldGen.InWorld(i, j))
                        continue;

                    int startAmount = Main.tile[i, j].LiquidAmount;
                    int startType = Main.tile[i, j].LiquidType;
                    if (Main.tile[i, j].LiquidAmount > 0 && TryGetLiquidMappingColor(startType, out Color liqColor))
                    {
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
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16 + 2 * extend, size + liquidHeight), new VertexColors(liqColor), Vector2.Zero, 1f, 0);
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight - extend) - drawOffset, new Rectangle(0, 0, 16, size + liquidHeight + 2 * extend), new VertexColors(liqColor), Vector2.Zero, 1f, 0);
                                }
                                else
                                {
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16 - extend, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16 + 2 * extend, size + liquidHeight), new VertexColors(liqColor), Vector2.Zero, 1f, 0);
                                    Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16 + 16 - liquidHeight) - drawOffset, new Rectangle(0, 0, 16, size + liquidHeight + extend), new VertexColors(liqColor), Vector2.Zero, 1f, 0);
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

                        if (foundValid && TryGetLiquidMappingColor(liquidType, out Color liqColor2))
                        {
                            Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(i * 16, j * 16) - drawOffset, new Rectangle(0, 0, 16, 16), new VertexColors(liqColor2), Vector2.Zero, 1f, 0);
                            tileMaskPoints.Add(new Point(i, j));
                        }
                    }

                    if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.LilyPad)
                        waterPlants.Add(new Point(i, j));
                }
            }

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(plantTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();

            foreach (Point point in waterPlants)
                Main.DrawTileInWater(-drawOffset, point.X, point.Y);

            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(tileMaskTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.tileBatch.Begin();
            foreach (Point point in tileMaskPoints)
            {
                if (WorldGen.InWorld(point.X, point.Y, 1))
                    LiquidUtils.DrawSingleTile(point.X, point.Y, drawOffset);
            }

            Main.tileBatch.Draw(plantTarget, Vector2.Zero, new VertexColors(Color.White));

            Main.tileBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(backWaterTargetPosFixed);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();
            Main.spriteBatch.Draw(Main.instance.backWaterTarget, Main.sceneBackgroundPos - Main.screenPosition, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(liquidMapTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
            LiquidUtils.ApplyMask_Image(liquidMapTargetNoCut, tileMaskTarget);
            Main.spriteBatch.Draw(liquidMapTargetNoCut, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            foreach (int id in liquidTargets.Keys)
            {
                Main.instance.GraphicsDevice.SetRenderTarget(liquidTargets[id]);
                Main.instance.GraphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
                LiquidUtils.ApplyMask_ImageColor(liquidMapTarget, null, liquidMapTargetNoCut, liquidColors[id]);

                Main.spriteBatch.Draw(backWaterTargetPosFixed, Vector2.Zero, Color.White * GetAlphaForLiquid(id));
                Main.spriteBatch.End();
            }

            Main.instance.GraphicsDevice.SetRenderTarget(null);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);

            LiquidAddonSystem.DrawAddonTargets();
        }
    }

    public override void PostUpdateDusts()
    {
        _waterAlpha = 0.75f;
        _shimmerAlpha = 0.75f;
    }

    public static float GetAlphaForLiquid(int type)
    {
        switch (type)
        {
            case LiquidID.Water:
                return _waterAlpha;
            case LiquidID.Shimmer:
                return _shimmerAlpha;
            default:
                return 1f;
        }
    }
}
