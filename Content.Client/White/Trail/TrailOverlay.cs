using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.White.Trail;

public sealed class TrailOverlay : Overlay
{
    private readonly IPrototypeManager _protoManager;
    private readonly IResourceCache _cache;
    private readonly ITrailLineManager<ITrailLine> _lineManager;

    private readonly Dictionary<string, ShaderInstance?> _shaderDict;
    private readonly Dictionary<string, Texture?> _textureDict;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public TrailOverlay(
        IPrototypeManager protoManager,
        IResourceCache cache,
        ITrailLineManager<ITrailLine> lineManager
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
        foreach (var item in _lineManager.GetLines())
            ProcessTrailData(args.WorldHandle, item);
    }

    private void ProcessTrailData(DrawingHandleWorld handle, ITrailLine line)
    {
        var drawData = line.GetDrawData();
        if (!drawData.Any())
            return;

        var settings = line.Settings;

        var shader = settings.ShaderSettings != null ? GetCachedShader(settings.ShaderSettings.ShaderId) : null;
        if (shader != null)
        {
            handle.UseShader(shader);
        }

        var tex = GetCachedTexture(settings.TexurePath);
        if (tex != null)
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                var color = Color.InterpolateBetween(settings.ColorLifetimeMod, settings.ColorBase, cur.LifetimePercent);
                RenderTrailTexture(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2, tex, color);
                prev = cur;
            }
        }
        else
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                var color = Color.InterpolateBetween(settings.ColorLifetimeMod, settings.ColorBase, cur.LifetimePercent);
                RenderTrailColor(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2, color);
                prev = cur;
            }
        }

        handle.UseShader(null);

#if DEBUG
        if (false)
        {
            var prev = drawData.First();
            foreach (var cur in drawData.Skip(1))
            {
                //var color = Color.InterpolateBetween(settings.ColorLifetimeMod, settings.ColorBase, cur.LifetimePercent);
                RenderTrailDebugBox(handle, prev.Point1, prev.Point2, cur.Point1, cur.Point2);
                //handle.DrawLine(cur.Point1, cur.Point1 + cur.AngleLeft.RotateVec(Vector2.UnitX), Color.Red);
                prev = cur;
            }
        }
#endif
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

    private static void RenderTrailTexture(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2, Texture tex, Color color)
    {
        var verts = new DrawVertexUV2D[] {
            new (from1, Vector2.Zero),
            new (from2, Vector2.UnitY),
            new (to2, Vector2.One),
            new (to1, Vector2.UnitX),
        };

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, tex, verts, color);
    }

    private static void RenderTrailColor(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2,Color color)
    {
        var verts = new Vector2[] {
            from1,
            from2,
            to2,
            to1,
        };

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color);
    }

    private static void RenderTrailDebugBox(DrawingHandleBase handle, Vector2 from1, Vector2 from2, Vector2 to1, Vector2 to2)
    {
        handle.DrawLine(from1, from2, Color.Gray);
        handle.DrawLine(from1, to1, Color.Gray);
        handle.DrawLine(from2, to2, Color.Gray);
        handle.DrawLine(to1, to2, Color.Gray);
    }
}
