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
    private readonly LinkedList<TrailSegment> _segments = new();
    [ViewVariables]
    private Vector2 _lastCreationPos;

    private readonly TrailSegmentHead _vPointHead;
    private readonly TrailSegmentExtHead _vPointExtHead;
    private readonly TrailSegmentExtFirst _vPointExtFirst;

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
    public bool HasSegments() => _segments.Count > 0;
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
                TrailSegment point = new(_curLifetime + Settings.Lifetime)
                {
                    Position = vPos,
                };

                _segments.AddLast(point);
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
        var lifetime = Settings.Lifetime;

        foreach (var item in _segments)
        {
            var offset = gravity;
            
            if (maxRandomWalk != Vector2.Zero && item.Gradient != Vector2.Zero)
            {
                //var effectiveRandomWalk = maxRandomWalk * (1 - item.GetLifetimeRemaining(_curLifetime) / Settings.Lifetime);
                var gradientNorm = item.Gradient.Normalized;
                offset += gradientNorm * maxRandomWalk.Y * Random.NextFloat(-1.0f, 1.0f);
                offset += gradientNorm.Rotated90DegreesAnticlockwiseWorld * maxRandomWalk.X * Random.NextFloat(-1.0f, 1.0f);
            }
            

            item.Position += offset;
        }
        if (_virtualSegmentPos.HasValue)
            _virtualSegmentPos = _virtualSegmentPos.Value + gravity;
    }

    public void RemoveExpiredSegments()
    {
        while (_segments.First?.Value.GetLifetimeRemaining(_curLifetime) <= 0)
            _segments.RemoveFirst();
    }

    public void Render(DrawingHandleWorld handle, Texture? texture)
    {
        if (_segments.Last == null)
            return;

        RenderDebug(handle);
    }

    private IEnumerable<SplinePointData> CalculateSpline()
    {
        const float pointsPerUnit = 4f;

        if (_segments.Last == null)
            yield break;

        var pointsArray = _segments.AsEnumerable<ITrailSegment>()
            .Prepend(_vPointExtFirst)
            .Append(_vPointHead)
            .Append(_vPointExtHead)
            .Reverse()
            .ToArray();

        for (var i = 1; i < pointsArray.Length - 2; i++)
        {
            ITrailSegment p0 = pointsArray[i - 1], p1 = pointsArray[i], p2 = pointsArray[i + 1], p3 = pointsArray[i + 2];
            Vector2 p0v = p0.Position, p1v = p1.Position, p2v = p2.Position, p3v = p3.Position;

            var stepAmount = 1 / (p2v - p1v).Length / pointsPerUnit;

            Vector2? prevPosition = null;
            for (var t = 0f; t < 1f; t += stepAmount)
            {
                //можно смешной матан закешировать с точностью до сотой но пох
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

                var curPosition = new Vector2(
                    0.5f * (p0v.X * q0p + p1v.X * q1p + p2v.X * q2p + p3v.X * q3p),
                    0.5f * (p0v.Y * q0p + p1v.Y * q1p + p2v.Y * q2p + p3v.Y * q3p)
                    );

                var curGradient = new Vector2(
                    0.5f * (p0v.X * q0g + p1v.X * q1g + p2v.X * q2g + p3v.X * q3g),
                    0.5f * (p0v.Y * q0g + p1v.Y * q1g + p2v.Y * q2g + p3v.Y * q3g)
                    );

                if (t == 0f)
                    p1.Gradient = curGradient; //генераторовый импостер в реальной жизни о_0 ඞඞඞඞඞඞ

                var curLifetime = p1.GetLifetimeRemaining(_curLifetime);
                var lifetimeDiff = p2.GetLifetimeRemaining(_curLifetime) - curLifetime;

                yield return new SplinePointData(
                    curPosition,
                    curGradient,
                    (curPosition - prevPosition ?? pointsArray[0].Position).Length,
                    curLifetime + lifetimeDiff * t
                    );

                prevPosition = curPosition;
            }
        }
    }

    private void RenderDebug(DrawingHandleWorld handle)
    {
        var points = _segments.AsEnumerable<ITrailSegment>()
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

        var data = CalculateSpline();
        Vector2? prevPos = null;
        foreach (var item in data)
        {
            if (prevPos.HasValue)
                handle.DrawLine(item.Position, prevPos.Value, Color.Red);
            handle.DrawLine(item.Position, item.Position + item.Gradient, Color.White);
            handle.DrawCircle(item.Position, 0.05f, new Color(0, 255, 0, 255));
            prevPos = item.Position;
        }
    }

    private interface ITrailSegment
    {
        Vector2 Position { get; }
        Vector2 Gradient { get; set; }
        float GetLifetimeRemaining(float curLifetime);
    }

    private sealed class TrailSegmentHead : ITrailSegment
    {
        private readonly TrailLineCatmullRom _line;

        public TrailSegmentHead(TrailLineCatmullRom line)
        {
            _line = line;
        }

        public Vector2 Position => _line._lastCreationPos;
        public Vector2 Gradient { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _line.Settings.Lifetime;
    }

    private sealed class TrailSegmentExtHead : ITrailSegment
    {
        private readonly TrailLineCatmullRom _line;

        public TrailSegmentExtHead(TrailLineCatmullRom line)
        {
            _line = line;
        }

        public Vector2 Position
        {
            get
            {
                var last = _line._segments.Last?.Value?.Position;
                if (last == null)
                    throw new InvalidOperationException();
                return _line._lastCreationPos + (_line._lastCreationPos - last.Value).Normalized * 0.3f;
            }
        }
        public Vector2 Gradient { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _line.Settings.Lifetime;
    }

    private sealed class TrailSegmentExtFirst : ITrailSegment
    {
        private readonly TrailLineCatmullRom _line;

        public TrailSegmentExtFirst(TrailLineCatmullRom line)
        {
            _line = line;
        }
        public Vector2 Position
        {
            get
            {
                var first = _line._segments.First?.Value?.Position;
                var second = _line._segments.First?.Next?.Value?.Position ?? _line._lastCreationPos;
                if (first == null)
                    throw new InvalidOperationException();
                return first.Value - (first.Value - second).Normalized * 0.3f;
            }
        }
        public Vector2 Gradient { get; set; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => 0f;
    }

    private sealed class TrailSegment : ITrailSegment
    {
        private readonly float _existTil;

        public TrailSegment(float existTil)
        {
            _existTil = existTil;
        }
        public Vector2 Position { get; set; }
        public Vector2 Gradient { get; set; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetLifetimeRemaining(float curLifetime) => _existTil - curLifetime;
    }

    private struct SplinePointData
    {
        public readonly Vector2 Position;
        public readonly Vector2 Gradient;
        public readonly float Length;
        public readonly float Lifetime;

        public SplinePointData(Vector2 position, Vector2 gradient, float length, float lifetime)
        {
            Position = position;
            Gradient = gradient;
            Length = length;
            Lifetime = lifetime;
        }
    }
}
