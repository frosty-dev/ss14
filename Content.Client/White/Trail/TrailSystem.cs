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
    [Dependency] private readonly ITrailLineManager<ITrailLine> _lineManager = default!;

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
        var srvSettings = state.Settings;

        component.Offset = srvSettings.Offset;
        component.СreationDistanceThresholdSquared = srvSettings.СreationDistanceThresholdSquared;
        component.СreationMethod = srvSettings.СreationMethod;
        component.Gravity = srvSettings.Gravity;
        component.MaxRandomWalk = srvSettings.MaxRandomWalk;
        component.Lifetime = srvSettings.Lifetime;
        component.TexurePath = srvSettings.TexurePath;
        component.ColorLifetimeStart = srvSettings.ColorLifetimeStart;
        component.ColorLifetimeEnd = srvSettings.ColorLifetimeEnd;
        component.ColorLifetimeDeltaLambdaOperations = srvSettings.ColorLifetimeDeltaLambdaOperations;
    }

    private void OnTrailRemove(EntityUid uid, TrailComponent comp, ComponentRemove args)
    {
        if (comp.Line != null)
        {
            comp.Line.Attached = false;
            comp.Line.Settings = comp.ToTrailSettings();
        }
    }

    private void OnTrailMove(EntityUid uid, TrailComponent comp, ref MoveEvent args)
    {
        if (comp.СreationMethod != SegmentCreationMethod.OnMove || _gameTiming.InPrediction)
            return;

        TryCreateSegment(comp, args.Component.MapPosition);
    }

    private void TryCreateSegment(TrailComponent comp, MapCoordinates coords)
    {
        if (coords.MapId == MapId.Nullspace)
            return;

        comp.Line ??= _lineManager.Create(comp, coords.MapId);
        comp.Line.TryCreateSegment(coords);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _lineManager.Update(frameTime);

        foreach (var (comp, xform) in EntityQuery<TrailComponent, TransformComponent>())
            if (comp.СreationMethod == SegmentCreationMethod.OnFrameUpdate)
                TryCreateSegment(comp, xform.MapPosition);
    }
}
