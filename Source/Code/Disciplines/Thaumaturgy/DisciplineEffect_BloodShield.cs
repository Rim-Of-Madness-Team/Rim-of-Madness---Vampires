using Verse;

namespace Vampire;

public class DisciplineEffect_BloodShield : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_BloodShieldHediff, 1.0f);
        if (target.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROMV_BloodShieldHediff) is { } hd &&
            hd.TryGetComp<HediffComp_Shield>() is { } shield)
            shield.NotifyRefilled();
    }
}