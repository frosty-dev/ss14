using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[DataDefinition]
[Serializable, NetSerializable]
public sealed class TrailShaderSettings
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("shaderId", required: true)]
    public string ShaderId { get; set; } = string.Empty;
}
