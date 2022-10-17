using System.Linq;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared.Clothing.Components;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Clothing.Overlays;

public sealed class HelmetOverlay : Overlay
{
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly ShaderInstance _shader;
    [Dependency] private readonly IGameTiming _timing = default!;

    private IRenderTexture? _buffer;
    private TimeSpan _lastDraw = TimeSpan.Zero;

    private HelmetOverlayState? _state;

    public HelmetOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _prototype.Index<ShaderPrototype>("Crt2").Instance().Duplicate();
    }

    public override bool RequestScreenTexture { get; } = true;
    public override OverlaySpace Space { get; } = OverlaySpace.ScreenSpace | OverlaySpace.WorldSpace;

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();

        _buffer?.Dispose();
    }

    public void SetState(HelmetOverlayState state)
    {
        _state = state;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.DrawingHandle is not DrawingHandleScreen handle)
            return;

        if (_state is null || ScreenTexture is null)
            return;

        _lastDraw = _timing.CurTime;
        var font = _cache.Exo2Stack(size: 32);

        _buffer?.Dispose();
        _buffer = _clyde.CreateRenderTarget(new Vector2i(ScreenTexture.Width, ScreenTexture.Height),
            RenderTargetColorFormat.Rgba8Srgb);

        const int margin = 50;

        handle.RenderInRenderTarget(_buffer, () =>
        {
            var bounds = new UIBox2(0, 0, ScreenTexture.Width, ScreenTexture.Height);

            handle.DrawTextureRect(ScreenTexture, bounds);

            // Overlay
            if (_state.Texture is { } texture)
                handle.DrawTextureRect(_cache.GetTexture(texture), bounds);

            if (!_state.HasHud)
                goto skipShader;

            var topLeft = new Vector2(bounds.TopLeft.X + 50, bounds.TopLeft.Y + 100);

            if (_state.GravitySensor && _state.HasGravity is { } hasGravity)
            {
                var text = Loc.GetString(hasGravity
                    ? "helmet-overlay-artificial-gravity"
                    : "helmet-overlay-zero-gravity");
                var color = hasGravity ? Color.Green : Color.Yellow;
                var box = DrawString(handle, font, topLeft, text, color);

                topLeft.X = box.Right + margin;
            }

            var bottomLeft = new Vector2(bounds.BottomLeft.X + 50, bounds.BottomLeft.Y - 100);

            if (_state.BatterySensor)
            {
                var maxVal = _state.BatteryMaxCharge!.Value;
                var val = _state.BatteryCharge!.Value;
                var fillColor = Color.White;

                if (val / maxVal < 0.2)
                    fillColor = _lastDraw.Seconds % 2 == 0 ? Color.Red : Color.White;

                var box = DrawProgressBar(handle, new ProgressBarParams
                {
                    Width = 200,
                    Text = Loc.GetString("helmet-overlay-battery"),
                    Font = font,
                    MaxValue = maxVal,
                    Value = val,
                    Start = bottomLeft,
                    Color = fillColor
                });

                bottomLeft.Y = box.Top - margin;
            }

            if (_state.InternalsSensor)
            {
                var internalsString = _state.InternalsPressure == null
                    ? Loc.GetString("helmet-overlay-not-available")
                    : $"{Math.Round(_state.InternalsPressure!.Value, 2)} {Loc.GetString("units-k-pascal")}";

                var color = Color.White;

                if (_state.InternalsPressure is not null && _state.InternalsPressure < 80.0f)
                    color = _lastDraw.Seconds % 2 == 0 ? Color.Red : Color.White;

                var box = DrawString(handle, font, bottomLeft,
                    $"{Loc.GetString("helmet-overlay-internals", ("pressure", internalsString))}", color);

                bottomLeft.Y = box.Top - margin;
            }

            if (_state.Pressure is { } pressure)
            {
                var box = DrawString(handle, font, bottomLeft,
                    Loc.GetString("helmet-overlay-pressure", ("pressure", Math.Round(pressure, 2))), Color.White);

                bottomLeft.Y = box.Top - margin;
            }

            if (_state.RadiationSensor)
            {
                DrawString(handle, font, bottomLeft,
                    Loc.GetString("helmet-overlay-radiation", ("radiation", _state.Radiation!.Value)), Color.White);
            }

            _shader.SetParameter("SCREEN_TEXTURE", _buffer.Texture);
            _shader.SetParameter("bloom_type", 1);
            _shader.SetParameter("mask_type", 1);
            _shader.SetParameter("res", new Vector2(ScreenTexture.Width, ScreenTexture.Height));
            handle.UseShader(_shader);
            handle.DrawRect(bounds, Color.White);

            skipShader:
            handle.UseShader(null);
        });


        handle.DrawTextureRect(_buffer.Texture, args.ViewportBounds, Color.White);
    }

    private static UIBox2 DrawProgressBar(DrawingHandleScreen handle, ProgressBarParams barParams)
    {
        const int margin = 20;

        var start = new Vector2(barParams.Start.X, barParams.Start.Y);

        // Draw text
        var textBounds = DrawString(handle, barParams.Font, start, barParams.Text, barParams.Color);

        // Draw bar rectangle
        var topLeftRect = new Vector2(textBounds.TopRight.X + margin, textBounds.TopRight.Y);
        var rightBottomRect = new Vector2(topLeftRect.X + barParams.Width, textBounds.BottomRight.Y);
        handle.DrawRect(new UIBox2(topLeftRect, rightBottomRect), barParams.Color, false);

        // Draw filling rectangle
        var rightBottomFillRect =
            new Vector2(barParams.Value / barParams.MaxValue * barParams.Width + topLeftRect.X,
                rightBottomRect.Y);
        handle.DrawRect(new UIBox2(new Vector2(topLeftRect.X, topLeftRect.Y + 1), rightBottomFillRect),
            barParams.Color);

        return new UIBox2(textBounds.TopLeft, rightBottomRect);
    }

    private static UIBox2 DrawString(DrawingHandleScreen handle, Font font, Vector2 pos, string str, Color color)
    {
        var textSizes = handle.DrawString(font, pos, str, color);
        var textHeight = str.EnumerateRunes().Select(r => font.GetCharMetrics(r, 1f)!.Value.Height).Max();

        return new UIBox2(new Vector2(pos.X, pos.Y + 10), new Vector2(pos.X + textSizes.X, pos.Y + textHeight + 10));
    }

    private struct ProgressBarParams
    {
        public Color Color;
        public float Width;
        public Vector2 Start;
        public Font Font;
        public string Text;
        public float Value;
        public float MaxValue;
    }
}
