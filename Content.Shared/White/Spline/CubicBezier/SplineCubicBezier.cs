using System.Linq;
using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public abstract class SplineCubicBezier<T> : Spline<T>
{
    protected const int LookupPrecision = 100;

    protected static readonly (float c0, float c1, float c2, float c3)[] PositionCoefficientLookup
        = Enumerable.Range(0, LookupPrecision + 1).Select(x => CalculateCoefficientsPosition((float) x / LookupPrecision)).ToArray();

    protected static readonly (float c0, float c1, float c2, float c3)[] GradientCoefficientLookup
        = Enumerable.Range(0, LookupPrecision + 1).Select(x => CalculateCoefficientsTangent((float) x / LookupPrecision)).ToArray();

    protected static (float c0, float c1, float c2, float c3) CalculateCoefficientsPosition(float t)
    {
        var tt = t * t;
        var ttt = tt * t;
        return (
            -ttt + 3f * tt - 3f * t + 1f,
            3f * ttt - 6f * tt + 3f * t,
            -3f * ttt + 3f * tt,
            ttt
            );
    }

    protected static (float c0, float c1, float c2, float c3) CalculateCoefficientsTangent(float t)
    {
        var tt = t * t;
        return (
            -3f * tt + 6f * t - 3,
            9f * tt - 12f * t + 3,
            -9f * tt + 6f * t,
            3f * tt
            );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(float t) => (int) (t * LookupPrecision);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SamplePosition(ReadOnlySpan<T> controlPoints, float u)
    {
        return CalculateBezier(GetCurrentControlPoints(controlPoints, (int) u), PositionCoefficientLookup[GetLookupIndex(u % 1)]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T SampleVelocity(ReadOnlySpan<T> controlPoints, float u)
    {
        return CalculateBezier(GetCurrentControlPoints(controlPoints, (int) u), GradientCoefficientLookup[GetLookupIndex(u % 1)]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override (T Position, T Velocity) SamplePositionVelocity(ReadOnlySpan<T> controlPoints, float u)
    {
        var lookupIndex = GetLookupIndex(u % 1);
        var currentControlPoints = GetCurrentControlPoints(controlPoints, (int) u);
        return (
            CalculateBezier(currentControlPoints, PositionCoefficientLookup[lookupIndex]),
            CalculateBezier(currentControlPoints, GradientCoefficientLookup[lookupIndex])
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float GetControlGroupAmount(int controlPointAmount) => (controlPointAmount - 1) / 3f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual (T p0, T p1, T p2, T p3) GetCurrentControlPoints(ReadOnlySpan<T> controlPoints, int u)
        => (controlPoints[u], controlPoints[u + 1], controlPoints[u + 2], controlPoints[u + 3]);

    protected abstract T CalculateBezier((T p0, T p1, T p2, T p3) points, (float c0, float c1, float c2, float c3) coeffs);
}
