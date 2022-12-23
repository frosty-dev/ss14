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
        ITrailSettings settings,
        Vector2[] paPositions,
        float[] paLifetimes
        )
    {
        (int u, float t)[] splinePointParams;
        if (settings.LengthStep == 0f)
            splinePointParams = Enumerable.Range(0, paPositions.Length - 1).Select(x => (u: x, t: 0f)).ToArray();
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
        foreach (var (u, t) in splinePointParams)
        {
            var splineData = splineIterator.SamplePositionGradient(paPositions, u, t);
            if (prevPosSplinePoint.HasValue)
                handle.DrawLine(splineData.Position, prevPosSplinePoint.Value, Color.Red);
            handle.DrawLine(splineData.Position, splineData.Position + splineData.Gradient, Color.White);
            handle.DrawCircle(splineData.Position, 0.03f, new Color(0, 255, 0, 255));
            prevPosSplinePoint = splineData.Position;
        }
    }
}
