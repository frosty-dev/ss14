namespace Content.Shared.White.Spline;

public interface ISpline<T>
{
    T SamplePosition(ReadOnlySpan<T> controlPoints, float u);
    T SampleVelocity(ReadOnlySpan<T> controlPoints, float u);
    (T Position, T Velocity) SamplePositionVelocity(ReadOnlySpan<T> controlPoints, float u);
    IEnumerable<float> IteratePointParamsByLength(T[] controlPoints, float lengthStepSize);
    float GetControlGroupAmount(int controlPointAmount);
}
