using System.Net;
using Content.Server.Chat.Managers;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSendOOCMessage : IUtkaCommand
{
    public string Name => "ooc";
    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args)
    {
        var chatSystem = IoCManager.Resolve<IChatManager>();
        var finalMessage = string.Join(" ", message.Message!);
        chatSystem.SendHookOOC($"{message.Ckey}", $"{finalMessage}");
    }
}
