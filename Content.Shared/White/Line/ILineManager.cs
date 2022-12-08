using Robust.Shared.Map;

namespace Content.Shared.White.Line;

public interface IDynamicLineManager<out TLine, TLineSettings>
    where TLine : IDynamicLine<TLineSettings>
{
    TLine Create(TLineSettings settings, MapId mapId);
    void Update(float dt);
}

