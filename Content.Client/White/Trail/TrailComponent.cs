using Content.Client.White.Line.Manager;
using Content.Shared.White.Line;
using Content.Shared.White.Trail;

namespace Content.Client.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent
{
    [ViewVariables]
    public ITrailLine? Line { get; set; } = null;

    public override TrailLineType CreatedTrailType
    {
        get => base.CreatedTrailType;
        set
        {
            if (base.CreatedTrailType == value)
                return;
            base.CreatedTrailType = value;
            if (Line != null)
            {
                var detachedSettings = new TrailSettings();
                TrailSettings.Inject(detachedSettings, this);
                Line.Settings = detachedSettings;
                Line.Attached = false;
                Line = null;
            }
        }
    }
}
