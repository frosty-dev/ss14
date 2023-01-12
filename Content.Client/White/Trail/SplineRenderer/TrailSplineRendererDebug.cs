using Content.Shared.White.Spline;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using System.Linq;

namespace Content.Client.White.Trail.SplineRenderer;

public sealed class TrailSplineRendererDebug : ITrailSplineRenderer
{
    public void Render(
        DrawingHandleWorld handle,
        Texture? texture,
        ISpline<Vector2> splineIterator,
        ISpline<Vector4> gradientIterator,
        ITrailSettings settings,
        Vector2[] paPositions,
        float[] paLifetimes
        )
    {
        float[] splinePointParams;
        if (settings.LengthStep == 0f)
            splinePointParams = Enumerable.Range(0, paPositions.Length - 1).Select(x => (float) x).ToArray();
        else
            splinePointParams = splineIterator.IteratePointParamsByLength(paPositions, Math.Max(settings.LengthStep, 0.1f)).ToArray();

        Vector2? prevPosControlPoint = null;
        foreach (var item in paPositions)
        {
            if (prevPosControlPoint.HasValue)
                handle.DrawLine(item, prevPosControlPoint.Value, Color.Blue);
            prevPosControlPoint = item;
        }

        Vector2? prevPosSplinePoint = null;
        foreach (var u in splinePointParams)
        {
            var (position, velocity) = splineIterator.SamplePositionVelocity(paPositions, u);
            if (prevPosSplinePoint.HasValue)
                handle.DrawLine(position, prevPosSplinePoint.Value, Color.Red);
            handle.DrawLine(position, position + velocity, Color.White);
            handle.DrawCircle(position, 0.03f, new Color(0, 255, 0, 255));
            prevPosSplinePoint = position;
        }
    }
}
