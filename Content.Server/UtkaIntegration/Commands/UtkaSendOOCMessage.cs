using Content.Server.Chat.Managers;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSendOOCMessage : IUtkaCommand
{
    public string Name => "OOC";
    public void Execute(UtkaSocket socket, FromDiscordMessage message, string[] args)
    {
        var chatSystem = IoCManager.Resolve<IChatManager>();

        chatSystem.SendHookOOC($"{message.Ckey}", $"{message.Message}");
    }
}
