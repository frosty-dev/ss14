using Content.Shared.White.Spline;
using Content.Shared.White.Trail;

namespace Content.Server.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent
{
    private Vector2 _gravity;
    private float _lifetime;
    private Vector2 _maxRandomWalk;
    private Vector2 _scale;
    private string? _texurePath;
    private Vector2 _creationOffset;
    private float _сreationDistanceThresholdSquared;
    private SegmentCreationMethod _сreationMethod;
    private Vector4[] _gradient;
    private float _lengthStep;
    private Spline2DType _splineIteratorType;
    private TrailSplineRendererType _splineRendererType;
    private Spline4DType _gradientIteratorType;

    public TrailComponent()
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
        _gradient = defaultTrail.Gradient;
        _gradientIteratorType = defaultTrail.GradientIteratorType;
    }

    public override Vector2 Gravity
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

    public override float Lifetime
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

    public override Vector2 MaxRandomWalk
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

    public override Vector2 Scale
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

    public override string? TexurePath
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

    public override Vector2 CreationOffset
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

    public override float СreationDistanceThresholdSquared
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

    public override SegmentCreationMethod СreationMethod
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

    public override Vector4[] Gradient
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

    public override float LengthStep
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

    public override Spline2DType SplineIteratorType
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

    public override TrailSplineRendererType SplineRendererType
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

    public override Spline4DType GradientIteratorType
    {
        get => _gradientIteratorType;
        set
        {
            if (_gradientIteratorType == value)
                return;
            _gradientIteratorType = value;
            Dirty();
        }
    }
}
