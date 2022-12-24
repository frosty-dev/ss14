using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public sealed class SplineCubicBezier4D : SplineCubicBezier<Vector4>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector4 Add(Vector4 op1, Vector4 op2) => new(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z, op1.W + op2.W);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector4 Subtract(Vector4 op1, Vector4 op2) => new(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z, op1.W - op2.W);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override float Magnitude(Vector4 op1) => MathF.Sqrt(op1.X * op1.X + op1.Y * op1.Y + op1.Z * op1.Z + op1.W * op1.W);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector4 CalculateBezier((Vector4 p0, Vector4 p1, Vector4 p2, Vector4 p3) points, (float c0, float c1, float c2, float c3) coeffs)
        => new(
            points.p0.X * coeffs.c0 + points.p1.X * coeffs.c1 + points.p2.X * coeffs.c2 + points.p3.X * coeffs.c3,
            points.p0.Y * coeffs.c0 + points.p1.Y * coeffs.c1 + points.p2.Y * coeffs.c2 + points.p3.Y * coeffs.c3,
            points.p0.Z * coeffs.c0 + points.p1.Z * coeffs.c1 + points.p2.Z * coeffs.c2 + points.p3.Z * coeffs.c3,
            points.p0.W * coeffs.c0 + points.p1.W * coeffs.c1 + points.p2.W * coeffs.c2 + points.p3.W * coeffs.c3
        );
}
