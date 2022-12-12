using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Components;

/// <summary>
///     Interact with to start piloting a shuttle.
/// </summary>
[NetworkedComponent]
[RegisterComponent]
public sealed class ShuttleConsoleComponent : Component
{
    [ViewVariables] public readonly List<PilotComponent> SubscribedPilots = new();

    /// <summary>
    ///     How much should the pilot's eye be zoomed by when piloting using this console?
    /// </summary>
    [DataField("zoom")] private Vector2 _zoom = new(1.5f, 1.5f);

    /// <summary>
    ///     How much should the pilot's eye be zoomed by when piloting using this console?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            Dirty();
        }
    }
}

[Serializable]
[NetSerializable]
public enum ShuttleConsoleUiKey : byte
{
    Key
}
