using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Vicissitude
{
    public class DisciplineEffect_PerfectForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_PerfectFormHediff, 1.0f);
        }
    }
}
