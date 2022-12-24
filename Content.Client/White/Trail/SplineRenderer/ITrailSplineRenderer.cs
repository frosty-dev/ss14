using Content.Shared.White.Spline;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;

namespace Content.Client.White.Trail.SplineRenderer;

public interface ITrailSplineRenderer
{
    void Render(
        DrawingHandleWorld handle,
        Texture? texture,
        ISpline<Vector2> splineIterator,
        ISpline<Vector4> gradientIterator,
        ITrailSettings settings,
        Vector2[] paPositions,
        float[] paLifetimes
        );
}
