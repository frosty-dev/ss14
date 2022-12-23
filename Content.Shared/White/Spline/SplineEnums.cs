using Content.Shared.White.Spline.CatmullRom;
using Content.Shared.White.Spline.Linear;

namespace Content.Shared.White.Spline;

public static class Spline
{
    public static ISpline<Vector2> From2DType(Spline2DType type)
        => type switch
        {
            Spline2DType.Linear => new SplineLinear2D(),
            Spline2DType.CatmullRom => new SplineCatmullRom2D(),
            _ => throw new NotImplementedException()
        };
}

public enum Spline2DType : byte
{
    Linear,
    CatmullRom
}

