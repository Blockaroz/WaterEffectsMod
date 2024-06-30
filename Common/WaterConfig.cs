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

    public static bool ReflectionsEnabled => Instance.reflectionBlockDepth > 1;

    public static bool ScreenVibranceEnabled => Instance.vibrance > 0f;

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(0f)]
    public float vibrance;        
    
    [DefaultValue(true)]
    public bool movementFizz;    
    
    [DefaultValue(true)]
    public bool ambientFizz;

    [DefaultValue(32)]
    public int reflectionBlockDepth;

    [DefaultValue(true)]
    public bool additionalAudioEffects;

    [DefaultValue(true)]
    public bool fixLiquidRendering;
}
