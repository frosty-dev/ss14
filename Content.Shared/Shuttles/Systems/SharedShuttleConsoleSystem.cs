using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Content.Shared.Shuttles.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Systems;

public abstract class SharedShuttleConsoleSystem : EntitySystem
{
    [Dependency] protected readonly ActionBlockerSystem ActionBlockerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PilotComponent, UpdateCanMoveEvent>(HandleMovementBlock);
        SubscribeLocalEvent<PilotComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PilotComponent, ComponentShutdown>(HandlePilotShutdown);
        SubscribeLocalEvent<ShuttleConsoleComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<ShuttleConsoleComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, ShuttleConsoleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ShuttleConsoleComponentState state)
            return;

        component.Zoom = state.Zoom;
        OnStateUpdate(uid, component);
    }

    private void OnGetState(EntityUid uid, ShuttleConsoleComponent component, ref ComponentGetState args)
    {
        args.State = new ShuttleConsoleComponentState
        {
            Zoom = component.Zoom,
        };

        OnStateUpdate(uid, component);
    }

    protected virtual void OnStateUpdate(EntityUid uid, ShuttleConsoleComponent component) { }

    protected virtual void HandlePilotShutdown(EntityUid uid, PilotComponent component, ComponentShutdown args)
    {
        ActionBlockerSystem.UpdateCanMove(uid);
    }

    private void OnStartup(EntityUid uid, PilotComponent component, ComponentStartup args)
    {
        ActionBlockerSystem.UpdateCanMove(uid);
    }

    private void HandleMovementBlock(EntityUid uid, PilotComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        if (component.Console == null)
            return;
        args.Cancel();
    }

    [Serializable]
    [NetSerializable]
    protected sealed class ShuttleConsoleComponentState : ComponentState
    {
        public Vector2 Zoom;
    }

    [Serializable]
    [NetSerializable]
    protected sealed class PilotComponentState : ComponentState
    {
        public PilotComponentState(EntityUid? uid)
        {
            Console = uid;
        }

        public EntityUid? Console { get; }
    }
}
