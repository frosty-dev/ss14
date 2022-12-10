using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[DataDefinition]
[Serializable, NetSerializable]
public sealed class TrailSettings : ITrailSettings
{
    public static readonly TrailSettings Default = new();

    [NonSerialized]
    private readonly Func<float, float>? _colorLifetimeDeltaLambda;

    public Vector2 Offset { get; set; } = new Vector2(0.5f, 0.0f);
    public float СreationDistanceThresholdSquared { get; set; } = 0.001f;
    public SegmentCreationMethod СreationMethod { get; set; } = SegmentCreationMethod.OnFrameUpdate;
    public Vector2 Gravity { get; set; } = new Vector2(0.05f, 0.05f);
    public Vector2 MaxRandomWalk { get; set; } = new Vector2(0.005f, 0.005f);
    public float Lifetime { get; set; } = 1f;
    public string? TexurePath { get; set; }
    public Color ColorLifetimeStart { get; set; } = Color.White;
    public Color ColorLifetimeEnd { get; set; } = Color.Transparent;
    public string? ColorLifetimeDeltaLambdaOperations { get; set; }
    public Func<float, float>? ColorLifetimeDeltaLambda => _colorLifetimeDeltaLambda;
    public TrailSettings ToTrailSettings()
        => new()
        {
            Offset = Offset,
            СreationDistanceThresholdSquared = СreationDistanceThresholdSquared,
            СreationMethod = СreationMethod,
            Gravity = Gravity,
            MaxRandomWalk = MaxRandomWalk,
            Lifetime = Lifetime,
            TexurePath = TexurePath,
            ColorLifetimeStart = ColorLifetimeStart,
            ColorLifetimeEnd = ColorLifetimeEnd,
        };
}
public enum SegmentCreationMethod : byte
{
    OnFrameUpdate,
    OnMove
}

public interface ITrailSettings
{
    Color ColorLifetimeStart { get; set; }
    Color ColorLifetimeEnd { get; set; }
    string? ColorLifetimeDeltaLambdaOperations { get; set; }
    Func<float, float>? ColorLifetimeDeltaLambda { get; }
    Vector2 Gravity { get; set; }
    float Lifetime { get; set; }
    Vector2 MaxRandomWalk { get; set; }
    Vector2 Offset { get; set; }
    string? TexurePath { get; set; }
    float СreationDistanceThresholdSquared { get; set; }
    SegmentCreationMethod СreationMethod { get; set; }
    TrailSettings ToTrailSettings();
}
