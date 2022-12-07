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
        args.State = new TrailComponentState(component.Settings);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var comp in EntityManager.EntityQuery<TrailComponent>())
        {
            if (comp.MakeDirtyKostil)
            {
                Dirty(comp);
                comp.MakeDirtyKostil = false;
            }
        }
    }
}
