using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client.White.Trail;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public LinkedList<TrailData> DetachedTrails { get; } = new LinkedList<TrailData>();

    public override void Initialize()
    {
        base.Initialize();

        IoCManager.Resolve<IOverlayManager>().AddOverlay(
            new TrailOverlay(
                this,
                EntityManager,
                IoCManager.Resolve<IPrototypeManager>(),
                IoCManager.Resolve<IResourceCache>()
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
        if(component.Data != null)
            component.Data.Settings = state.Settings;
    }

    private void OnTrailRemove(EntityUid uid, TrailComponent comp, ComponentRemove args)
    {
        if(comp.Data != null)
            DetachedTrails.AddLast(comp.Data);
    }

    private void OnTrailMove(EntityUid uid, TrailComponent comp, ref MoveEvent args)
    {
        if (
            comp.Settings.СreationMethod != PointCreationMethod.OnMove
            || _gameTiming.InPrediction
            || args.NewPosition.InRange(EntityManager, args.OldPosition, comp.Settings.СreationDistanceThreshold)
            )
            return;

        TryAddSegment(comp, args.Component);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        foreach (var (comp, xform) in EntityQuery<TrailComponent, TransformComponent>())
        {
            if (comp.Settings.СreationMethod == PointCreationMethod.OnFrameUpdate)
                TryAddSegment(comp, xform);

            var data = comp.Data;
            if (data == null)
                continue;

            RemoveExpiredPoints(data.Segments, data.LifetimeAccumulator);
            UpdateTrailData(data, frameTime, xform.MapPosition);
            ProcessSegmentMovement(data);
        }

        var nextNode = DetachedTrails.First;
        while (nextNode != null)
        {
            var curNode = nextNode;
            nextNode = nextNode.Next;

            var data = curNode.Value;
            UpdateTrailData(data, frameTime);
            RemoveExpiredPoints(data.Segments, data.LifetimeAccumulator);
            if (data.Segments.Count > 0)
                ProcessSegmentMovement(data);
            else
                DetachedTrails.Remove(curNode);
        }
    }

    private void UpdateTrailData(TrailData data, float frameTime, MapCoordinates? parentCoords = null)
    {
        if (data.Segments.Last == null)
        {
            data.LifetimeAccumulator = 0;
            return;
        }
        data.LifetimeAccumulator += frameTime;

        if (parentCoords != null && parentCoords != MapCoordinates.Nullspace)
            data.LastParentCoords = parentCoords.Value;
        if (data.LastParentCoords.HasValue)
            data.CalculatedDrawData = EnumerateDrawData(data).ToArray();
    }

    private void RemoveExpiredPoints(LinkedList<TrailSegment> segment, float trailLifetime)
    {
        while (segment.First?.Value.ExistTil < trailLifetime)
            segment.RemoveFirst();
    }

    private void ProcessSegmentMovement(TrailData data)
    {
        var gravity = data.Settings.Gravity;
        var maxRandomWalk = data.Settings.MaxRandomWalk;

        foreach (var cur in data.CalculatedDrawData)
        {
            if (cur.Segment == null)
                continue;
            var curSegment = cur.Segment;

            var offset = gravity;
            if (maxRandomWalk != Vector2.Zero && cur.AngleRight != float.NaN)
            {
                var alignedWalk = cur.AngleRight.RotateVec(maxRandomWalk);
                offset += new Vector2(alignedWalk.X * _random.NextFloat(-1.0f, 1.0f), alignedWalk.Y * _random.NextFloat(-1.0f, 1.0f)) * cur.LifetimePercent;
            }

            curSegment.Coords = curSegment.Coords.Offset(offset);
        }
    }

    private void TryAddSegment(TrailComponent comp, TransformComponent xform)
    {
        if (xform.MapID == MapId.Nullspace)
            return;

        if (comp.Data == null)
            comp.Data = new(comp.Settings);

        var data = comp.Data;
        if (data.LastParentCoords.HasValue && data.LastParentCoords.Value.MapId != xform.MapID)
        {
            DetachedTrails.AddLast(data);
            comp.Data = new(comp.Settings);
        }

        var newPos = xform.MapPosition.Offset(data.Settings.Gravity * 0.01f);
        var segmentsList = data.Segments;

        if (segmentsList.Last == null)
        {
            segmentsList.AddLast(new TrailSegment(newPos, data.LifetimeAccumulator + data.Settings.Lifetime));
            return;
        }

        var coords = segmentsList.Last.Value.Coords;
        if (newPos.InRange(coords, comp.Settings.СreationDistanceThreshold))
            return;

        segmentsList.AddLast(new TrailSegment(newPos, data.LifetimeAccumulator + data.Settings.Lifetime));
    }

    private static IEnumerable<TrailSegmentDrawData> EnumerateDrawData(TrailData data)
    {
        if (data.Segments.Last == null || data.LastParentCoords == null)
            yield break;

        var parentCoords = data.LastParentCoords.Value;
        var mapId = parentCoords.MapId;

        var baseOffset = data.Settings.Offset;

        yield return ConstructDrawData(
            0f,
            baseOffset,
            parentCoords.Position,
            data.Segments.Last.Value.Coords.Position,
            true);

        var curNode = data.Segments.Last;
        while (curNode != null)
        {
            var curSegment = curNode.Value;
            if (curSegment.Coords.MapId == mapId)
                yield return ConstructDrawData(
                    (curSegment.ExistTil - data.LifetimeAccumulator) / data.Settings.Lifetime,
                    baseOffset,
                    curSegment.Coords.Position,
                    curNode.Next?.Value.Coords.Position ?? parentCoords.Position,
                    segment: curSegment
                    );
            curNode = curNode.Previous;
        }
    }

    private static TrailSegmentDrawData ConstructDrawData(
        float lifetimePercent,
        Vector2 offset,
        Vector2 curPos,
        Vector2 nextPos,
        bool flipAngle = false,
        TrailSegment? segment = null
        )
    {
        var angle = (nextPos - curPos).ToWorldAngle();
        if (flipAngle)
            angle = angle.Opposite();
        var rotatedOffset = angle.RotateVec(offset);
        return new TrailSegmentDrawData(curPos - rotatedOffset, curPos + rotatedOffset, angle, lifetimePercent, segment);
    }
}
