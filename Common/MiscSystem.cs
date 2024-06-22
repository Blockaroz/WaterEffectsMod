using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public class MiscSystem : ModSystem
{
    public override void OnModLoad()
    {
        On_Main.CheckMonoliths += MiscDrawUpdate;
    }

    private void MiscDrawUpdate(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        if (!Filters.Scene["WaterEffects:Vibrance"].Active && WaterConfig.ScreenVibranceEnabled)
            Filters.Scene.Activate("WaterEffects:Vibrance", default);
        if (Filters.Scene["WaterEffects:Vibrance"].Active && !WaterConfig.ScreenVibranceEnabled)
            Filters.Scene.Deactivate("WaterEffects:Vibrance");

        Filters.Scene["WaterEffects:Vibrance"].GetShader().UseColor(Color.Lerp(Main.ColorOfTheSkies, Color.White, 0.5f)).UseIntensity(WaterConfig.Instance.vibrance);
    }
}
