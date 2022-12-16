using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Random;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail.Line;

public sealed class TrailLineContiniousStretch : ITrailLine
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
        RecalculateDrawData();

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

    private void RecalculateDrawData()
    {
        if (_segments.Last == null)
            return;

        var baseWidth = Settings.Scale.X;
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

    private IEnumerable<TrailLineDrawData> GetDrawData()
    {
        foreach (var item in _segments)
            if (item.DrawData.HasValue)
                yield return item.DrawData.Value;

        var lastPos = _segments.Last?.Value.Position;
        if (lastPos != null)
        {
            var angle = (lastPos.Value - _lastCreationPos).ToWorldAngle();
            var rotatedOffset = angle.ToVec() * Settings.Scale.X;

            yield return new(_lastCreationPos - rotatedOffset, _lastCreationPos + rotatedOffset, angle, 1f);
        }
    }

    public void Render(DrawingHandleWorld handle, Texture? texture)
    {
        var drawData = GetDrawData();
        if (!drawData.Any())
            return;

        var settings = Settings;

        if (texture != null)
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                var lambda = settings.ColorLifetimeDeltaLambda != null
                    ? settings.ColorLifetimeDeltaLambda(cur.LifetimePercent)
                    : cur.LifetimePercent;
                var color = Color.InterpolateBetween(settings.ColorLifetimeEnd, settings.ColorLifetimeStart, lambda);
                RenderTrailTexture(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2, texture, color);
                prev = cur;
            }
        }
        else
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                Color color;
                if (settings.ColorLifetimeDeltaLambda == null)
                    color = Color.InterpolateBetween(settings.ColorLifetimeEnd, settings.ColorLifetimeStart, cur.LifetimePercent);
                else
                    color = Color.InterpolateBetween(settings.ColorLifetimeEnd, settings.ColorLifetimeStart, settings.ColorLifetimeDeltaLambda(cur.LifetimePercent));
                RenderTrailColor(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2, color);
                prev = cur;
            }
        }

#if DEBUG
        if (false)
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                //var color = Color.InterpolateBetween(settings.ColorLifetimeMod, settings.ColorBase, cur.LifetimePercent);
                RenderTrailDebugBox(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2);
                //handle.DrawLine(cur.Point1, cur.Point1 + cur.AngleRight.RotateVec(Vector2.UnitX), Color.Red);
                prev = cur;
            }
        }
#endif
    }

    private static void RenderTrailTexture(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2, Texture tex, Color color)
    {
        var verts = new DrawVertexUV2D[] {
            new (from1, Vector2.Zero),
            new (from2, Vector2.UnitY),
            new (to2, Vector2.One),
            new (to1, Vector2.UnitX),
        };

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, tex, verts, color);
    }

    private static void RenderTrailColor(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2, Color color)
    {
        var verts = new Vector2[] {
            from1,
            from2,
            to2,
            to1,
        };

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color);
    }

    private static void RenderTrailDebugBox(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2)
    {
        handle.DrawLine(from1, from2, Color.Gray);
        handle.DrawLine(from1, to1, Color.Gray);
        handle.DrawLine(from2, to2, Color.Gray);
        handle.DrawLine(to1, to2, Color.Gray);
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
    private struct TrailLineDrawData
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
}
