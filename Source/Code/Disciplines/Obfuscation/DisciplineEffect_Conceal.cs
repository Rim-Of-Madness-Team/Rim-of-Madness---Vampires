using Verse;

namespace Vampire;

public class DisciplineEffect_Conceal : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_InvisibilityHediff, 1.0f);
    }
}