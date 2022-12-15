using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Random;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail.Line;

public sealed class TrailLineCatmullRom : ITrailLine
{
    private static readonly IRobustRandom Random = IoCManager.Resolve<IRobustRandom>();

    [ViewVariables]
    private readonly LinkedList<SplinePoint> _splinePoints = new();
    [ViewVariables]
    private Vector2 _lastCreationPos;

    private readonly SplinePointHead _vPointHead;
    private readonly SplinePointExtHead _vPointExtHead;
    private readonly SplinePointExtFirst _vPointExtFirst;

    [ViewVariables]
    private float _curLifetime;
    [ViewVariables]
    private Vector2? _virtualSegmentPos = null;

    public TrailLineCatmullRom()
    {
        _vPointHead = new(this);
        _vPointExtHead = new(this);
        _vPointExtFirst = new(this);
    }

    [ViewVariables]
    public MapId MapId { get; set; }
    [ViewVariables]
    public bool Attached { get; set; }
    [ViewVariables]
    public ITrailSettings Settings { get; set; } = TrailSettings.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSegments() => _splinePoints.Count > 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLifetime(float time) => _curLifetime += time;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetLifetime() => _curLifetime = 0f;

    public void TryCreateSegment((Vector2 WorldPosition, Angle WorldRotation) worldPosRot, MapId mapId)
    {
        if (!Attached)
            return;

        if (mapId != MapId)
            return;

        if (worldPosRot.WorldPosition == Vector2.Zero)
            return;
        var pos = worldPosRot.WorldPosition + worldPosRot.WorldRotation.RotateVec(Settings.CreationOffset);

        _lastCreationPos = pos;

        if (_virtualSegmentPos.HasValue)
        {
            var vPos = _virtualSegmentPos.Value;
            if ((vPos - pos).LengthSquared > Settings.СreationDistanceThresholdSquared)
            {
                SplinePoint point = new(_curLifetime + Settings.Lifetime)
                {
                    Position = vPos,
                };

                _splinePoints.AddLast(point);
                _virtualSegmentPos = null;
            }
            return;
        }

        var lastPos = _splinePoints.Last?.Value.Position;
        if (!lastPos.HasValue || (lastPos.Value - pos).LengthSquared > Settings.СreationDistanceThresholdSquared)
            _virtualSegmentPos = pos;
    }

    public void UpdateSegments(float dt)
    {
        var gravity = Settings.Gravity;
        var maxRandomWalk = Settings.MaxRandomWalk;

        foreach (var item in _splinePoints)
        {
            var offset = gravity;
            /*
            if (maxRandomWalk != Vector2.Zero)
            {

                var drawData = item.DrawData.Value;
                var alignedWalk = drawData.AngleRight.RotateVec(maxRandomWalk);
                offset += new Vector2(alignedWalk.X * Random.NextFloat(-1.0f, 1.0f), alignedWalk.Y * Random.NextFloat(-1.0f, 1.0f)) * drawData.LifetimePercent;
            }
            */

            item.Position += offset;
        }
        /*
        if (_virtualSegmentPos.HasValue)
            _virtualSegmentPos = _virtualSegmentPos.Value + gravity;
        */


    }

    public void RemoveExpiredSegments()
    {
        while (_splinePoints.First?.Value.GetLifetimeRemaining(_curLifetime) <= 0)
            _splinePoints.RemoveFirst();
    }

    public void Render(DrawingHandleWorld handle, Texture? texture)
    {
        if (_splinePoints.Last == null)
            return;

        RenderDebug(handle);
    }

    private IEnumerable<SplinePointRenderData> GetSplinePointDrawData()
    {
        if (_splinePoints.Last == null)
            yield break;

        var pointsArray = _splinePoints.AsEnumerable<ISplinePoint>()
            .Prepend(_vPointExtFirst)
            .Append(_vPointHead)
            .Append(_vPointExtHead)
            .Reverse()
            .ToArray();

        const float step = 0.3f;

        Vector2? prevPosition = null;
        for (var tTotal = 2f; tTotal <= pointsArray.Length - 1; tTotal += step)
        {
            var i = (int) tTotal;
            var t = tTotal - i;

            var tt = t * t;
            var ttt = tt * t;

            var q0p = -ttt + 2.0f * tt - t;
            var q1p = 3.0f * ttt - 5.0f * tt + 2.0f;
            var q2p = -3.0f * ttt + 4.0f * tt + t;
            var q3p = ttt - tt;

            var q0g = -3.0f * tt + 4.0f * t - 1;
            var q1g = 9.0f * tt - 10.0f * t;
            var q2g = -9.0f * tt + 8.0f * t + 1.0f;
            var q3g = 3.0f * tt - 2.0f * t;

            ISplinePoint p0 = pointsArray[i - 2], p1 = pointsArray[i - 1], p2 = pointsArray[i], p3 = pointsArray[i + 1];
            Vector2 p0v = p0.Position, p1v = p1.Position, p2v = p2.Position, p3v = p3.Position;

            var curPosition = new Vector2(
                0.5f * (p0v.X * q0p + p1v.X * q1p + p2v.X * q2p + p3v.X * q3p),
                0.5f * (p0v.Y * q0p + p1v.Y * q1p + p2v.Y * q2p + p3v.Y * q3p)
                );

            var curGradient = new Vector2(
                0.5f * (p0v.X * q0g + p1v.X * q1g + p2v.X * q2g + p3v.X * q3g),
                0.5f * (p0v.Y * q0g + p1v.Y * q1g + p2v.Y * q2g + p3v.Y * q3g)
                );

            var curLifetime = p1.GetLifetimeRemaining(_curLifetime);
            var lifetimeDiff = p2.GetLifetimeRemaining(_curLifetime) - curLifetime;

            yield return new SplinePointRenderData(
                curPosition,
                curGradient,
                (curPosition - prevPosition ?? pointsArray[0].Position).Length,
                curLifetime + lifetimeDiff * t
                );

            prevPosition = curPosition;
        }
    }

    private void RenderDebug(DrawingHandleWorld handle)
    {
        var points = _splinePoints.AsEnumerable<ISplinePoint>()
            .Prepend(_vPointExtFirst)
            .Append(_vPointHead)
            .Append(_vPointExtHead)
            .ToArray();

        Vector2? prevPointPos = null;
        foreach (var item in points)
        {
            if (prevPointPos.HasValue)
                handle.DrawLine(item.Position, prevPointPos.Value, Color.Blue);
            prevPointPos = item.Position;
        }

        var data = GetSplinePointDrawData();
        Vector2? prevPos = null;
        foreach (var item in data)
        {
            if (prevPos.HasValue)
                handle.DrawLine(item.Position, prevPos.Value, Color.Red);
            handle.DrawCircle(item.Position, 0.05f, new Color(0, 255, 0, 255));
            prevPos = item.Position;
        }
    }

    private interface ISplinePoint
    {
        Vector2 Position { get; }
        float GetLifetimeRemaining(float curLifetime);
    }

    private sealed class SplinePointHead : ISplinePoint
    {
        private readonly TrailLineCatmullRom _line;

        public SplinePointHead(TrailLineCatmullRom line)
        {
            _line = line;
        }

        public Vector2 Position => _line._lastCreationPos;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _line.Settings.Lifetime;
    }

    private sealed class SplinePointExtHead : ISplinePoint
    {
        private readonly TrailLineCatmullRom _line;

        public SplinePointExtHead(TrailLineCatmullRom line)
        {
            _line = line;
        }

        public Vector2 Position
        {
            get
            {
                var last = _line._splinePoints.Last?.Value?.Position;
                if (last == null)
                    throw new InvalidOperationException();
                return _line._lastCreationPos + (_line._lastCreationPos - last.Value).Normalized * 0.3f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _line.Settings.Lifetime;
    }

    private sealed class SplinePointExtFirst : ISplinePoint
    {
        private readonly TrailLineCatmullRom _line;

        public SplinePointExtFirst(TrailLineCatmullRom line)
        {
            _line = line;
        }
        public Vector2 Position
        {
            get
            {
                var first = _line._splinePoints.First?.Value?.Position;
                var second = _line._splinePoints.First?.Next?.Value?.Position ?? _line._lastCreationPos;
                if (first == null)
                    throw new InvalidOperationException();
                return first.Value - (first.Value - second).Normalized * 0.3f;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => 0f;
    }

    private sealed class SplinePoint : ISplinePoint
    {
        private readonly float _existTil;

        public SplinePoint(float existTil)
        {
            _existTil = existTil;
        }

        public Vector2 Position { get; set; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _existTil - curLifetime;
    }

    private struct SplinePointRenderData
    {
        public readonly Vector2 Position;
        public readonly Vector2 Gradient;
        public readonly float Length;
        public readonly float Lifetime;

        public SplinePointRenderData(Vector2 position, Vector2 gradient, float length, float lifetime)
        {
            Position = position;
            Gradient = gradient;
            Length = length;
            Lifetime = lifetime;
        }
    }
}
