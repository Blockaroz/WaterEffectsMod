using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Water;

public class HoneyAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Honey;

    public override Color LiquidColor => Color.Yellow;

    public override bool AddToColorRendering => true;

    public override bool HasVisuals => false;

    public override bool HasAudio => false;
}
