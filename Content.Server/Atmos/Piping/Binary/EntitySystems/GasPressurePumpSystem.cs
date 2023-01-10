using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Chat.Managers;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;

namespace Content.Server.Atmos.Piping.Binary.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasPressurePumpSystem : EntitySystem
    {
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasPressurePumpComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<GasPressurePumpComponent, AtmosDeviceUpdateEvent>(OnPumpUpdated);
            SubscribeLocalEvent<GasPressurePumpComponent, AtmosDeviceDisabledEvent>(OnPumpLeaveAtmosphere);
            SubscribeLocalEvent<GasPressurePumpComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<GasPressurePumpComponent, InteractHandEvent>(OnPumpInteractHand);
            // Bound UI subscriptions
            SubscribeLocalEvent<GasPressurePumpComponent, GasPressurePumpChangeOutputPressureMessage>(OnOutputPressureChangeMessage);
            SubscribeLocalEvent<GasPressurePumpComponent, GasPressurePumpToggleStatusMessage>(OnToggleStatusMessage);
        }

        private void OnInit(EntityUid uid, GasPressurePumpComponent pump, ComponentInit args)
        {
            UpdateAppearance(uid, pump);
        }

        private void OnExamined(EntityUid uid, GasPressurePumpComponent pump, ExaminedEvent args)
        {
            if (!EntityManager.GetComponent<TransformComponent>(pump.Owner).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
                return;

            if (Loc.TryGetString("gas-pressure-pump-system-examined", out var str,
                        ("statusColor", "lightblue"), // TODO: change with pressure?
                        ("pressure", pump.TargetPressure)
            ))
                args.PushMarkup(str);
        }

        private void OnPumpUpdated(EntityUid uid, GasPressurePumpComponent pump, AtmosDeviceUpdateEvent args)
        {
            if (!pump.Enabled
                || !EntityManager.TryGetComponent(uid, out NodeContainerComponent? nodeContainer)
                || !nodeContainer.TryGetNode(pump.InletName, out PipeNode? inlet)
                || !nodeContainer.TryGetNode(pump.OutletName, out PipeNode? outlet))
            {
                _ambientSoundSystem.SetAmbience(pump.Owner, false);
                return;
            }

            var outputStartingPressure = outlet.Air.Pressure;

            if (outputStartingPressure >= pump.TargetPressure)
            {
                _ambientSoundSystem.SetAmbience(pump.Owner, false);
                return; // No need to pump gas if target has been reached.
            }

            if (inlet.Air.TotalMoles > 0 && inlet.Air.Temperature > 0)
            {
                // We calculate the necessary moles to transfer using our good ol' friend PV=nRT.
                var pressureDelta = pump.TargetPressure - outputStartingPressure;
                var transferMoles = (pressureDelta * outlet.Air.Volume) / (inlet.Air.Temperature * Atmospherics.R);

                var removed = inlet.Air.Remove(transferMoles);
                _atmosphereSystem.Merge(outlet.Air, removed);
                _ambientSoundSystem.SetAmbience(pump.Owner, removed.TotalMoles > 0f);
            }
        }

        private void OnPumpLeaveAtmosphere(EntityUid uid, GasPressurePumpComponent pump, AtmosDeviceDisabledEvent args)
        {
            pump.Enabled = false;
            UpdateAppearance(uid, pump);

            DirtyUI(uid, pump);
            _userInterfaceSystem.TryCloseAll(uid, GasPressurePumpUiKey.Key);
        }

        private void OnPumpInteractHand(EntityUid uid, GasPressurePumpComponent pump, InteractHandEvent args)
        {
            if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
                return;

            if (EntityManager.GetComponent<TransformComponent>(pump.Owner).Anchored)
            {
                _userInterfaceSystem.TryOpen(uid, GasPressurePumpUiKey.Key, actor.PlayerSession);
                DirtyUI(uid, pump);
            }
            else
            {
                args.User.PopupMessageCursor(Loc.GetString("comp-gas-pump-ui-needs-anchor"));
            }

            args.Handled = true;
        }

        private void OnToggleStatusMessage(EntityUid uid, GasPressurePumpComponent pump, GasPressurePumpToggleStatusMessage args)
        {
            pump.Enabled = args.Enabled;
            var player = args.Session.AttachedEntity!.Value;
            _adminLogger.Add(LogType.AtmosPowerChanged, LogImpact.Medium,
                $"{ToPrettyString(player):player} set the power on {ToPrettyString(uid):device} to {args.Enabled}");
            if (_entityManager.GetComponent<MetaDataComponent>(uid).EntityName == "plasma pump" && args.Enabled)
                _chatManager.SendAdminAnnouncement(Loc.GetString("admin-chatalert-plasma-pump-enabled",
                    ("pump", ToPrettyString(uid)), ("player", ToPrettyString(player))));
            DirtyUI(uid, pump);
            UpdateAppearance(uid, pump);
        }

        private void OnOutputPressureChangeMessage(EntityUid uid, GasPressurePumpComponent pump, GasPressurePumpChangeOutputPressureMessage args)
        {
            pump.TargetPressure = Math.Clamp(args.Pressure, 0f, Atmospherics.MaxOutputPressure);
            var player = args.Session.AttachedEntity!.Value;
            _adminLogger.Add(LogType.AtmosPressureChanged, LogImpact.Medium,
                $"{ToPrettyString(player):player} set the pressure on {ToPrettyString(uid):device} to {args.Pressure}kPa");
            if (_entityManager.GetComponent<MetaDataComponent>(uid).EntityName == "plasma pump")
                _chatManager.SendAdminAnnouncement(Loc.GetString("admin-chatalert-plasma-pump-pressure-change",
                    ("pump", ToPrettyString(uid)), ("player", ToPrettyString(player)), ("pressure", args.Pressure)));
            DirtyUI(uid, pump);

        }

        private void DirtyUI(EntityUid uid, GasPressurePumpComponent? pump)
        {
            if (!Resolve(uid, ref pump))
                return;

            _userInterfaceSystem.TrySetUiState(uid, GasPressurePumpUiKey.Key,
                new GasPressurePumpBoundUserInterfaceState(EntityManager.GetComponent<MetaDataComponent>(pump.Owner).EntityName, pump.TargetPressure, pump.Enabled));
        }

        private void UpdateAppearance(EntityUid uid, GasPressurePumpComponent? pump = null, AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref pump, ref appearance, false))
                return;

            appearance.SetData(PumpVisuals.Enabled, pump.Enabled);
        }
    }
}
