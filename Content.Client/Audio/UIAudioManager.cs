using Content.Client.Reflection;
using Content.Shared.CCVar;
using Robust.Client.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.IoC.Exceptions;
using Robust.Shared.Player;
using Robust.Shared.Reflection;

namespace Content.Client.Audio;

public sealed class UIAudioManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IReflectionManager _refl = default!;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    ///     Audio systems are tied to <see cref="Robust.Shared.GameObjects.IEntitySystemManager"/> and the manager is intitialized only when the client is in single player or connected to a server.
    ///     (SO WE CAN'T HAVE AN AUDIO PLAYING IN THE MAIN MENU THANK YOU GOOD JOB RETARDS). This method handles audio system's resolving so it even does not fail any tests.
    /// </summary>
    /// <returns></returns>
    private SharedAudioSystem? GetAudioSystem()
    {
        // In integration tests we don't have initialized IEntitySystemManager and we have no reasons to have that.
        return _refl.IsInIntegrationTest() ? null : IoCManager.Resolve<IEntitySystemManager>().GetEntitySystemOrNull<SharedAudioSystem>();
    }

    public AudioSystem.PlayingStream? Play(string filename, AudioParams? audioParams = null)
    {
        if (GetAudioSystem() is not { } audio)
            return null;

        audioParams ??= AudioParams.Default;
        audioParams = audioParams.Value.WithVolume(audioParams.Value.Volume + _cfg.GetCVar(CCVars.UIVolume));

        return audio.PlayGlobal(filename, Filter.Local(), false, audioParams) as AudioSystem.PlayingStream;
    }
}
