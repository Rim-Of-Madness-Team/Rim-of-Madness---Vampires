using Verse;

namespace Vampire;

public class DisciplineEffect_BatForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_BatFormHediff, 1.0f);
    }
}