using System.Linq;
using System.Net;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Utility;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaAssayCommand : IUtkaCommand
{
    public string Name => "asay";
    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args)
    {
        var ckey = message.Ckey;
        var content = string.Join(" ", args);

        var chatSystem = IoCManager.Resolve<IChatManager>();
        var adminManager = IoCManager.Resolve<IAdminManager>();

        chatSystem.TrySendOOCMessage(null!, content, OOCChatType.Admin);

        var clients = adminManager.ActiveAdmins.Select(p => p.ConnectedClient);

        var wrappedMessage = Loc.GetString("chat-manager-send-admin-chat-wrap-message",
            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
            ("playerName", ckey!), ("message", FormattedMessage.EscapeText(content!)));

         chatSystem.ChatMessageToMany(ChatChannel.Admin, content, wrappedMessage, default, false, true, clients.ToList());
    }
}
