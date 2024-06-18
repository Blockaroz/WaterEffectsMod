using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using WaterEffectsMod.Content;

namespace WaterEffectsMod;

public static class AllAssets
{
    public static void Load()
    {
        string assetPath = $"{nameof(WaterEffectsMod)}/Assets";
        BlackTileSlope = ModContent.Request<Texture2D>($"{assetPath}/Textures/BlackTileSlope", AssetRequestMode.ImmediateLoad);
        Noise = [
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/LavaNoise_0", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/LavaNoise_1", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/WaterNoise_0", AssetRequestMode.ImmediateLoad) ];

        BlankScreenEffect = ModContent.Request<Effect>($"{assetPath}/Effects/Screen", AssetRequestMode.ImmediateLoad);
        ColorCutoutEffect = ModContent.Request<Effect>($"{assetPath}/Effects/ColorCutout", AssetRequestMode.ImmediateLoad);
       
        ReflectionMapEffect = ModContent.Request<Effect>($"{assetPath}/Effects/ReflectionMapEffect", AssetRequestMode.ImmediateLoad);
        ReflectionEffect = [
            ModContent.Request<Effect>($"{assetPath}/Effects/Reflections/MirrorEffect", AssetRequestMode.ImmediateLoad)
            ];

        ScreenTarget.Load();
    }

    public static Asset<Texture2D> BlackTileSlope;
    public static Asset<Texture2D>[] Noise;

    public static Asset<Effect> BlankScreenEffect;
    public static Asset<Effect> ColorCutoutEffect;

    public static Asset<Effect> ReflectionMapEffect;
    public static Asset<Effect>[] ReflectionEffect;

    public static Asset<Effect> LavaCausticEffect;
}
