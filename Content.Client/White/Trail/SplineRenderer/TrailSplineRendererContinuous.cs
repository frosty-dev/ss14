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

        var gradientControlGroups = gradientIterator.GetControlGroupAmount(settings.Gradient.Length);
        var colorToPointMul = 0f;
        if (gradientControlGroups > 0)
            colorToPointMul = gradientControlGroups / splineIterator.GetControlGroupAmount(paPositions.Length);

        (Vector2, Vector2)? prevPoints = null;
        foreach (var u in splinePointParams)
        {
            var (position, velocity) = splineIterator.SamplePositionVelocity(paPositions, u);

            var offset = velocity.Rotated90DegreesAnticlockwiseWorld.Normalized * settings.Scale.X;
            var curPoints = (position - offset, position + offset);

            if (prevPoints.HasValue)
            {
                var colorVec = Vector4.One;
                if (settings.Gradient != null && settings.Gradient.Length > 0)
                {
                    if (gradientControlGroups > 0)
                        colorVec = gradientIterator.SamplePosition(settings.Gradient, u * colorToPointMul);
                    else
                        colorVec = settings.Gradient[0];
                }

                if (texture != null)
                {
                    var verts = new DrawVertexUV2D[] {
                        new (curPoints.Item1, Vector2.Zero),
                        new (curPoints.Item2, Vector2.UnitY),
                        new (prevPoints.Value.Item2, Vector2.One),
                        new (prevPoints.Value.Item1, Vector2.UnitX),
                    };
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, texture, verts, new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W));
                }
                else
                {
                    var verts = new Vector2[] {
                        curPoints.Item1,
                        curPoints.Item2,
                        prevPoints.Value.Item2,
                        prevPoints.Value.Item1,
                    };
                    handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W));
                }
            }

            prevPoints = curPoints;
        }
    }
}
