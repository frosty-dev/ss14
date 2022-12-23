using Content.Shared.White.Spline;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using System.Linq;

namespace Content.Client.White.Trail.SplineRenderer;

public sealed class TrailSplineRendererContinuous : ITrailSplineRenderer
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

        var colorToPointMul = 1f / (paPositions.Length - 1) * (settings.Gradient.Length - 1);
        (Vector2, Vector2)? prevPoints = null;
        foreach (var (u, t) in splinePointParams)
        {
            var (position, movementGradient) = splineIterator.SamplePositionGradient(paPositions, u, t);

            var offset = movementGradient.Rotated90DegreesAnticlockwiseWorld.Normalized * settings.Scale.X;
            var curPoints = (position - offset, position + offset);

            if (prevPoints.HasValue)
            {
                var color = Color.White;
                if (settings.Gradient.Length == 1)
                    color = settings.Gradient[0];
                else if (settings.Gradient.Length > 1)
                {
                    var colorGradientPos = (u + t) * colorToPointMul;
                    var colorGradientU = (int) colorGradientPos;
                    color = Color.InterpolateBetween(settings.Gradient[colorGradientU], settings.Gradient[colorGradientU + 1], colorGradientPos % 1f);
                }

                if (texture != null)
                {
                    var verts = new DrawVertexUV2D[] {
                        new (curPoints.Item1, Vector2.Zero),
                        new (curPoints.Item2, Vector2.UnitY),
                        new (prevPoints.Value.Item2, Vector2.One),
                        new (prevPoints.Value.Item1, Vector2.UnitX),
                    };
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, texture, verts, color);
                }
                else
                {
                    var verts = new Vector2[] {
                        curPoints.Item1,
                        curPoints.Item2,
                        prevPoints.Value.Item2,
                        prevPoints.Value.Item1,
                    };
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color);
                }
            }

            prevPoints = curPoints;
        }
    }
}
