using Verse;

namespace Vampire;

public class DisciplineEffect_Hide : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_HideHediff, 1.0f);
    }
}