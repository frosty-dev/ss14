using Content.Shared.White.Spline.CatmullRom;
using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.Linear;

public abstract class SplineLinear<T> : Spline<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SamplePosition(ReadOnlySpan<T> controlPoints, float u)
    {
        var iu = (int) u;
        var t = u % 1;
        return Add(Multiply(controlPoints[iu], 1 - t), Multiply(controlPoints[iu + 1], t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SampleVelocity(ReadOnlySpan<T> controlPoints, float u)
    {
        var iu = (int) u;
        return iu == 0 ? Subtract(controlPoints[iu + 1], controlPoints[iu]) : Subtract(controlPoints[iu + 1], controlPoints[iu - 1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override (T Position, T Velocity) SamplePositionVelocity(ReadOnlySpan<T> controlPoints, float u)
        => (
            SamplePosition(controlPoints, u),
            SampleVelocity(controlPoints, u)
        );
    protected abstract T Multiply(T op1, float scalar);
}
