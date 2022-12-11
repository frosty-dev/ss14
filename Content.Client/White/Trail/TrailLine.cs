using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Sandboxing;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail;

public interface ITrailLine : ITimedLine, IAttachedLine, IDynamicLine<ITrailSettings>, IDrawableLine<TrailLineDrawData> { }

public interface ITrailLineManager<out TTrailLine> : IDynamicLineManager<TTrailLine, ITrailSettings> where TTrailLine : ITrailLine { }

public sealed class TrailLineManager<TTrailLine> : ITrailLineManager<ITrailLine>
    where TTrailLine : class, ITrailLine, new()
{
    private readonly LinkedList<ITrailLine> _lines = new();

    private static readonly ISandboxHelper SandboxHelper = IoCManager.Resolve<ISandboxHelper>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ITrailLine> GetLines() => _lines;

    public ITrailLine Create(ITrailSettings settings, MapId mapId)
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
    private Vector2 _lastCreationPos;

    [ViewVariables]
    private float _lifetime;
    [ViewVariables]
    private Vector2? _virtualSegmentPos = null;

    [ViewVariables]
    public MapId MapId { get; set; }
    [ViewVariables]
    public bool Attached { get; set; }
    [ViewVariables]
    public ITrailSettings Settings { get; set; } = TrailSettings.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSegments() => _segments.Count > 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLifetime(float time) => _lifetime += time;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetLifetime() => _lifetime = 0f;

    public void TryCreateSegment(TransformComponent xform)
    {
        if (!Attached)
            return;

        if (xform.MapID != MapId)
            return;
        var posRot = xform.GetWorldPositionRotation();
        if (posRot.WorldPosition == Vector2.Zero)
            return;
        var pos = posRot.WorldPosition + posRot.WorldRotation.RotateVec(Settings.CreationOffset);

        _lastCreationPos = pos;

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

        var baseWidth = Settings.Width;
        var segmentLifetime = Settings.Lifetime;

        var curNode = _segments.Last;
        while (curNode != null)
        {
            var curSegment = curNode.Value;

            var prevPos = curNode.Next?.Value.Position ?? _lastCreationPos;
            var curPos = curSegment.Position;
            var angle = (curPos - prevPos).ToWorldAngle();
            var rotatedOffset = angle.ToVec() * baseWidth;

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
            var angle = (lastPos.Value - _lastCreationPos).ToWorldAngle();
            var rotatedOffset = angle.ToVec() * Settings.Width;

            yield return new(_lastCreationPos - rotatedOffset, _lastCreationPos + rotatedOffset, angle, 1f);
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

