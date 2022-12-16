using Content.Shared.White.LambdaParser;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[NetworkedComponent()]
public abstract class SharedTrailComponent : Component, ITrailSettings
{
    [NonSerialized]
    private Func<float, float>? _colorLifetimeDeltaLambda;

    private Color _colorLifetimeStart;
    private Color _colorLifetimeEnd;
    private string? _colorLifetimeDeltaLambdaOperations;
    private Vector2 _gravity;
    private float _lifetime;
    private Vector2 _maxRandomWalk;
    private Vector2 _scale;
    private string? _texurePath;
    private Vector2 _creationOffset;
    private float _сreationDistanceThresholdSquared;
    private SegmentCreationMethod _сreationMethod;
    private TrailLineType _сreatedTrailType;

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
        _colorLifetimeStart = defaultTrail.ColorLifetimeStart;
        _colorLifetimeEnd = defaultTrail.ColorLifetimeEnd;
        _colorLifetimeDeltaLambdaOperations = defaultTrail.ColorLifetimeDeltaLambdaOperations;
        _сreatedTrailType = defaultTrail.CreatedTrailType;
    }

    public Func<float, float>? ColorLifetimeDeltaLambda => _colorLifetimeDeltaLambda;

    [DataField("colorLifetimeStart")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Color ColorLifetimeStart
    {
        get => _colorLifetimeStart;
        set
        {
            if (_colorLifetimeStart == value)
                return;
            _colorLifetimeStart = value;
            Dirty();
        }
    }
    [DataField("colorLifetimeEnd")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Color ColorLifetimeEnd
    {
        get => _colorLifetimeEnd;
        set
        {
            if (_colorLifetimeEnd == value)
                return;
            _colorLifetimeEnd = value;
            Dirty();
        }
    }

    [DataField("colorLifetimeDeltaLambdaOperations")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? ColorLifetimeDeltaLambdaOperations
    {
        get => _colorLifetimeDeltaLambdaOperations;
        set
        {
            if (_colorLifetimeDeltaLambdaOperations == value)
                return;
            _colorLifetimeDeltaLambdaOperations = value;
            _colorLifetimeDeltaLambda = MathsLambdaParser.ToFloatLambda(value);

            Dirty();
        }
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

    [DataField("trailType")]
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual TrailLineType CreatedTrailType
    {
        get => _сreatedTrailType;
        set
        {
            if (_сreatedTrailType == value)
                return;
            _сreatedTrailType = value;
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
