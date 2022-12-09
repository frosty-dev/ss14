using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using TerraFX.Interop.Windows;

namespace Content.Client.White.Trail;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
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

        component.Settings = state.Settings;
        if(component.Line != null)
            component.Line.Settings = state.Settings;
    }

    private void OnTrailRemove(EntityUid uid, TrailComponent comp, ComponentRemove args)
    {
        if (comp.Line != null)
            comp.Line.Attached = false;
    }

    private void OnTrailMove(EntityUid uid, TrailComponent comp, ref MoveEvent args)
    {
        if (
            comp.Settings.СreationMethod != PointCreationMethod.OnMove
            || _gameTiming.InPrediction
            || args.NewPosition.InRange(EntityManager, args.OldPosition, comp.Settings.СreationDistanceThreshold)
        )
            return;

        TryCreateSegment(comp, args.Component.MapPosition);
    }

    private void TryCreateSegment(TrailComponent comp, MapCoordinates coords)
    {
        if (coords.MapId == MapId.Nullspace)
            return;

        comp.Line ??= _lineManager.Create(comp.Settings, coords.MapId);
        comp.Line.TryCreateSegment(coords);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _lineManager.Update(frameTime);

        foreach (var (comp, xform) in EntityQuery<TrailComponent, TransformComponent>())
            if (comp.Settings.СreationMethod == PointCreationMethod.OnFrameUpdate)
                TryCreateSegment(comp, xform.MapPosition);
    }
}
