using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using WaterEffectsMod.Common;

namespace WaterEffectsMod.Content.Water;

public class WaterReflectionShaderData : ScreenShaderData
{
    private Asset<Effect>[] _shaders;

    public WaterReflectionShaderData(Asset<Effect>[] shaders, string passName) : base(shaders[0], passName)
    {
        _shaders = shaders;
        Main.OnRenderTargetsInitialized += InitTargets;
        Main.OnRenderTargetsReleased += ReleaseTargets;
        On_Main.CheckMonoliths += DrawTargets;
    }

    private float _clarity;
    public RenderTarget2D _overlayTarget;
    public RenderTarget2D _reflectionTargetSwap;
    public RenderTarget2D _reflectionTarget;
    private bool _targetsReady;

    private void InitTargets(int width, int height)
    {
        try
        {
            _overlayTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            _reflectionTargetSwap = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            _reflectionTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, mipMap: false, Main.instance.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            _targetsReady = true;
        }
        catch (Exception ex)
        {
            Lighting.Mode = Terraria.Graphics.Light.LightMode.Retro;
            Console.WriteLine("Failed to create water reflection render targets. " + ex);
        }
    }

    private void ReleaseTargets()
    {
        try
        {
            _overlayTarget?.Dispose();
            _reflectionTargetSwap?.Dispose();
            _reflectionTarget?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error disposing water reflection render targets. " + ex);
        }

        _overlayTarget = null;
        _reflectionTargetSwap = null;
        _reflectionTarget = null;

        _targetsReady = false;
    }

    private void DrawTargets(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        if (!_targetsReady || !LiquidRenderingSystem.targetsReady)
            return;

        Main.instance.GraphicsDevice.SetRenderTarget(_overlayTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
        LiquidUtils.ApplyMask_Image(LiquidRenderingSystem.liquidTargets[LiquidID.Water], null);
        Main.spriteBatch.Draw(LiquidRenderingSystem.liquidMapTarget, Vector2.Zero, LiquidRenderingSystem.liquidMapTarget.Frame(), Color.White, 0, Vector2.Zero, 1, 0, 0);
        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(_reflectionTargetSwap);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        LiquidUtils.GetAreaForDrawing(out int left, out int right, out int top, out int bottom);
        LiquidUtils.DrawSurfaceMap(left, right, top, bottom, WaterConfig.Instance.reflectionBlockDepth);
        
        Main.instance.GraphicsDevice.SetRenderTarget(_reflectionTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
        Main.spriteBatch.Draw(_reflectionTargetSwap, Vector2.Zero, LiquidRenderingSystem.liquidMapTarget.Frame(), Color.White, 0, Vector2.Zero, 1, 0, 0);
        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
    }

    public override void Update(GameTime gameTime)
    {
        float clarityTarget = 0.5f;

        if (Main.bloodMoon)
            clarityTarget *= 0.5f;

        if (Main.LocalPlayer.ZoneShimmer)
            clarityTarget = 0.6f;

        if (Main.LocalPlayer.ZoneJungle)
            clarityTarget = 0.5f;

        if (Main.LocalPlayer.ZoneWaterCandle)
            clarityTarget *= 0.65f;
        if (Main.LocalPlayer.ZonePeaceCandle)
            clarityTarget *= 1.35f;

        _clarity = MathF.Round(MathHelper.Lerp(_clarity, clarityTarget, 0.05f), 2);
    }

    public override void Apply()
    {
        base.Apply();
        if (!Main.drawToScreen && _targetsReady)
        {
            Vector2 offscreen = new Vector2(Main.offScreenRange);
            Vector2 zoomedResolution = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom;
            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            Vector2 screenCenter = Main.screenPosition + screenSize * (Vector2.One - Vector2.One / Main.GameViewMatrix.Zoom);

            Effect effect = _shaders[0].Value;
            effect.Parameters["uOpacity"]?.SetValue(CombinedOpacity);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uZoom"].SetValue(Main.GameViewMatrix.Zoom / (screenSize.X / screenSize.Y));
            effect.Parameters["uImageSize"].SetValue(screenSize * 2f);
            effect.Parameters["uImageCutout"].SetValue(_overlayTarget);
            effect.Parameters["uReflectionMap"].SetValue(_reflectionTarget);
            effect.Parameters["uDepth"].SetValue(WaterConfig.Instance.reflectionBlockDepth + 1);
            effect.Parameters["uClearness"].SetValue(_clarity);
            effect.Parameters["debug"].SetValue(false);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
