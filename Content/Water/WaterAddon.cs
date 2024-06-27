using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Water;

public class WaterAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Water;

    public override void Update()
    {
        if (!Filters.Scene["WaterEffects:Reflections"].Active && WaterConfig.ReflectionsEnabled)
        {
            Filters.Scene.Activate("WaterEffects:Reflections", default);
            Filters.Scene.Deactivate("WaterDistortion");
        }

        if (Filters.Scene["WaterEffects:Reflections"].Active && !WaterConfig.ReflectionsEnabled)
            Filters.Scene.Deactivate("WaterEffects:Reflections", default);

        if (LiquidRenderingSystem.targetsReady)
        {
            if (LiquidRenderingSystem.liquidTargets.TryGetValue(LiquidType, out var target))
            {
                Filters.Scene["WaterDistortion"].GetShader()
                    .UseImageScale(new Vector2(0.25f))
                    .UseImage(AllAssets.Texture_Noise[2].Value)
                    .UseImage(target, 2, SamplerState.PointClamp)
                    .UseTargetPosition(Vector2.Zero);
            }
        }
    }
}
