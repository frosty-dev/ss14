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
    public static ISpline<Vector4> From4DType(Spline4DType type)
    => type switch
    {
        Spline4DType.Linear => new SplineLinear4D(),
        Spline4DType.Bezier => new SplineCubicBezier4D(),
        _ => throw new NotImplementedException()
    };
    public static ISpline<Color> FromColorType(SplineColorType type)
        => type switch
        {
            SplineColorType.Linear => new SplineLinearColor(),
            SplineColorType.Bezier => new SplineCubicBezierColor(),
            _ => throw new NotImplementedException()
        };
}

public enum Spline2DType : byte
{
    Linear,
    CatmullRom
}

public enum Spline4DType : byte
{
    Linear,
    Bezier
}

public enum SplineColorType : byte
{
    Linear,
    Bezier
}

