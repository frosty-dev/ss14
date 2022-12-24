using Content.Shared.White.Spline;
using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using System.Linq;

namespace Content.Client.White.Trail.SplineRenderer;

public sealed class TrailSplineRendererPoint : ITrailSplineRenderer
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
        if (texture == null)
            return;

        float[] splinePointParams;
        if (settings.LengthStep == 0f)
            splinePointParams = Enumerable.Range(0, paPositions.Length - 1).Select(x => (float) x).ToArray();
        else
            splinePointParams = splineIterator.IteratePointParamsByLength(paPositions, Math.Max(settings.LengthStep, 0.1f)).ToArray();

        var gradientControlGroups = gradientIterator.GetControlGroupAmount(settings.Gradient.Length);
        var colorToPointMul = 0f;
        if (gradientControlGroups > 0)
            colorToPointMul = gradientControlGroups / gradientIterator.GetControlGroupAmount(paPositions.Length);

        foreach (var u in splinePointParams)
        {
            var (position, velocity) = splineIterator.SamplePositionVelocity(paPositions, u);

            var colorVec = Vector4.One;
            if (settings.Gradient != null && settings.Gradient.Length > 0)
            {
                if (gradientControlGroups > 0)
                    colorVec = gradientIterator.SamplePosition(settings.Gradient, u * colorToPointMul);
                else
                    colorVec = settings.Gradient[0];
            }

            var quad = Box2.FromDimensions(position, texture.Size * settings.Scale / EyeManager.PixelsPerMeter);
            handle.DrawTextureRect(texture, new Box2Rotated(quad, velocity.ToAngle(), quad.Center), new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W));
        }
    }
}
