using System.Diagnostics.CodeAnalysis;
using Content.Client.Audio;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio;
using Robust.Shared.IoC.Exceptions;
using TerraFX.Interop.Windows;

namespace Content.Client.UserInterface.Controls;

/// <summary>
///     Just a <see cref="Robust.Client.UserInterface.Controls.Button"/> but with sound.
/// </summary>
[Virtual]
public class CommonButton : Button
{
    [Dependency] private readonly UIAudioManager _audio = default!;

    public CommonButton()
    {
        IoCManager.InjectDependencies(this);
        OnMouseEntered += PlayHoverSound;
        OnPressed += PlayPressedSound;
    }

    private void PlayHoverSound(GUIMouseHoverEventArgs _)
    {
        _audio.Play("/Audio/UI/hover.ogg", AudioParams.Default.WithVolume(-10f));
    }

    private void PlayPressedSound(ButtonEventArgs _)
    {
        _audio.Play("/Audio/UI/pressed.ogg", AudioParams.Default.WithVolume(-5f));
    }
}
