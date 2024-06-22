using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Filters.Scene.Activate("WaterEffects:Reflections", default);

        if (Filters.Scene["WaterEffects:Reflections"].Active && !WaterConfig.ReflectionsEnabled)
            Filters.Scene.Deactivate("WaterEffects:Reflections", default);

        Filters.Scene["WaterDistortion"].GetShader().UseImageScale(Vector2.One * 0.5f).UseImage(AllAssets.Texture_Noise[2].Value, 2, SamplerState.PointWrap);
    }
}
