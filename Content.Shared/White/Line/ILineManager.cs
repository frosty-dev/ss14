using Robust.Shared.Map;

namespace Content.Shared.White.Line;

public interface ILineManager<out TLine>
    where TLine : ILine
{
    IEnumerable<TLine> GetLines();
}

public interface IDynamicLineManager<out TLine, TLineSettings> : ILineManager<TLine>
    where TLine : IDynamicLine<TLineSettings>
{
    TLine Create(TLineSettings settings, MapId mapId);
    void Update(float dt);
}

