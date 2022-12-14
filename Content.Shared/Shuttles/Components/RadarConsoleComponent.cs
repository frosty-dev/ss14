using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedRadarConsoleSystem))]
public sealed class RadarConsoleComponent : Component
{
    private float _maxRange = 256f;
    private Angle _rotation = Angle.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("maxRange")]
    public float MaxRange
    {
        get => _maxRange;
        set
        {
            _maxRange = value;
            Dirty();
        }
    }

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("rotation")]
    public double Rotation
    {
        get => _rotation.Degrees;
        set
        {
            _rotation = Angle.FromDegrees(value);
            Dirty();
        }
    }
}
