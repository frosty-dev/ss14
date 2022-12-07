using Content.Shared.White.Trail;

namespace Content.Server.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent
{
    public override TrailSettings Settings { get; set; } = TrailSettings.Default;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool MakeDirtyKostil { get; set; }
}
