using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Chat.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SlangSanitizationCommand : IConsoleCommand
{
    public string Command => "enableSlangSanitization";
    public string Description => "Toggles the slang sanitization.";
    public string Help => "enableSlangSanitization <bool>";

    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !bool.TryParse(args[0], out bool value))
        {
            shell.WriteError($"{args[0]} is not a valid boolean.");
            return;
        }

        _cfg.SetCVar(CCVars.ChatSlangFilter, value);

        var announce = Loc.GetString("chatsan-announce-slang-sanitization",
            ("admin", $"{shell.Player?.Name}"),
            ("value", $"{value}"));

        IoCManager.Resolve<IChatManager>().DispatchServerAnnouncement(announce, Color.Red);
    }
}
