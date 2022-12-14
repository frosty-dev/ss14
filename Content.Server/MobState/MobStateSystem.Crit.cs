using Content.Shared.StatusEffect;
using Content.Shared.Database;

namespace Content.Server.MobState;

public sealed partial class MobStateSystem
{

    public override void EnterCritState(EntityUid uid)
    {
        base.EnterCritState(uid);
        _adminLogger.Add(LogType.Damaged, LogImpact.High, $"{ToPrettyString(uid):uid} ENTERED in СRIT State!");
        if (HasComp<StatusEffectsComponent>(uid))
        {
            Status.TryRemoveStatusEffect(uid, "Stun");
        }
    }

    public override void ExitCritState(EntityUid uid)
    {
        _adminLogger.Add(LogType.Damaged, LogImpact.High, $"{ToPrettyString(uid):user} EXIT from СRIT State!");
    }
}
