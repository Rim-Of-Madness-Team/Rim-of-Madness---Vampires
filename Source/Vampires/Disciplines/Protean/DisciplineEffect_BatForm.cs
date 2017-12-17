using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Protean
{
    public class DisciplineEffect_BatForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(CasterPawn, VampDefOf.ROMV_BatFormHediff, 1.0f);
        }
    }
}
