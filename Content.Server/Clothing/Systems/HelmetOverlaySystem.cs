using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Gravity;
using Content.Server.Power.Components;
using Content.Server.Radiation.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory.Events;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Clothing.Systems;

public sealed class HelmetOverlaySystem : SharedHelmetOverlaySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;

    /// <summary>
    ///     Overlays which a somebody wearing.
    /// </summary>
    private readonly List<HelmetOverlayComponent> _overlayComponents = new();

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private TimeSpan _nextUpdate = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HelmetOverlayComponent, ComponentGetState>(OnGetState);

        SubscribeLocalEvent<GotEquippedEvent>(OnGotEquippedEvent);
        SubscribeLocalEvent<GotUnequippedEvent>(OnGotUnequippedEvent);
    }

    private void OnGetState(EntityUid uid, HelmetOverlayComponent component, ref ComponentGetState args)
    {
        args.State = new HelmetOverlayState
        {
            PressureSensor = component.PressureSensor,
            RadiationSensor = component.RadiationSensor,
            InternalsSensor = component.InternalsSensor,
            HealthSensor = component.HealthSensor,
            BatterySensor = component.BatterySensor,
            GravitySensor = component.GravitySensor,
            Pressure = component.Pressure,
            Radiation = component.Radiation,
            InternalsPressure = component.InternalsPressure,
            Health = component.Health,
            BatteryCharge = component.BatteryCharge,
            BatteryMaxCharge = component.BatteryMaxCharge,
            HasGravity = component.HasGravity,
            Texture = component.Texture,
            Equipee = component.Equipee,
            HasHud = component.HasHud
        };
    }

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(2);

        foreach (var component in _overlayComponents)
        {
            UpdateSensorsValue(component);
        }
    }

    private void UpdateSensorsValue(HelmetOverlayComponent component)
    {
        DebugTools.Assert(component.Equipee is not null);

        var xForm = Transform(component.Owner);
        var position = _transform.GetGridOrMapTilePosition(component.Owner, xForm);

        if (component.PressureSensor)
        {
            if (_atmos.GetTileMixture(xForm.GridUid, null, position) is not { } mixture)
            {
                component.Pressure = null;
                goto SkipPressure;
            }

            component.Pressure = mixture.Pressure;
        }

        SkipPressure:

        if (component.RadiationSensor)
        {
            var rad = EnsureComp<RadiationReceiverComponent>(component.Owner);

            component.Radiation = rad.CurrentRadiation;
        }

        if (component.BatterySensor)
        {
            if (!TryComp<BatteryComponent>(component.Owner, out var battery))
            {
                component.BatteryCharge = null;
                component.BatteryMaxCharge = null;
                goto SkipBattery;
            }

            component.BatteryCharge = battery.CurrentCharge;
            component.BatteryMaxCharge = battery.MaxCharge;
        }

        SkipBattery:

        if (component.InternalsSensor)
        {
            if (!TryComp<InternalsComponent>(component.Equipee, out var internals))
                goto SkipInternals;

            if (internals.GasTankEntity is null || !TryComp<GasTankComponent>(internals.GasTankEntity, out var tank))
            {
                component.InternalsPressure = null;
                goto SkipInternals;
            }

            component.InternalsPressure = tank.Air.Pressure;
        }

        SkipInternals:

        if (component.GravitySensor)
            component.HasGravity = !_gravity.IsWeightless(component.Equipee!.Value);


        Dirty(component);
    }

    private void OnGotEquippedEvent(GotEquippedEvent ev)
    {
        if (!TryGetOverlayComponent(ev.Equipment, out var component))
            return;

        if (!_overlayComponents.Contains(component))
            _overlayComponents.Add(component);

        if (component.Equipee != ev.Equipee)
        {
            component.Equipee = ev.Equipee;
            Dirty(component);
        }

        UpdateSensorsValue(component);
    }

    private void OnGotUnequippedEvent(GotUnequippedEvent ev)
    {
        if (!TryGetOverlayComponent(ev.Equipment, out var component))
            return;

        _overlayComponents.Remove(component);
        UpdateSensorsValue(component);
    }
}
