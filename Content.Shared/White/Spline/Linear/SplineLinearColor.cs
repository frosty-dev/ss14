using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.Linear;

public sealed class SplineLinearColor : SplineLinear<Color>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color Add(Color op1, Color op2) => new(op1.R + op2.R, op1.G + op2.G, op1.B + op2.B, op1.A + op2.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color Subtract(Color op1, Color op2) => new(op1.R - op2.R, op1.G - op2.G, op1.B - op2.B, op1.A - op2.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override float Magnitude(Color op1) => MathF.Sqrt(op1.R * op1.R + op1.G * op1.G + op1.B * op1.B + op1.A * op1.A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override Color Multiply(Color op1, float scalar) => new(op1.R * scalar, op1.G * scalar, op1.B * scalar, op1.A * scalar);
}
