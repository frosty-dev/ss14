using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarConsoleSystem : SharedRadarConsoleSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentStartup>(OnRadarStartup);
    }

    private void OnRadarStartup(EntityUid uid, RadarConsoleComponent component, ComponentStartup args)
    {
        OnStateUpdate(uid, component);
    }

    protected override void OnStateUpdate(EntityUid uid, RadarConsoleComponent component)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = onGrid ? xform.Coordinates : null;
        Angle? angle = onGrid ? Angle.FromDegrees(component.Rotation) : null;

        if (TryComp<IntrinsicUIComponent>(component.Owner, out var intrinsic))
        {
            foreach (var uiKey in intrinsic.UIs)
            {
                if (uiKey.Key?.Equals(RadarConsoleUiKey.Key) == true)
                {
                    coordinates = new EntityCoordinates(component.Owner, Vector2.Zero);
                    angle = Angle.Zero;
                    break;
                }
            }
        }

        var radarState = new RadarConsoleBoundInterfaceState(
            component.MaxRange,
            coordinates,
            angle,
            new List<DockingInterfaceState>());

        _uiSystem.GetUiOrNull(uid, RadarConsoleUiKey.Key)?.SetState(radarState);
    }
}
