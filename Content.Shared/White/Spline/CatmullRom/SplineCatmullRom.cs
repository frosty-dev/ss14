using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public abstract class SplineCatmullRom<T> : Spline<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SamplePosition(ReadOnlySpan<T> controlPoints, int u, float t)
        => CalculateCatmullRom(GetCurrentControlPoints(controlPoints, u), CalculateCoefficientsPosition(t, t * t, t * t * t));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SampleGradient(ReadOnlySpan<T> controlPoints, int u, float t)
        => CalculateCatmullRom(GetCurrentControlPoints(controlPoints, u), CalculateCoefficientsGradient(t, t * t));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override (T Position, T Gradient) SamplePositionGradient(ReadOnlySpan<T> controlPoints, int u, float t)
    {
        var tt = t * t;
        return (
            CalculateCatmullRom(GetCurrentControlPoints(controlPoints, u), CalculateCoefficientsPosition(t, tt, tt * t)),
            CalculateCatmullRom(GetCurrentControlPoints(controlPoints, u), CalculateCoefficientsGradient(t, tt))
            );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual (T p0, T p1, T p2, T p3) GetCurrentControlPoints(ReadOnlySpan<T> controlPoints, int u)
    {
        T p1 = controlPoints[u];
        T p2 = controlPoints[u + 1];
        T p0 = u == 0 ? Add(p1, Subtract(p1, p2)) : controlPoints[u - 1];
        T p3 = u + 2 == controlPoints.Length ? Add(p2, Subtract(p2, p2)) : controlPoints[u + 2];
        return (p0, p1, p2, p3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual (float c0, float c1, float c2, float c3) CalculateCoefficientsPosition(float t, float tt, float ttt)
        => (
        -ttt + 2.0f * tt - t,
        3.0f * ttt - 5.0f * tt + 2.0f,
        -3.0f * ttt + 4.0f * tt + t,
        ttt - tt
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual (float c0, float c1, float c2, float c3) CalculateCoefficientsGradient(float t, float tt)
        => (
        -3.0f * tt + 4.0f * t - 1,
        9.0f * tt - 10.0f * t,
        -9.0f * tt + 8.0f * t + 1.0f,
        3.0f * tt - 2.0f * t
        );

    protected abstract T CalculateCatmullRom((T p0, T p1, T p2, T p3) points, (float c0, float c1, float c2, float c3) coeffs);
}
