using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Client.ResourceManagement;
using Robust.Shared.Map;
using Robust.Shared.Sandboxing;
using System.Linq;
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
        var tline = (TTrailLine)SandboxHelper.CreateInstance(typeof(TTrailLine));
        tline.Attached = true;
        tline.Settings = settings;
        tline.MapId = mapId;

        _lines.AddLast(tline);
        return tline;
    }

    public void Update(float dt)
    {
        foreach (var item in _lines)
        {
            if (!item.HasSegments())
            {
                item.ResetLifetime();
                continue;
            }

            item.AddLifetime(dt);
            item.RemoveExpiredSegments();
            item.UpdateSegments(dt);
            item.RecalculateDrawData();
        }
    }
}

public sealed class TrailLine : ITrailLine
{
    [ViewVariables]
    private readonly LinkedList<TrailLineSegment> _segments = new();
    [ViewVariables]
    private readonly TrailLineSegment _headSegment = new();

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

    public void TryCreateSegment(Vector2 pos)
    {
        if (!Attached)
            return;

        _headSegment.Position = pos;

        if (_virtualSegmentPos.HasValue)
        {
            if (_virtualSegmentPos != pos)
            {
                TrailLineSegment segment = new()
                {
                    Position = pos,
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
            /*
            if (maxRandomWalk != Vector2.Zero && cur.AngleRight != float.NaN)
            {
                var alignedWalk = cur.AngleRight.RotateVec(maxRandomWalk);
                offset += new Vector2(alignedWalk.X * _random.NextFloat(-1.0f, 1.0f), alignedWalk.Y * _random.NextFloat(-1.0f, 1.0f)) * cur.LifetimePercent;
            }
            */

            item.Position += offset;
        }
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

        var prev = _headSegment;
        foreach (var item in _segments.AsEnumerable().Reverse())
        {
            var curPos = item.Position;
            var angle = (curPos - prev.Position).ToWorldAngle();
            var rotatedOffset = angle.RotateVec(baseOffset);

            item.DrawData = new(
                curPos - rotatedOffset,
                curPos + rotatedOffset,
                angle,
                (item.ExistTil - _lifetime) / segmentLifetime
                );

            prev = item;
        }

        if (Attached)
        {
            var headPos = _headSegment.Position;
            var headAngle = _segments.Last.Value.DrawData.AngleRight;
            var headRotatedOffset = headAngle.RotateVec(baseOffset);

            _headSegment.DrawData = new(
                headPos - headRotatedOffset,
                headPos + headRotatedOffset,
                headAngle,
                1f
                );
        }
    }

    public IEnumerable<TrailLineDrawData> GetDrawData()
    {
        foreach (var item in _segments)
            yield return item.DrawData;
        if (Attached)
            yield return _headSegment.DrawData;
    }

    private class TrailLineSegment
    {
        [ViewVariables]
        public Vector2 Position { get; set; }
        [ViewVariables]
        public float ExistTil { get; init; }
        [ViewVariables]
        public TrailLineDrawData DrawData { get; set; }
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

