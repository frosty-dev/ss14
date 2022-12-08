using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[DataDefinition]
[Serializable, NetSerializable]
public sealed class TrailSettings
{
    public readonly static TrailSettings Default = new()
    {
        Gravity = new Vector2(0.05f, 0.05f),
        MaxRandomWalk = new Vector2(0.005f, 0.005f),
    };

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("offset", required: true)]
    public Vector2 Offset { get; set; } = Vector2.UnitX;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("сreationDistanceThreshold")]
    public float СreationDistanceThreshold { get; set; } = 0.1f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("сreationDistanceThresholdSquared")]
    public float СreationDistanceThresholdSquared { get; set; } = 0.001f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("сreationMethod")]
    public PointCreationMethod СreationMethod { get; set; } = PointCreationMethod.OnMove;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("gravity")]
    public Vector2 Gravity { get; set; } = Vector2.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("randomWalk")]
    public Vector2 MaxRandomWalk { get; set; } = Vector2.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("lifetime", required: true)]
    public float Lifetime { get; set; } = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("texturePath", required: true)]
    public string TexurePath { get; set; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("colorBase")]
    public Color ColorBase { get; set; } = Color.White;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("colorLifetimeMod")]
    public Color ColorLifetimeMod { get; set; } = Color.Transparent;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("shaderSettings")]
    public TrailShaderSettings? ShaderSettings { get; set; }
}
public enum PointCreationMethod : byte
{
    OnFrameUpdate,
    OnMove
}
