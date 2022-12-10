using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Trail;

[NetworkedComponent()]
public abstract class SharedTrailComponent : Component, ITrailSettings
{
    private Color _colorBase = Color.White;
    private Color _colorLifetimeMod = Color.Transparent;
    private Vector2 _gravity = new Vector2(0.05f, 0.05f);
    private float _lifetime = 1f;
    private Vector2 _maxRandomWalk = new Vector2(0.005f, 0.005f);
    private Vector2 _offset = new Vector2(0.5f, 0.0f);
    private string? _texurePath;
    private float _сreationDistanceThresholdSquared = 0.001f;
    private SegmentCreationMethod _сreationMethod = SegmentCreationMethod.OnFrameUpdate;

    [DataField("colorBase")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Color ColorBase
    {
        get => _colorBase;
        set
        {
            if (_colorBase == value)
                return;
            _colorBase = value;
            Dirty();
        }
    }
    [DataField("colorLifetimeMod")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Color ColorLifetimeMod
    {
        get => _colorLifetimeMod;
        set
        {
            if (_colorLifetimeMod == value)
                return;
            _colorLifetimeMod = value;
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
    [DataField("offset", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Offset
    {
        get => _offset;
        set
        {
            if (_offset == value)
                return;
            _offset = value;
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
            Offset = Offset,
            СreationDistanceThresholdSquared = СreationDistanceThresholdSquared,
            СreationMethod = СreationMethod,
            Gravity = Gravity,
            MaxRandomWalk = MaxRandomWalk,
            Lifetime = Lifetime,
            TexurePath = TexurePath,
            ColorBase = ColorBase,
            ColorLifetimeMod = ColorLifetimeMod,
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
