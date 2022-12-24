using Content.Client.White.Trail.SplineRenderer;
using Content.Shared.White.Spline;
using Content.Shared.White.Spline.Linear;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Random;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail.Line;

public sealed class TrailSpline : ITrailLine
{
    private static readonly IRobustRandom Random = IoCManager.Resolve<IRobustRandom>();

    [ViewVariables]
    private readonly LinkedList<TrailSplineSegment> _segments = new();
    [ViewVariables]
    private Vector2 _lastCreationPos;

    [ViewVariables]
    private float _curLifetime;
    [ViewVariables]
    private Vector2? _virtualSegmentPos;

    [ViewVariables]
    public MapId MapId { get; set; }
    [ViewVariables]
    public bool Attached { get; set; }
    [ViewVariables]
    public ITrailSettings Settings { get; set; } = TrailSettings.Default;
    [ViewVariables]
    public ISpline<Vector2> SplineIterator { get; set; } = new SplineLinear2D();
    [ViewVariables]
    public ISpline<Vector4> GradientIterator { get; set; } = new SplineLinear4D();
    [ViewVariables]
    public ITrailSplineRenderer Renderer { get; set; } = new TrailSplineRendererDebug();

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
                _segments.AddLast(new TrailSplineSegment() { Position = vPos, ExistTil = _curLifetime + Settings.Lifetime });
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

        if (_segments.Last != null)
        {
            var i = 0;
            var positions = new Vector2[_segments.Count + 1];
            positions[_segments.Count] = _lastCreationPos;

            var curNode = _segments.First;
            while (curNode != null)
            {
                var offset = gravity;
                var curValue = curNode.Value;
                if (maxRandomWalk != Vector2.Zero)
                {
                    positions[i] = curValue.Position;
                    if (curNode.Next != null)
                    {
                        positions[i + 1] = curNode.Next.Value.Position;
                        if (curNode.Next.Next != null)
                            positions[i + 2] = curNode.Next.Next.Value.Position;
                    }

                    var effectiveRandomWalk = maxRandomWalk * (curValue.ExistTil - _curLifetime) / lifetime;
                    var gradientNorm = -SplineIterator.SampleVelocity(positions, (float)i).Normalized;
                    offset += gradientNorm * effectiveRandomWalk.Y * Random.NextFloat(-1.0f, 1.0f);
                    offset += gradientNorm.Rotated90DegreesAnticlockwiseWorld * effectiveRandomWalk.X * Random.NextFloat(-1.0f, 1.0f);
                }
                curValue.Position += offset;
                i++;
                curNode = curNode.Next;
            }
        }

        if (_virtualSegmentPos.HasValue)
            _virtualSegmentPos = _virtualSegmentPos.Value + gravity;

        if (!Attached)
            _lastCreationPos += gravity;
    }

    public void RemoveExpiredSegments()
    {
        while (_segments.First?.Value.ExistTil < _curLifetime)
            _segments.RemoveFirst();
    }

    public void Render(DrawingHandleWorld handle, Texture? texture)
    {
        if (_segments.Last == null)
            return;

        var arrSize = _segments.Count + 1;
        var paPositions = new Vector2[arrSize];
        var paLifetimes = new float[arrSize];
        paPositions[0] = _lastCreationPos;
        paLifetimes[0] = 1f;

        var reversedIndexedSegments = _segments.Reverse().Select((x, i) => (x, i + 1));
        foreach (var (x, i) in reversedIndexedSegments)
        {
            paPositions[i] = x.Position;
            paLifetimes[i] = (x.ExistTil - _curLifetime) / Settings.Lifetime;
        }

        Renderer.Render(handle, texture, SplineIterator, GradientIterator, Settings, paPositions, paLifetimes);
    }

    private sealed class TrailSplineSegment
    {
        public Vector2 Position { get; set; }
        public float ExistTil { get; init; }
    }
}
