using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public sealed class SplineCubicBezierColor : SplineCubicBezier<Color>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color Add(Color op1, Color op2) => new(op1.R + op2.R, op1.G + op2.G, op1.B + op2.B, op1.A + op2.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color Subtract(Color op1, Color op2) => new(op1.R - op2.R, op1.G - op2.G, op1.B - op2.B, op1.A - op2.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override float Magnitude(Color op1) => MathF.Sqrt(op1.R * op1.R + op1.G * op1.G + op1.B * op1.B + op1.A * op1.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color CalculateBezier((Color p0, Color p1, Color p2, Color p3) points, (float c0, float c1, float c2, float c3) coeffs)
        => new(
            points.p0.R * coeffs.c0 + points.p1.R * coeffs.c1 + points.p2.R * coeffs.c2 + points.p3.R * coeffs.c3,
            points.p0.G * coeffs.c0 + points.p1.G * coeffs.c1 + points.p2.G * coeffs.c2 + points.p3.G * coeffs.c3,
            points.p0.B * coeffs.c0 + points.p1.B * coeffs.c1 + points.p2.B * coeffs.c2 + points.p3.B * coeffs.c3,
            points.p0.A * coeffs.c0 + points.p1.A * coeffs.c1 + points.p2.A * coeffs.c2 + points.p3.A * coeffs.c3
        );
}
