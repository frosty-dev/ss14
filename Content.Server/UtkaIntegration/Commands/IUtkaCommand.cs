using System.Net;

namespace Content.Server.UtkaIntegration;

public interface IUtkaCommand
{
    string Name { get; }
    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args);
}
