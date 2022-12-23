using Content.Client.White.Trail.Line.Manager;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Trail;

public sealed class TrailOverlay : Overlay
{
    private readonly IPrototypeManager _protoManager;
    private readonly IResourceCache _cache;
    private readonly ITrailLineManager _lineManager;

    private readonly Dictionary<string, ShaderInstance?> _shaderDict;
    private readonly Dictionary<string, Texture?> _textureDict;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public TrailOverlay(
        IPrototypeManager protoManager,
        IResourceCache cache,
        ITrailLineManager lineManager
        )
    {
        _protoManager = protoManager;
        _cache = cache;
        _lineManager = lineManager;

        _shaderDict = new();
        _textureDict = new();

        ZIndex = (int) Shared.DrawDepth.DrawDepth.Effects;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        foreach (var item in _lineManager.Lines)
            item.Render(handle, GetCachedTexture(item.Settings.TexurePath ?? ""));
    }

    //влепить на ети два метода мемори кеш со слайдинг експирейшоном вместо дикта если проблемы будут
    private ShaderInstance? GetCachedShader(string id)
    {
        ShaderInstance? shader;
        if (_shaderDict.TryGetValue(id, out shader))
            return shader;
        if (_protoManager.TryIndex<ShaderPrototype>(id, out var shaderRes))
            shader = shaderRes?.InstanceUnique();
        _shaderDict.Add(id, shader);
        return shader;
    }

    private Texture? GetCachedTexture(string path)
    {
        Texture? texture;
        if (_textureDict.TryGetValue(path, out texture))
            return texture;
        if (_cache.TryGetResource<TextureResource>(path, out var texRes))
            texture = texRes;
        _textureDict.Add(path, texture);
        return texture;
    }
}
