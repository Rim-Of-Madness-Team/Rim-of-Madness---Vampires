using Verse;

namespace Vampire;

public class DisciplineEffect_HiddenForce : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_HiddenForceHediff, 1.0f);
    }
}