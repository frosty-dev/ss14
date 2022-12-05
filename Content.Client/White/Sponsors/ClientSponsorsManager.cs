using Content.Shared.White.Sponsors;
using Robust.Shared.Network;

namespace Content.Client.White.Sponsors;

public sealed class ClientSponsorsManager : SponsorsManager
{
    [Dependency] private readonly IClientNetManager _netMgr = default!;

    public bool IsSponsor = default!;
    public bool AllowedNeko = default!;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgSponsoringInfo>(HandleSponsoringInfo);
    }

    private void HandleSponsoringInfo(MsgSponsoringInfo message)
    {
        IsSponsor = message.IsSponsor;
        AllowedNeko = message.AllowedNeko;
    }
}
