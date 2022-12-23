using Content.Shared.White.Trail;
using Robust.Shared.GameStates;

namespace Content.Server.White.Trail;

public sealed class TrailSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrailComponent, ComponentGetState>(OnGetState);
    }
    private void OnGetState(EntityUid uid, TrailComponent component, ref ComponentGetState args)
    {
        var settings = new TrailSettings();
        TrailSettings.Inject(settings, component);
        args.State = new TrailComponentState(settings);
    }
}
