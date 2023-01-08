using Content.Server.Chat.Systems;
using Content.Shared.CCVar;
using Content.Shared.White.TTS;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.White.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;

    private const int MaxMessageChars = 100; // same as SingleBubbleCharLimit
    private bool _isEnabled = false;

    public override void Initialize()
    {
        _cfg.OnValueChanged(CCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeNetworkEvent<RequestTTSEvent>(OnRequestTTS);
    }

    private void OnRequestTTS(RequestTTSEvent ev)
    {
        throw new NotImplementedException();
    }

    private async void OnEntitySpoke(EntityUid uid, TTSComponent component, EntitySpokeEvent args)
    {
        if (!_isEnabled ||
            args.OriginalMessage.Length > MaxMessageChars ||
            !_prototypeManager.TryIndex<TTSVoicePrototype>(component.VoicePrototypeId, out var protoVoice))
            return;

        var textSanitized = Sanitize(args.OriginalMessage);
        var textSsml = ToSsmlText(textSanitized, SpeechRate.Fast);
        var metadata = Comp<MetaDataComponent>(uid);
        var soundData = await _ttsManager.ConvertTextToSpeech(metadata.EntityName, protoVoice.Speaker, textSanitized);
        RaiseNetworkEvent(new PlayTTSEvent(uid, soundData), Filter.Pvs(uid));
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }
}
