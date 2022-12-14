using Content.Shared.MobState.Components;
using Content.Shared.MobState.EntitySystems;
using Robust.Shared.GameStates;
using Content.Shared.Administration.Logs;

namespace Content.Server.MobState;

public sealed partial class MobStateSystem : SharedMobStateSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateComponent, ComponentGetState>(OnMobGetState);
    }

    private void OnMobGetState(EntityUid uid, MobStateComponent component, ref ComponentGetState args)
    {
        args.State = new MobStateComponentState(component.CurrentThreshold);
    }
}
