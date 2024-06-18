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

    public virtual Color LiquidColor { get; }

    public virtual Mod Mod { get; private set; }

    public virtual bool AddToColorRendering => true;

    public virtual void OnLoad() { }

    public void Load(Mod mod)
    {
        Mod = mod;
        OnLoad();
        LiquidAddonSystem.liquidAddons ??= new List<LiquidAddon>();
        LiquidAddonSystem.liquidAddons.Add(this);
        if (AddToColorRendering)
        {
            LiquidAddonSystem.liquidColors ??= new Dictionary<int, Color>();
            LiquidAddonSystem.liquidColors.Add(LiquidType, LiquidColor);
        }
    }

    public virtual void Update() { }

    public void Unload()
    {
    }

    public void CreateAndDrawTarget()
    {
        if (overlayTarget == null || Main.screenWidth != currentWidth || Main.screenHeight != currentHeight)
        {
            overlayTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            currentWidth = Main.screenWidth;
            currentHeight = Main.screenHeight;
            return;
        }

        DrawTarget();
    }

    public RenderTarget2D overlayTarget;

    public int currentWidth;
    public int currentHeight;

    public virtual void DrawTarget()
    {
        Main.instance.GraphicsDevice.SetRenderTarget(overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        Effect cutout = AllAssets.ColorCutoutEffect.Value;
        cutout.Parameters["uCutoutColor"].SetValue(LiquidColor.ToVector4());
        cutout.CurrentTechnique.Passes[0].Apply();

        Main.spriteBatch.Draw(LiquidAddonSystem.liquidOverlayTarget, Vector2.Zero, Color.White);

        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);
    }

    public virtual void Draw() { }
}
