using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.Linear;

public sealed class SplineLinear2D : SplineLinear<Vector2>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 Add(Vector2 op1, Vector2 op2) => op1 + op2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 Subtract(Vector2 op1, Vector2 op2) => op1 - op2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override float Magnitude(Vector2 op1) => op1.Length;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Vector2 Multiply(Vector2 op1, float scalar) => op1 * scalar;
}
