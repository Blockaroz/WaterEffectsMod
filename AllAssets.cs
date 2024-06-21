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
        Texture_Noise = [
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/LavaNoise_0", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/LavaNoise_1", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/WaterNoise_0", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/WaterNoise_1", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Texture2D>($"{assetPath}/Textures/Noise/WaterNoise_2", AssetRequestMode.ImmediateLoad) ];

        Effect_BlankScreen = ModContent.Request<Effect>($"{assetPath}/Effects/Screen", AssetRequestMode.ImmediateLoad);
        Effect_ColorCutout = ModContent.Request<Effect>($"{assetPath}/Effects/ColorCutout", AssetRequestMode.ImmediateLoad);
        Effect_ImageMask = ModContent.Request<Effect>($"{assetPath}/Effects/ImageMask", AssetRequestMode.ImmediateLoad);

        Effect_ScreenVibrance = ModContent.Request<Effect>($"{assetPath}/Effects/ScreenVibrance", AssetRequestMode.ImmediateLoad);
       
        Effect_ReflectionMap = ModContent.Request<Effect>($"{assetPath}/Effects/ReflectionMapEffect", AssetRequestMode.ImmediateLoad);
        Effect_Reflection = [
            ModContent.Request<Effect>($"{assetPath}/Effects/Reflections/BasicMirror", AssetRequestMode.ImmediateLoad),
            ModContent.Request<Effect>($"{assetPath}/Effects/Reflections/ShinyWater", AssetRequestMode.ImmediateLoad)
            ];

        ScreenTarget.Load();
    }

    public static Asset<Texture2D>[] Texture_Noise;

    public static Asset<Effect> Effect_BlankScreen;
    public static Asset<Effect> Effect_ColorCutout;
    public static Asset<Effect> Effect_ImageMask;

    public static Asset<Effect> Effect_ScreenVibrance;

    public static Asset<Effect> Effect_ReflectionMap;
    public static Asset<Effect>[] Effect_Reflection;
    public static Asset<Effect> Effect_LavaOverlay;
}
