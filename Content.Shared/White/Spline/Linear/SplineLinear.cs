using Content.Shared.White.Spline.CatmullRom;
using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.Linear;

public abstract class SplineLinear<T> : Spline<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SamplePosition(ReadOnlySpan<T> controlPoints, int u, float t)
        => Add(Multiply(controlPoints[u], 1 - t), Multiply(controlPoints[u + 1], t));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SampleGradient(ReadOnlySpan<T> controlPoints, int u, float t)
        => u == 0 ? Subtract(controlPoints[u + 1], controlPoints[u]) : Subtract(controlPoints[u + 1], controlPoints[u - 1]);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override (T Position, T Gradient) SamplePositionGradient(ReadOnlySpan<T> controlPoints, int u, float t)
        => (
            SamplePosition(controlPoints, u, t),
            SampleGradient(controlPoints, u, t)
        );
    protected abstract T Multiply(T op1, float scalar);
}
