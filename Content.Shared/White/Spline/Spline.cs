using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public abstract class Spline<T> : ISpline<T>
{
    public abstract T SamplePosition(ReadOnlySpan<T> controlPoints, int u, float t);
    public abstract T SampleGradient(ReadOnlySpan<T> controlPoints, int u, float t);
    public abstract (T Position, T Gradient) SamplePositionGradient(ReadOnlySpan<T> controlPoints, int u, float t);

    public virtual IEnumerable<(int u, float t)> IteratePointParamsByLength(T[] controlPoints, float lengthStepSize)
    {
        //ну а хули нам наивным салюшонам
        for (var u = 0; u < controlPoints.Length - 1; u++)
        {
            var segmentLength = ApproximateArcLength(controlPoints, u);
            var tStepSize = lengthStepSize / segmentLength;
            for (var t = 0f; t < 1; t += tStepSize)
                yield return (u, t);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual float ApproximateArcLength(ReadOnlySpan<T> controlPoints, int u)
        => Magnitude(Subtract(controlPoints[u], controlPoints[u + 1]));

    protected abstract T Add(T op1, T op2);
    protected abstract T Subtract(T op1, T op2);
    protected abstract float Magnitude(T op1);
}
