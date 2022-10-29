using Verse;

namespace Vampire;

public class DisciplineEffect_CrocodileTongue : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_CrocodileTongueHediff, 1.0f);
    }
}