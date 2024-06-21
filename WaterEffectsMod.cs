using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using WaterEffectsMod.Common;

namespace WaterEffectsMod;

public class WaterEffectsMod : Mod
{
    public WaterEffectsMod Instance;

    public override void Load()
    {
        Instance = this;

        AllAssets.Load();

        Filters.Scene["WaterEffects:Reflections"] = new Filter(new ScreenShaderData(AllAssets.Effect_Reflection[1], "ShaderPass"), EffectPriority.VeryHigh);
        Filters.Scene["WaterEffects:Vibrance"] = new Filter(new ScreenShaderData(AllAssets.Effect_ScreenVibrance, "ShaderPass"), EffectPriority.High);
    }
}
