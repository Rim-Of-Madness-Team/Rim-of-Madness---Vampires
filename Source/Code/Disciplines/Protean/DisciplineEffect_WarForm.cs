using Verse;

namespace Vampire;

public class DisciplineEffect_WarForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_WarFormHediff, 1.0f);
    }
}