using Verse;

namespace Vampire;

public class DisciplineEffect_Invisibility : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_InvisibilityHediff, 1.0f);
        //HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_InvisibilityHediff, 1.0f);
    }
}