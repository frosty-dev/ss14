using Content.Shared.Alert;
using Content.Shared.StatusEffect;
using Content.Shared.Database;

namespace Content.Server.MobState;

public sealed partial class MobStateSystem
{

    public override void EnterDeadState(EntityUid uid)
    {
        base.EnterDeadState(uid);

        Alerts.ShowAlert(uid, AlertType.HumanDead);
        _adminLogger.Add(LogType.Damaged, LogImpact.Extreme, $"{ToPrettyString(uid):uid} is DEAD!!!");
        if (HasComp<StatusEffectsComponent>(uid))
        {
            Status.TryRemoveStatusEffect(uid, "Stun");
        }
    }
}
