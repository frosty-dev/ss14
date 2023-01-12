using Content.Client.White.Trail.Line;
using Content.Client.White.Trail.Line.Manager;
using Content.Client.White.Trail.SplineRenderer;
using Content.Shared.White.Spline;
using Content.Shared.White.Trail;

namespace Content.Client.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent, ITrailLineHolder
{
    [ViewVariables]
    public ITrailLine? TrailLine { get; set; }

    public override Spline2DType SplineIteratorType
    {
        get => base.SplineIteratorType;
        set
        {
            if (base.SplineIteratorType == value)
                return;
            base.SplineIteratorType = value;
            if (TrailLine is TrailSpline trailSpline)
                trailSpline.SplineIterator = Spline.From2DType(value);
        }
    }

    public override Spline4DType GradientIteratorType
    {
        get => base.GradientIteratorType;
        set
        {
            if (base.GradientIteratorType == value)
                return;
            base.GradientIteratorType = value;
            if (TrailLine is TrailSpline trailSpline)
                trailSpline.GradientIterator = Spline.From4DType(value);
        }
    }

    public override TrailSplineRendererType SplineRendererType
    {
        get => base.SplineRendererType;
        set
        {
            if (base.SplineRendererType == value)
                return;
            base.SplineRendererType = value;
            if (TrailLine is TrailSpline trailSpline)
                trailSpline.Renderer = TrailSplineRenderer.FromType(value);
        }
    }
}
