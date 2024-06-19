using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace WaterEffectsMod;

public class WaterEffectsMod : Mod
{
    public WaterEffectsMod Instance;

    public override void Load()
    {
        Instance = this;

        AllAssets.Load();

        Filters.Scene["WaterEffects:Reflections"] = new Filter(new ScreenShaderData(AllAssets.ReflectionEffect[0], "ShaderPass"), EffectPriority.VeryHigh);
    }
}
