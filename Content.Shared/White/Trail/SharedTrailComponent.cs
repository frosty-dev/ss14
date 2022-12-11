using Content.Shared.White.LambdaParser;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[NetworkedComponent()]
public abstract class SharedTrailComponent : Component, ITrailSettings
{
    private Color _colorLifetimeStart = Color.White;
    private Color _colorLifetimeEnd = Color.Transparent;
    private string? _colorLifetimeDeltaLambdaOperations;
    [NonSerialized]
    private Func<float, float>? _colorLifetimeDeltaLambda;
    private Vector2 _gravity = new Vector2(0.05f, 0.05f);
    private float _lifetime = 1f;
    private Vector2 _maxRandomWalk = new Vector2(0.005f, 0.005f);
    private float _width = 0.5f;
    private string? _texurePath;
    private Vector2 _creationOffset;
    private float _сreationDistanceThresholdSquared = 0.001f;
    private SegmentCreationMethod _сreationMethod = SegmentCreationMethod.OnFrameUpdate;

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

    public Func<float, float>? ColorLifetimeDeltaLambda => _colorLifetimeDeltaLambda;

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

    [DataField("width", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Width
    {
        get => _width;
        set
        {
            if (_width == value)
                return;
            _width = value;
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

    [DataField("сreationMethod")]
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

    public TrailSettings ToTrailSettings()
        => new()
        {
            Width = Width,
            СreationDistanceThresholdSquared = СreationDistanceThresholdSquared,
            СreationMethod = СreationMethod,
            CreationOffset = CreationOffset,
            Gravity = Gravity,
            MaxRandomWalk = MaxRandomWalk,
            Lifetime = Lifetime,
            TexurePath = TexurePath,
            ColorLifetimeStart = ColorLifetimeStart,
            ColorLifetimeEnd = ColorLifetimeEnd,
            ColorLifetimeDeltaLambdaOperations = ColorLifetimeDeltaLambdaOperations
        };
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
