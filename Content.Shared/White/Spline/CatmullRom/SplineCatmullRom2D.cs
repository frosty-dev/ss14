using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public sealed class SplineCatmullRom2D : SplineCatmullRom<Vector2>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 Add(Vector2 op1, Vector2 op2) => op1 + op2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 Subtract(Vector2 op1, Vector2 op2) => op1 - op2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override float Magnitude(Vector2 op1) => op1.Length;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 CalculateCatmullRom((Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) points, (float c0, float c1, float c2, float c3) coeffs)
        => new(
            0.5f * (points.p0.X * coeffs.c0 + points.p1.X * coeffs.c1 + points.p2.X * coeffs.c2 + points.p3.X * coeffs.c3),
            0.5f * (points.p0.Y * coeffs.c0 + points.p1.Y * coeffs.c1 + points.p2.Y * coeffs.c2 + points.p3.Y * coeffs.c3)
        );
}
