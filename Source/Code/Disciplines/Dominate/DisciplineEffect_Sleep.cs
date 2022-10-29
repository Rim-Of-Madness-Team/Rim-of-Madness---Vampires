using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_Sleep : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_SleepHediff, 1.0f);
        FleckMaker.ThrowMetaIcon(target.Position, target.Map, FleckDefOf.SleepZ);
    }
}