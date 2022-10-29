using Verse;

namespace Vampire;

public class DisciplineEffect_ZuloForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_ZuloFormHediff, 1.0f);
    }
}