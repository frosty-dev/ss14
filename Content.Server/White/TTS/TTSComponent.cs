using Content.Shared.White.TTS;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.White.TTS;

/// <summary>
/// Apply TTS for entity chat say messages
/// </summary>
[RegisterComponent]
// ReSharper disable once InconsistentNaming
public sealed class TTSComponent : Component
{
    /// <summary>
    /// Prototype of used voice for TTS.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("voice", customTypeSerializer:typeof(PrototypeIdSerializer<TTSVoicePrototype>))]
    public string VoicePrototypeId { get; set; } = string.Empty;
}
