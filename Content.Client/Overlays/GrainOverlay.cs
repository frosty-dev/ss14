using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Client.UserInterface;


namespace Content.Client.Overlays;

public sealed class GrainOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IUserInterfaceManager _interface = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private ShaderInstance _shader;

    public GrainOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototype.Index<ShaderPrototype>("Grain").Instance().Duplicate();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null)
            return;

        var screenHandle = args.WorldHandle;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("strength", _interface.RootControl.UIScale * 15.0f);

        screenHandle.UseShader(_shader);
        screenHandle.DrawRect(args.WorldBounds, Color.White);
        screenHandle.UseShader(null);
    }
}
