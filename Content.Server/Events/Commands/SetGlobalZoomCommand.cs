using System.Linq;
using System.Windows.Input;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Client.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SetGlobalZoomCommand : IConsoleCommand
{
    public string Command => "setglobalzoom";
    public string Description => "Sets the global zoom for all characters.";
    public string Help => "setglobalzoom <float>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if(args.Length != 1)
        {
            shell.WriteLine("Invalid number of arguments.");
            return;
        }

        if (!float.TryParse(args[0], out var zoom))
        {
            shell.WriteLine("Invalid zoom value.");
            return;
        }

        var entityManager = IoCManager.Resolve<EntityManager>();
        var eyes = entityManager.GetAllComponents(typeof(SharedEyeComponent), true).Cast<SharedEyeComponent>().ToList();

        foreach (var eye in eyes)
        {
            eye.Zoom = new Vector2(zoom, zoom);
            entityManager.Dirty(eye);
        }

    }
}
