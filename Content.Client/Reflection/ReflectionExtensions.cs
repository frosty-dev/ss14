using System.Linq;
using Robust.Client.Reflection;
using Robust.Shared.Reflection;

namespace Content.Client.Reflection;

public static class ReflectionExtensions
{
    /// <summary>
    ///     A poor but working solution to tweak a behaviour in some places without editing engine's code.
    ///     <see cref="Content.Client.Audio.UIAudioManager"/> is one of that places.
    /// </summary>
    /// <returns>Returns true if we running some integration tests.</returns>
    public static bool IsInIntegrationTest(this IReflectionManager refl)
    {
        return refl.Assemblies.Any(a => a.FullName != null && a.FullName.Contains("IntegrationTests"));
    }
}
