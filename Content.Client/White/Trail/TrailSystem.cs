using Content.Client.White.Trail.Line.Manager;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.White.Trail;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ITrailLineManager _lineManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        IoCManager.Resolve<IOverlayManager>().AddOverlay(
            new TrailOverlay(
                IoCManager.Resolve<IPrototypeManager>(),
                IoCManager.Resolve<IResourceCache>(),
                _lineManager
                ));

        SubscribeLocalEvent<TrailComponent, MoveEvent>(OnTrailMove);
        SubscribeLocalEvent<TrailComponent, ComponentRemove>(OnTrailRemove);
        SubscribeLocalEvent<TrailComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, TrailComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not TrailComponentState state)
            return;
        TrailSettings.Inject(component, state.Settings);
    }

    private void OnTrailRemove(EntityUid uid, TrailComponent comp, ComponentRemove args)
    {
        _lineManager.Detach(comp);
    }

    private void OnTrailMove(EntityUid uid, TrailComponent comp, ref MoveEvent args)
    {
        if (comp.СreationMethod != SegmentCreationMethod.OnMove || _gameTiming.InPrediction)
            return;

        TryCreateSegment(comp, args.Component);
    }

    private void TryCreateSegment(TrailComponent comp, TransformComponent xform)
    {
        if (xform.MapID == MapId.Nullspace)
            return;

        comp.TrailLine ??= _lineManager.CreateTrail(comp, xform.MapID);
        comp.TrailLine.TryCreateSegment(xform.GetWorldPositionRotation(), xform.MapID);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _lineManager.Update(frameTime);

        foreach (var (comp, xform) in EntityQuery<TrailComponent, TransformComponent>())
            if (comp.СreationMethod == SegmentCreationMethod.OnFrameUpdate)
                TryCreateSegment(comp, xform);
    }
}
