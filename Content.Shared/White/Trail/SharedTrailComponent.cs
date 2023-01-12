using Content.Shared.White.Spline;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[NetworkedComponent()]
public abstract class SharedTrailComponent : Component, ITrailSettings
{
    [DataField("gravity")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Vector2 Gravity { get; set; }

    [DataField("lifetime", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual float Lifetime { get; set; }

    [DataField("lengthStep")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual float LengthStep { get; set; }

    [DataField("randomWalk")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Vector2 MaxRandomWalk { get; set; }

    [DataField("scale", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Vector2 Scale { get; set; }

    [DataField("texturePath")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual string? TexurePath { get; set; }

    [DataField("creationOffset")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Vector2 CreationOffset { get; set; }

    [DataField("сreationDistanceThresholdSquared")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual float СreationDistanceThresholdSquared { get; set; }

    [DataField("creationMethod")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual SegmentCreationMethod СreationMethod { get; set; }

    [DataField("gradient", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Vector4[] Gradient { get; set; } = new Vector4[] { Vector4.One, new Vector4(1f, 1f, 1f, 0f) };

    [DataField("gradientIteratorType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Spline4DType GradientIteratorType { get; set; }

    [DataField("splineIteratorType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Spline2DType SplineIteratorType { get; set; }

    [DataField("splineRendererType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual TrailSplineRendererType SplineRendererType { get; set; }
}

[Serializable, NetSerializable]
public sealed class TrailComponentState : ComponentState
{
    public TrailSettings Settings;

    public TrailComponentState(TrailSettings settings)
    {
        Settings = settings;
    }
}
