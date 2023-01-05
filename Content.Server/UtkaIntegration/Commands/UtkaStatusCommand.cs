using System.Net;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaStatusCommand : IUtkaCommand
{
    public string Name => "status";
    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args)
    {

    }
}
