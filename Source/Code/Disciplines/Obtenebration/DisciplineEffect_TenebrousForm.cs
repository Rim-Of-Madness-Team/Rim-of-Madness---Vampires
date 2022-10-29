using Verse;

namespace Vampire;

public class DisciplineEffect_TenebrousForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_TenebrousFormHediff, 1.0f);
    }
}