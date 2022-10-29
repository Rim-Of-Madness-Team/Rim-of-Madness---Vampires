using Verse;

namespace Vampire;

public class DisciplineEffect_MistForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_MistFormHediff, 1.0f);
    }
}