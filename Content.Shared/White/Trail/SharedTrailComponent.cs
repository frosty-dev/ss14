using Content.Shared.White.Spline;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[NetworkedComponent()]
public abstract class SharedTrailComponent : Component, ITrailSettings
{
    private Vector2 _gravity;
    private float _lifetime;
    private Vector2 _maxRandomWalk;
    private Vector2 _scale;
    private string? _texurePath;
    private Vector2 _creationOffset;
    private float _сreationDistanceThresholdSquared;
    private SegmentCreationMethod _сreationMethod;
    private List<Color>? _gradient;
    private float _lengthStep;
    private Spline2DType _splineIteratorType;
    private TrailSplineRendererType _splineRendererType;

    protected SharedTrailComponent()
    {
        var defaultTrail = TrailSettings.Default;
        _scale = defaultTrail.Scale;
        _сreationDistanceThresholdSquared = defaultTrail.СreationDistanceThresholdSquared;
        _сreationMethod = defaultTrail.СreationMethod;
        _creationOffset = defaultTrail.CreationOffset;
        _gravity = defaultTrail.Gravity;
        _maxRandomWalk = defaultTrail.MaxRandomWalk;
        _lifetime = defaultTrail.Lifetime;
        _texurePath = defaultTrail.TexurePath;
        //_gradient = defaultTrail.Gradient;
    }

    [DataField("gravity")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Gravity
    {
        get => _gravity;
        set
        {
            if (_gravity == value)
                return;
            _gravity = value;
            Dirty();
        }
    }

    [DataField("lifetime", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Lifetime
    {
        get => _lifetime;
        set
        {
            if (_lifetime == value)
                return;
            _lifetime = value;
            Dirty();
        }
    }

    [DataField("randomWalk")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 MaxRandomWalk
    {
        get => _maxRandomWalk;
        set
        {
            if (_maxRandomWalk == value)
                return;
            _maxRandomWalk = value;
            Dirty();
        }
    }

    [DataField("scale", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Scale
    {
        get => _scale;
        set
        {
            if (_scale == value)
                return;
            _scale = value;
            Dirty();
        }
    }

    [DataField("texturePath")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? TexurePath
    {
        get => _texurePath;
        set
        {
            if (_texurePath == value)
                return;
            _texurePath = value;
            Dirty();
        }
    }

    [DataField("creationOffset")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 CreationOffset
    {
        get => _creationOffset;
        set
        {
            if (_creationOffset == value)
                return;
            _creationOffset = value;
            Dirty();
        }
    }

    [DataField("сreationDistanceThresholdSquared")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float СreationDistanceThresholdSquared
    {
        get => _сreationDistanceThresholdSquared;
        set
        {
            if (_сreationDistanceThresholdSquared == value)
                return;
            _сreationDistanceThresholdSquared = value;
            Dirty();
        }
    }

    [DataField("creationMethod")]
    [ViewVariables(VVAccess.ReadWrite)]
    public SegmentCreationMethod СreationMethod
    {
        get => _сreationMethod;
        set
        {
            if (_сreationMethod == value)
                return;
            _сreationMethod = value;
            Dirty();
        }
    }

    [DataField("gradient", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<Color>? Gradient
    {
        get => _gradient;
        set
        {
            if (_gradient == value)
                return;
            _gradient = value;
            Dirty();
        }
    }

    [DataField("lengthStep")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float LengthStep
    {
        get => _lengthStep;
        set
        {
            if (_lengthStep == value)
                return;
            _lengthStep = value;
            Dirty();
        }
    }

    [DataField("splineIteratorType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual Spline2DType SplineIteratorType
    {
        get => _splineIteratorType;
        set
        {
            if (_splineIteratorType == value)
                return;
            _splineIteratorType = value;
            Dirty();
        }
    }
    [DataField("splineRendererType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual TrailSplineRendererType SplineRendererType
    {
        get => _splineRendererType;
        set
        {
            if (_splineRendererType == value)
                return;
            _splineRendererType = value;
            Dirty();
        }
    }
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
