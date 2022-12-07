using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.GameTicking;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class RoundId : IConsoleCommand
    {
        public string Command => "roundid";
        public string Description => "Shows the id of the current round.";
        public string Help => "Output RoundID + #roundID";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            shell.WriteLine($"RoundID #{EntitySystem.Get<GameTicker>().RoundId}");
        }
    }
}
