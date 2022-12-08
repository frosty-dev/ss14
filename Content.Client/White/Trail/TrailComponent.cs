using Content.Shared.White.Trail;

namespace Content.Client.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent
{
    [ViewVariables]
    public ITrailLine? Line { get; set; } = null;
    [ViewVariables]
    public override TrailSettings Settings { get; set; } = TrailSettings.Default;
}
