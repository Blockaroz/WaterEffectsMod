using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace WaterEffectsMod.Content;

public static class ScreenTarget
{
    public static void Load()
    {
        On_FilterManager.EndCapture += SetScreenTexture;
    }

    public static RenderTarget2D texture;

    private static void SetScreenTexture(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
    {
        texture = screenTarget2;

        orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
    }
}
