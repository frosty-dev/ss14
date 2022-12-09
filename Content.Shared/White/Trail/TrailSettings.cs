using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[DataDefinition]
[Serializable, NetSerializable]
public sealed class TrailSettings : ITrailSettings
{
    public static readonly TrailSettings Default = new();

    public Vector2 Offset { get; set; } = new Vector2(0.5f, 0.0f);
    public float СreationDistanceThresholdSquared { get; set; } = 0.001f;
    public SegmentCreationMethod СreationMethod { get; set; } = SegmentCreationMethod.OnFrameUpdate;
    public Vector2 Gravity { get; set; } = new Vector2(0.05f, 0.05f);
    public Vector2 MaxRandomWalk { get; set; } = new Vector2(0.005f, 0.005f);
    public float Lifetime { get; set; } = 1f;
    public string? TexurePath { get; set; } = string.Empty;
    public Color ColorBase { get; set; } = Color.White;
    public Color ColorLifetimeMod { get; set; } = Color.Transparent;

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
            ColorBase = ColorBase,
            ColorLifetimeMod = ColorLifetimeMod,
        };
}
public enum SegmentCreationMethod : byte
{
    OnFrameUpdate,
    OnMove
}

public interface ITrailSettings
{
    Color ColorBase { get; set; }
    Color ColorLifetimeMod { get; set; }
    Vector2 Gravity { get; set; }
    float Lifetime { get; set; }
    Vector2 MaxRandomWalk { get; set; }
    Vector2 Offset { get; set; }
    string? TexurePath { get; set; }
    float СreationDistanceThresholdSquared { get; set; }
    SegmentCreationMethod СreationMethod { get; set; }
    TrailSettings ToTrailSettings();
}
