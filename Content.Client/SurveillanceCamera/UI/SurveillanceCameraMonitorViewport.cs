using Content.Client.Viewport;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.SurveillanceCamera.UI;

public sealed class SurveillanceCameraMonitorViewport : ScalingViewport
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private IRenderTexture? _buffer;
    private readonly ShaderInstance _grainShader;

    public SurveillanceCameraMonitorViewport()
    {
        IoCManager.InjectDependencies(this);

        _grainShader = _prototype.Index<ShaderPrototype>("Crt").Instance().Duplicate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _buffer?.Dispose();
    }

    protected override void Resized()
    {
        base.Resized();

        _buffer?.Dispose();
        _buffer = _clyde.CreateRenderTarget(PixelSize, RenderTargetColorFormat.Rgba8Srgb, default);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_buffer is null)
            return;

        handle.RenderInRenderTarget(_buffer, () =>
        {
            base.Draw(handle);
        }, Color.Transparent);

        if (_cfg.GetCVar(CCVars.Shaders))
        {
            _grainShader.SetParameter("SCREEN_TEXTURE", _buffer.Texture);
            _grainShader.SetParameter("noise_opacity", 0.1f);
            _grainShader.SetParameter("roll", false);
            _grainShader.SetParameter("roll_size", 0.0f);
            handle.UseShader(_grainShader);
        }

        handle.DrawTextureRect(_buffer.Texture, PixelSizeBox);
        handle.UseShader(null);
    }
}
