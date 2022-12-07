using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Server.MachineLinking.Components;
using Content.Shared.TextScreen;
using Robust.Server.GameObjects;
using Content.Shared.MachineLinking;
using Content.Server.UserInterface;
using Content.Shared.Access.Systems;
using Content.Server.Interaction;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server.MachineLinking.System
{
    public sealed partial class SignalTimerSystem : EntitySystem
    {
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SignalLinkerSystem _signalSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly InteractionSystem _interaction = default!;
        [Dependency] private readonly RadioSystem _radioSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SignalTimerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<SignalTimerComponent, AfterActivatableUIOpenEvent>(OnAfterActivatableUIOpen);

            SubscribeLocalEvent<SignalTimerComponent, SignalTimerTextChangedMessage>(OnTextChangedMessage);
            SubscribeLocalEvent<SignalTimerComponent, SignalTimerDelayChangedMessage>(OnDelayChangedMessage);
            SubscribeLocalEvent<SignalTimerComponent, SignalTimerStartMessage>(OnTimerStartMessage);
        }


        private void Report(EntityUid source, string channelName, string messageKey, params (string, object)[] args)
        {
            var message = args.Length == 0 ? Loc.GetString(messageKey) : Loc.GetString(messageKey, args);
            var channel = _prototypeManager.Index<RadioChannelPrototype>(channelName);
            _radioSystem.SendRadioMessage(source, message, channel);
        }

        private void OnInit(EntityUid uid, SignalTimerComponent component, ComponentInit args)
        {
            if (TryComp(uid, out SignalTransmitterComponent? comp))
            {
                comp.Outputs.TryAdd(component.TriggerPort, new());
                comp.Outputs.TryAdd(component.StartPort, new());
            }

            _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);
        }

        private void OnAfterActivatableUIOpen(EntityUid uid, SignalTimerComponent component, AfterActivatableUIOpenEvent args)
        {
            var time = TryComp<ActiveSignalTimerComponent>(uid, out var active) ? active.TriggerTime : TimeSpan.Zero;

            if (_ui.TryGetUi(uid, SignalTimerUiKey.Key, out var bui))
            {
                _ui.SetUiState(bui, new SignalTimerBoundUserInterfaceState(component.Label,
                    TimeSpan.FromSeconds(component.Delay).Minutes.ToString("D2"),
                    TimeSpan.FromSeconds(component.Delay).Seconds.ToString("D2"),
                    component.CanEditLabel,
                    time,
                    active != null,
                    _accessReader.IsAllowed(args.User, uid)));
            }
        }

        public bool Trigger(EntityUid uid, SignalTimerComponent signalTimer)
        {
            RemComp<ActiveSignalTimerComponent>(uid);

            _signalSystem.InvokePort(uid, signalTimer.TriggerPort);

            var announceMessage = signalTimer.Label;
            if (string.IsNullOrWhiteSpace(announceMessage)) { announceMessage = Loc.GetString("label-none");}

            if (signalTimer.TimerCanAnnounce)
            {
                Report(uid, SignalTimerComponent.SecChannel, "timer-end-announcement", ("Label", announceMessage));
            }
            _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Text);

            if (_ui.TryGetUi(uid, SignalTimerUiKey.Key, out var bui))
            {
                _ui.SetUiState(bui, new SignalTimerBoundUserInterfaceState(signalTimer.Label,
                    TimeSpan.FromSeconds(signalTimer.Delay).Minutes.ToString("D2"),
                    TimeSpan.FromSeconds(signalTimer.Delay).Seconds.ToString("D2"),
                    signalTimer.CanEditLabel,
                    TimeSpan.Zero,
                    RemComp<ActiveSignalTimerComponent>(uid),
                    null));
            }

            return true;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            foreach (var (active, timer) in EntityQuery<ActiveSignalTimerComponent, SignalTimerComponent>())
            {
                if (active.TriggerTime <= _gameTiming.CurTime)
                {
                    Trigger(timer.Owner, timer);

                    if (timer.DoneSound != null)
                    {
                        var filter = Filter.Pvs(timer.Owner, entityManager: EntityManager);
                        _audio.Play(timer.DoneSound, filter, timer.Owner, false ,timer.SoundParams);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a UI <paramref name="message"/> is allowed to be sent by the user.
        /// </summary>
        /// <param name="uid">The entity that is interacted with.</param>
        private bool IsMessageValid(EntityUid uid, BoundUserInterfaceMessage message)
        {
            if (message.Session.AttachedEntity is not { Valid: true } mob)
                return false;

            if (!_interaction.InRangeUnobstructed(mob, uid))
                return false;

            if (!_accessReader.IsAllowed(mob, uid))
                return false;

            return true;
        }

        private void OnTextChangedMessage(EntityUid uid, SignalTimerComponent component, SignalTimerTextChangedMessage args)
        {
            if (!IsMessageValid(uid, args))
                return;

            component.Label = args.Text[..Math.Min(5,args.Text.Length)];
            _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);
        }

        private void OnDelayChangedMessage(EntityUid uid, SignalTimerComponent component, SignalTimerDelayChangedMessage args)
        {
            if (!IsMessageValid(uid, args))
                return;

            component.Delay = args.Delay.TotalSeconds;
        }

        private void OnTimerStartMessage(EntityUid uid, SignalTimerComponent component, SignalTimerStartMessage args)
        {
            if (!IsMessageValid(uid, args))
                return;

            if (!HasComp<ActiveSignalTimerComponent>(uid))
            {
                var activeTimer = EnsureComp<ActiveSignalTimerComponent>(uid);
                activeTimer.TriggerTime = _gameTiming.CurTime + TimeSpan.FromSeconds(component.Delay);
                component.User = args.User;

                _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Timer);
                _appearanceSystem.SetData(uid, TextScreenVisuals.TargetTime, activeTimer.TriggerTime);
                _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);

                _signalSystem.InvokePort(uid, component.StartPort);

                var announceMessage = component.Label;
                if (string.IsNullOrWhiteSpace(announceMessage)) { announceMessage = Loc.GetString("label-none");}

                //sorry, skill issue
                var time = TimeSpan.FromSeconds(component.Delay);
                var timeFormatted = string.Format("{0}{1}{2}",
                    time.Duration().Hours > 0 ? $"{time.Hours:0} час;{(time.Hours == 1 ? string.Empty : "ов")} " : string.Empty,
                    time.Duration().Minutes > 0 ? $"{time.Minutes:0} минут;{(time.Minutes != 1 ? string.Empty : "а")} " : string.Empty,
                    time.Duration().Seconds > 0 ? $"{time.Seconds:0} секунд{(time.Seconds != 1 ? string.Empty : "а")} " : string.Empty);
                if (component.TimerCanAnnounce)
                {
                    Report(uid, SignalTimerComponent.SecChannel, "timer-start-announcement", ("Label", announceMessage), ("Time", timeFormatted));
                }
            }
            else
            {
                component.User = args.User;
                RemComp<ActiveSignalTimerComponent>(uid);

                _appearanceSystem.SetData(uid, TextScreenVisuals.Mode, TextScreenMode.Text);
                _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, component.Label);

                var announceMessage = component.Label;
                if (string.IsNullOrWhiteSpace(announceMessage)) { announceMessage = Loc.GetString("label-none");}

                if (component.TimerCanAnnounce)
                {
                    Report(uid, SignalTimerComponent.SecChannel, "timer-suffer-end", ("Label", announceMessage));
                }
            }
        }
    }
}
