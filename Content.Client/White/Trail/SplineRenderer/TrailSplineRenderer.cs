using Content.Shared.White.Trail;

namespace Content.Client.White.Trail.SplineRenderer;

public static class TrailSplineRenderer
{
    public static ITrailSplineRenderer FromType(TrailSplineRendererType type)
        => type switch
        {
            TrailSplineRendererType.Continuous => new TrailSplineRendererContinuous(),
            TrailSplineRendererType.Point => new TrailSplineRendererPoint(),
            TrailSplineRendererType.Debug => new TrailSplineRendererDebug(),
            _ => throw new NotImplementedException()
        };
}
