using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public class LiquidAudioSystem : ModSystem
{
    public override void PostUpdateEverything()
    {
        if (!SoundEngine.IsAudioSupported)
            return;

        float intensity = 0f;

        bool underwater = Collision.DrownCollision(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, Main.LocalPlayer.gravDir);

        if (underwater)
            intensity = 0.8f;

        
    }
}
