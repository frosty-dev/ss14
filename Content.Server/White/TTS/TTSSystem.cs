using System.Linq;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Shared.CCVar;
using Content.Shared.White.TTS;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.White.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

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
        if (string.IsNullOrEmpty(textSanitized))
        {
            return;
        }
        var soundData = await GenerateTTS(uid, textSanitized, protoVoice.Speaker);
        var ttsEvent = new PlayTTSEvent(uid, soundData);

        // Say
        if (args.ObfuscatedMessage is null)
        {
            RaiseNetworkEvent(ttsEvent, Filter.Pvs(uid));
            return;
        }

        // Whisper
        var wList = new List<string>
        {
            "тсс",
            "псс",
            "ччч",
            "ссч",
            "сфч",
            "тст"
        };
        var chosenWhisperText = _random.Pick(wList);
        var obfSoundData = await GenerateTTS(uid, chosenWhisperText, protoVoice.Speaker);
        var obfTtsEvent = new PlayTTSEvent(uid, obfSoundData);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue)
                continue;
            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).LengthSquared;
            if (distance > ChatSystem.VoiceRange * ChatSystem.VoiceRange)
                continue;

            RaiseNetworkEvent(distance > ChatSystem.WhisperRange ? obfTtsEvent : ttsEvent, session);
        }
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }

    private async Task<byte[]> GenerateTTS(EntityUid uid, string text, string speaker)
    {
        var textSanitized = Sanitize(text);
        var metadata = Comp<MetaDataComponent>(uid);
        return await _ttsManager.ConvertTextToSpeech(metadata.EntityName, speaker, textSanitized);
    }
}
