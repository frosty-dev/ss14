using System.Runtime.CompilerServices;

namespace Content.Shared.White.Spline.CatmullRom;

public abstract class Spline<T> : ISpline<T>
{
    public abstract T SamplePosition(ReadOnlySpan<T> controlPoints, float u);
    public abstract T SampleVelocity(ReadOnlySpan<T> controlPoints, float u);
    public abstract (T Position, T Velocity) SamplePositionVelocity(ReadOnlySpan<T> controlPoints, float u);

    public virtual IEnumerable<float> IteratePointParamsByLength(T[] controlPoints, float lengthStepSize)
    {
        //ну а хули нам наивным салюшонам
        for (var u = 0; u < controlPoints.Length - 1; u++)
        {
            var segmentLength = ApproximateArcLength(controlPoints, u);
            var tStepSize = lengthStepSize / segmentLength;
            for (var t = 0f; t < 1; t += tStepSize)
                yield return u + t;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual float GetControlGroupAmount(int controlPointAmount) => controlPointAmount - 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual float ApproximateArcLength(ReadOnlySpan<T> controlPoints, float u)
        => Magnitude(Subtract(controlPoints[(int)u], controlPoints[(int) u + 1]));

    protected abstract T Add(T op1, T op2);
    protected abstract T Subtract(T op1, T op2);
    protected abstract float Magnitude(T op1);

}
