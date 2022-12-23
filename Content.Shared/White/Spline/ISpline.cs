namespace Content.Shared.White.Spline;

public interface ISpline<T>
{
    T SamplePosition(ReadOnlySpan<T> controlPoints, int u, float t);
    T SampleGradient(ReadOnlySpan<T> controlPoints, int u, float t);
    (T Position, T Gradient) SamplePositionGradient(ReadOnlySpan<T> controlPoints, int u, float t);
    IEnumerable<(int u, float t)> IteratePointParamsByLength(T[] controlPoints, float lengthStepSize);

}
