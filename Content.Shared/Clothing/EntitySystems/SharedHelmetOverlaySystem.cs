using System.Diagnostics.CodeAnalysis;
using Content.Shared.Clothing.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.EntitySystems;

public abstract class SharedHelmetOverlaySystem : EntitySystem
{
    protected bool TryGetOverlayComponent(EntityUid equipment, [NotNullWhen(true)] out HelmetOverlayComponent? component)
    {
        component = CompOrNull<HelmetOverlayComponent>(equipment);

        return component is not null;
    }
}
