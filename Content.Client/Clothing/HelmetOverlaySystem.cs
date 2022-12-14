using Content.Client.Clothing.Overlays;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Client.Clothing;

public sealed class HelmetOverlaySystem : SharedHelmetOverlaySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private HelmetOverlayComponent? _component;

    private HelmetOverlay? _overlay;

    public override void Initialize()
    {
        SubscribeLocalEvent<HelmetOverlayComponent, ComponentHandleState>(OnHandleState);

        SubscribeLocalEvent<GotEquippedEvent>(OnGotEquippedEvent);
        SubscribeLocalEvent<GotUnequippedEvent>(OnGotUnequippedEvent);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        if (_overlay is null)
            return;

        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        if (_overlay is null)
        {
            return;
        }

        if (_component is null)
        {
            DisposeOverlay();
            return;
        }

        if (!TryComp(args.Entity, out InventoryComponent? inventory))
        {
            _overlayManager.RemoveOverlay(_overlay);
            return;
        }

        if (!_inventory.TryGetSlots(args.Entity, out var slots, inventory))
        {
            DisposeOverlay();
            return;
        }

        foreach (var slot in slots)
        {
            if (!_inventory.TryGetSlotContainer(args.Entity, slot.Name, out var containerSlot, out _,
                    inventory))
                continue;

            if (!containerSlot.Contains(_component.Owner))
                continue;

            _overlayManager.AddOverlay(_overlay);
            return;
        }

        DisposeOverlay();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_overlay is null)
            return;

        _overlayManager.RemoveOverlay(_overlay);
        _overlay.Dispose();
        _overlay = null;
    }

    private void OnHandleState(EntityUid uid, HelmetOverlayComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not HelmetOverlayState state)
            return;

        component.Texture = state.Texture;
        component.PressureSensor = state.PressureSensor;
        component.RadiationSensor = state.RadiationSensor;
        component.InternalsSensor = state.InternalsSensor;
        component.HealthSensor = state.HealthSensor;
        component.BatterySensor = state.BatterySensor;
        component.GravitySensor = state.GravitySensor;
        component.Pressure = state.Pressure;
        component.Radiation = state.Radiation;
        component.InternalsPressure = state.InternalsPressure;
        component.Health = state.Health;
        component.BatteryCharge = state.BatteryCharge;
        component.BatteryMaxCharge = state.BatteryMaxCharge;
        component.HasGravity = state.HasGravity;
        component.Equipee = state.Equipee;
        component.HasHud = state.HasHud;

        if (component.Owner != _component?.Owner)
            return;

        // Say to overlay about the new data.
        _overlay?.SetState(state);
    }

    private void OnGotEquippedEvent(GotEquippedEvent ev)
    {
        if (!IsPlayerControlledEntity(ev.Equipee) || !TryGetOverlayComponent(ev.Equipment, out var component))
            return;

        // Check if we trying to wear multiple overlays.
        DebugTools.Assert(_component is null || _component is not null && _component.Owner == component.Owner);

        _component = component;
        EnsureHasOverlay();
    }

    private void OnGotUnequippedEvent(GotUnequippedEvent ev)
    {
        if (!IsPlayerControlledEntity(ev.Equipee))
            return;

        if (_component is not null && _component.Owner != ev.Equipment)
            return;

        DisposeOverlay();
    }

    private void DisposeOverlay()
    {
        if (_overlay is null)
            return;

        _overlayManager.RemoveOverlay(_overlay);
        _overlay.Dispose();
        _overlay = null;
    }

    private bool IsPlayerControlledEntity(EntityUid uid)
    {
        if (_playerManager.LocalPlayer is not { } player)
            return false;

        return uid == player.ControlledEntity;
    }

    private void EnsureHasOverlay()
    {
        _overlay ??= new HelmetOverlay();

        if (!_overlayManager.HasOverlay(typeof(HelmetOverlay)))
            _overlayManager.AddOverlay(_overlay);
        else
            _overlayManager.AddOverlay(_overlay);
    }
}
