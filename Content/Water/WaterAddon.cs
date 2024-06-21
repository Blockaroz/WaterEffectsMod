using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using WaterEffectsMod.Common.LiquidAddons;

namespace WaterEffectsMod.Content.Water;

public class WaterAddon : LiquidAddon
{
    public override int LiquidType => LiquidID.Water;

    public override Color LiquidColor => Color.Blue;
}
