using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Sandboxing;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail;

public interface ITrailLine : ITimedLine, IAttachedLine, IDynamicLine<TrailSettings>, IDrawableLine<TrailLineDrawData> { }

public interface ITrailLineManager<out TTrailLine> : IDynamicLineManager<TTrailLine, TrailSettings> where TTrailLine : ITrailLine { }

public sealed class TrailLineManager<TTrailLine> : ITrailLineManager<ITrailLine>
    where TTrailLine : class, ITrailLine, new()
{
    private readonly LinkedList<ITrailLine> _lines = new();

    private static readonly ISandboxHelper SandboxHelper = IoCManager.Resolve<ISandboxHelper>();

    public IEnumerable<ITrailLine> GetLines() => _lines;

    public ITrailLine Create(TrailSettings settings, MapId mapId)
    {
        var tline = (TTrailLine) SandboxHelper.CreateInstance(typeof(TTrailLine));
        tline.Attached = true;
        tline.Settings = settings;
        tline.MapId = mapId;

        _lines.AddLast(tline);
        return tline;
    }

    public void Update(float dt)
    {
        var curNode = _lines.First;
        while (curNode != null)
        {
            var curLine = curNode.Value;
            curNode = curNode.Next;

            if (!curLine.HasSegments())
            {
                if (curLine.Attached)
                    curLine.ResetLifetime();
                else
                    _lines.Remove(curLine);
                continue;
            }

            curLine.AddLifetime(dt);
            curLine.RemoveExpiredSegments();
            curLine.UpdateSegments(dt);
            curLine.RecalculateDrawData();
        }
    }
}

public sealed class TrailLine : ITrailLine
{
    private static readonly IRobustRandom Random = IoCManager.Resolve<IRobustRandom>();

    [ViewVariables]
    private readonly LinkedList<TrailLineSegment> _segments = new();
    [ViewVariables]
    private Vector2 _lastHeadPos;

    [ViewVariables]
    private float _lifetime;
    [ViewVariables]
    private Vector2? _virtualSegmentPos = null;

    [ViewVariables]
    public MapId MapId { get; set; }
    [ViewVariables]
    public bool Attached { get; set; }
    [ViewVariables]
    public TrailSettings Settings { get; set; } = TrailSettings.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSegments() => _segments.Count > 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLifetime(float time) => _lifetime += time;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetLifetime() => _lifetime = 0f;

    public void TryCreateSegment(MapCoordinates coords)
    {
        if (!Attached)
            return;

        if (coords.MapId != MapId)
            return;

        var pos = coords.Position;
        if (pos == Vector2.Zero)
            return;

        _lastHeadPos = pos;

        if (_virtualSegmentPos.HasValue)
        {
            var vPos = _virtualSegmentPos.Value;
            if ((vPos - pos).LengthSquared > Settings.СreationDistanceThresholdSquared)
            {
                TrailLineSegment segment = new()
                {
                    Position = vPos,
                    ExistTil = _lifetime + Settings.Lifetime
                };

                _segments.AddLast(segment);
                _virtualSegmentPos = null;
            }
            return;
        }

        var lastPos = _segments.Last?.Value.Position;
        if (!lastPos.HasValue || (lastPos.Value - pos).LengthSquared > Settings.СreationDistanceThresholdSquared)
            _virtualSegmentPos = pos;
    }

    public void UpdateSegments(float dt)
    {
        var gravity = Settings.Gravity;
        var maxRandomWalk = Settings.MaxRandomWalk;

        foreach (var item in _segments)
        {
            var offset = gravity;

            if (maxRandomWalk != Vector2.Zero && item.DrawData.HasValue)
            {
                var drawData = item.DrawData.Value;
                var alignedWalk = drawData.AngleRight.RotateVec(maxRandomWalk);
                offset += new Vector2(alignedWalk.X * Random.NextFloat(-1.0f, 1.0f), alignedWalk.Y * Random.NextFloat(-1.0f, 1.0f)) * drawData.LifetimePercent;
            }

            item.Position += offset;
        }

        if (_virtualSegmentPos.HasValue)
            _virtualSegmentPos = _virtualSegmentPos.Value + gravity;
    }

    public void RemoveExpiredSegments()
    {
        while (_segments.First?.Value.ExistTil < _lifetime)
            _segments.RemoveFirst();
    }

    public void RecalculateDrawData()
    {
        if (_segments.Last == null)
            return;

        var baseOffset = Settings.Offset;
        var segmentLifetime = Settings.Lifetime;

        var curNode = _segments.Last;
        while (curNode != null)
        {
            var curSegment = curNode.Value;

            var prevPos = curNode.Next?.Value.Position ?? _lastHeadPos;
            var curPos = curSegment.Position;
            var angle = (curPos - prevPos).ToWorldAngle();
            var rotatedOffset = angle.RotateVec(baseOffset);

            curSegment.DrawData = new(
                curPos - rotatedOffset,
                curPos + rotatedOffset,
                angle,
                (curSegment.ExistTil - _lifetime) / segmentLifetime
                );

            curNode = curNode.Previous;
        }
    }

    public IEnumerable<TrailLineDrawData> GetDrawData()
    {
        foreach (var item in _segments)
            if (item.DrawData.HasValue)
                yield return item.DrawData.Value;

        var lastPos = _segments.Last?.Value.Position;
        if (lastPos != null)
        {
            var angle = (lastPos.Value - _lastHeadPos).ToWorldAngle();
            var rotatedOffset = angle.RotateVec(Settings.Offset);

            yield return new(_lastHeadPos - rotatedOffset, _lastHeadPos + rotatedOffset, angle, 1f);
        }
    }

    private sealed class TrailLineSegment
    {
        [ViewVariables]
        public Vector2 Position { get; set; }
        [ViewVariables]
        public float ExistTil { get; init; }
        [ViewVariables]
        public TrailLineDrawData? DrawData { get; set; } = null;
    }
}

public struct TrailLineDrawData
{
    public readonly Vector2 Point1;
    public readonly Vector2 Point2;
    public readonly Angle AngleRight;
    public readonly float LifetimePercent;

    public TrailLineDrawData(Vector2 point1, Vector2 point2, Angle angleRight, float lifetimePercent)
    {
        Point1 = point1;
        Point2 = point2;
        AngleRight = angleRight;
        LifetimePercent = lifetimePercent;
    }
}

