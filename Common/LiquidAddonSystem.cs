using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public class LiquidAddonSystem : ModSystem
{
    public static List<LiquidAddon> liquidAddons;

    public override void Load()
    {
        liquidAddons = new List<LiquidAddon>();
        Main.OnRenderTargetsInitialized += InitAddonTargets;
        Main.OnRenderTargetsReleased += ReleaseAddonTargets;
    }

    private void InitAddonTargets(int width, int height)
    {
        foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
            addon.InitTarget(width, height);
    }

    private void ReleaseAddonTargets()
    {
        foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
            addon.ReleaseTarget();
    }

    public static void DrawAddonTargets()
    {
        foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
            addon.DrawTarget();
    }

    public static void DrawAddons()
    {
        foreach (LiquidAddon addon in liquidAddons.Where(n => n.HasVisuals))
            addon.Draw();
    }

    public static void UpdateAddons()
    {
        foreach (LiquidAddon addon in liquidAddons)
            addon.Update();
    }

    public override void PostUpdateDusts()
    {
        UpdateAddons();
    }
}
