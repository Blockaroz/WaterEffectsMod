using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace WaterEffectsMod.Common;

public abstract class LiquidAddon : ILoadable
{
    public virtual int LiquidType { get; }

    public virtual Color LiquidColor => LiquidRenderingSystem.GetLiquidMappingColor(LiquidType);

    public virtual Mod Mod { get; private set; }

    public virtual bool HasVisuals => true;

    public virtual bool HasAudio => true;

    public virtual void OnLoad() { }

    public void Load(Mod mod)
    {
        Mod = mod;
        OnLoad();
        LiquidAddonSystem.liquidAddons ??= new List<LiquidAddon>();
        LiquidAddonSystem.liquidAddons.Add(this);
    }

    public virtual void Update() { }

    public virtual void UpdateAudio() { }

    public void Unload()
    {
    }

    public void InitTarget(int width, int height)
    {
        overlayTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
    }

    public void ReleaseTarget()
    {
        overlayTarget?.Dispose();
        overlayTarget = null;
    }

    public RenderTarget2D overlayTarget;

    public virtual void DrawTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

        LiquidUtils.ApplyMask_Image(LiquidRenderingSystem.liquidTargets[LiquidType], null);

        Main.spriteBatch.Draw(LiquidRenderingSystem.liquidMapTarget, Vector2.Zero, Color.White);

        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
    }

    public virtual void Draw() { }
}
