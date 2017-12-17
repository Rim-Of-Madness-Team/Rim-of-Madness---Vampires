using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Vicissitude
{
    public class DisciplineEffect_ZuloForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_ZuloFormHediff, 1.0f);
        }
    }
}
