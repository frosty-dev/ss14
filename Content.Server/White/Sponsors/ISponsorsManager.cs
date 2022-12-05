using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Network;

namespace Content.Server.White.Sponsors;

public interface ISponsorsManager
{
    void Initialize();

    /**
     * Gets the cached color of the players OOC if he is a sponsor
     */
    ISponsor? GetSponsorInfo(NetUserId userId);
}

public interface ISponsor
{
    int? Tier { get; }
    string? OOCColor { get; }
    bool HavePriorityJoin { get; }
}
