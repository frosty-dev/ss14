using Content.Shared.Shuttles.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Systems;

public abstract class SharedRadarConsoleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadarConsoleComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<RadarConsoleComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, RadarConsoleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not RadarConsoleComponentState state)
            return;

        component.MaxRange = state.Range;
        component.Rotation = state.Rotation;
        OnStateUpdate(uid, component);
    }

    private void OnGetState(EntityUid uid, RadarConsoleComponent component, ref ComponentGetState args)
    {
        args.State = new RadarConsoleComponentState
        {
            Range = component.MaxRange,
            Rotation = component.Rotation
        };

        OnStateUpdate(uid, component);
    }

    protected virtual void OnStateUpdate(EntityUid uid, RadarConsoleComponent component) { }

    [Serializable]
    [NetSerializable]
    protected sealed class RadarConsoleComponentState : ComponentState
    {
        public float Range;
        public Angle Rotation;
    }
}
