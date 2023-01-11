namespace Content.Server.Singularity.Components;

[RegisterComponent]
public sealed class SingularityDestroyerComponent : Component
{
    [DataField("active")] [ViewVariables(VVAccess.ReadWrite)]
    public bool Active = true;
}
