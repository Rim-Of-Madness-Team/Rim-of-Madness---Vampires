using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Obtenebration
{
    public class DisciplineEffect_TenebrousForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_TenebrousFormHediff, 1.0f);
        }
    }
}
