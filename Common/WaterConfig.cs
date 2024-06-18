using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WaterEffectsMod.Common;

public class WaterConfig : ModConfig
{
    public static WaterConfig Instance => ModContent.GetInstance<WaterConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool waterReflectionsEnabled;

    [DefaultValue(24)]
    public int waterReflectionBlockDepth;

    [DefaultValue(0f)]
    public float waterReflectionMultiplierOverride;
}
