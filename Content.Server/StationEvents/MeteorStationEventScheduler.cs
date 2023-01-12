using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Server.StationEvents;

public sealed class MeteorStationEventSchedulerSystem : GameRuleSystem
{
    public override string Prototype => "MeteorStationEventScheduler";

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    private float _endTime;
    [ViewVariables(VVAccess.ReadWrite)]
    private float _maxChaos;
    [ViewVariables(VVAccess.ReadWrite)]
    private float _startingChaos;
    [ViewVariables(VVAccess.ReadWrite)]
    private float _timeUntilNextEvent;
    [ViewVariables(VVAccess.ReadWrite)]
    public float _timeUntilCallShuttle = 1800;
    [ViewVariables(VVAccess.ReadWrite)]
    private bool _shuttleAnnouncement = true;

    [ViewVariables]
    public float ChaosModifier
    {
        get
        {
            var roundTime = (float) _gameTicker.RoundDuration().TotalSeconds;
            if (roundTime > _endTime)
                return _maxChaos;

            return (_maxChaos / _endTime) * roundTime + _startingChaos;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetSeverityModifierEvent>(OnGetSeverityModifier);
    }

    public override void Started()
    {
        var avgChaos = _cfg.GetCVar(CCVars.EventsRampingAverageChaos);
        var avgTime = _cfg.GetCVar(CCVars.EventsRampingAverageEndTime);

        // Worlds shittiest probability distribution
        // Got a complaint? Send them to
        _maxChaos = _random.NextFloat(avgChaos - avgChaos / 4, avgChaos + avgChaos / 4);
        // This is in minutes, so *60 for seconds (for the chaos calc)
        _endTime = _random.NextFloat(avgTime - avgTime / 4, avgTime + avgTime / 4) * 60f;
        _startingChaos = _maxChaos / 10;

        PickNextEventTime();
    }

    public override void Ended()
    {
        _endTime = 0f;
        _maxChaos = 0f;
        _startingChaos = 0f;
        _timeUntilNextEvent = 0f;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var roundTime = (float) _gameTicker.RoundDuration().TotalSeconds;
        if (roundTime >= _timeUntilCallShuttle && _shuttleAnnouncement)
        {
            foreach (var comp in EntityQuery<StationDataComponent>(true))
            {
                _chatSystem.DispatchStationAnnouncement(comp.Owner, Loc.GetString("emergency_shuttle_meteor_available"));
            }
            _shuttleAnnouncement = false;
        }

        if (!RuleStarted || !_event.EventsEnabled)
            return;

        if (_timeUntilNextEvent > 0f)
        {
            _timeUntilNextEvent -= frameTime;
            return;
        }

        PickNextEventTime();
        _event.RunCertainEvent("MeteorSwarm");
    }

    private void OnGetSeverityModifier(GetSeverityModifierEvent ev)
    {
        if (!RuleStarted)
            return;

        ev.Modifier *= ChaosModifier;
        Logger.Info($"Ramping set modifier to {ev.Modifier}");
    }

    private void PickNextEventTime()
    {
        var mod = ChaosModifier;
        // 4-12 minutes baseline. Will get faster over time as the chaos mod increases.
        const float minTime = 240f;
        const float maxTime = 720f;

        _timeUntilNextEvent = _random.NextFloat(minTime / mod, maxTime / mod);
    }
}
