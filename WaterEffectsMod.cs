using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using WaterEffectsMod.Content.Water;

namespace WaterEffectsMod;

public class WaterEffectsMod : Mod
{
    public WaterEffectsMod Instance;

    public override void Load()
    {
        Instance = this;

        AllAssets.Load();

        Filters.Scene["WaterEffects:Reflections"] = new Filter(new WaterReflectionShaderData(AllAssets.Effect_Reflection, "ShaderPass"), EffectPriority.VeryHigh);
        Filters.Scene["WaterEffects:Vibrance"] = new Filter(new ScreenShaderData(AllAssets.Effect_ScreenVibrance, "ShaderPass"), EffectPriority.High);
    }

    public override void PostSetupContent()
    {
        Main.targetSet = false;
    }
}
