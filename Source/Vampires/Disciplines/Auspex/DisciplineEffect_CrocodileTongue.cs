using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Auspex
{
    public class DisciplineEffect_CrocodileTongue : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_CrocodileTongueHediff, 1.0f);
        }
    }
}