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
        ITrailSettings settings,
        Vector2[] paPositions,
        float[] paLifetimes
        )
    {
        if (texture == null)
            return;

        (int u, float t)[] splinePointParams;
        if (settings.LengthStep == 0f)
            splinePointParams = Enumerable.Range(0, paPositions.Length - 1).Select(x => (u: x, t: 0f)).ToArray();
        else
            splinePointParams = splineIterator.IteratePointParamsByLength(paPositions, Math.Max(settings.LengthStep, 0.1f)).ToArray();

        var colorToPointMul = 1f / (paPositions.Length - 1) * (settings.Gradient?.Count ?? 1 - 1);
        foreach (var (u, t) in splinePointParams)
        {
            var (position, movementGradient) = splineIterator.SamplePositionGradient(paPositions, u, t);

            var color = Color.White;
            if (settings.Gradient != null)
            {
                if (settings.Gradient.Count == 1)
                    color = settings.Gradient[0];
                else if (settings.Gradient.Count > 1)
                {
                    var colorGradientPos = (u + t) * colorToPointMul;
                    var colorGradientU = (int) colorGradientPos;
                    color = Color.InterpolateBetween(settings.Gradient[colorGradientU], settings.Gradient[colorGradientU + 1], colorGradientPos % 1f);
                }
            }

            var quad = Box2.FromDimensions(position, texture.Size * settings.Scale / EyeManager.PixelsPerMeter);
            handle.DrawTextureRect(texture, new Box2Rotated(quad, movementGradient.ToAngle(), quad.Center), color);
        }
    }
}
