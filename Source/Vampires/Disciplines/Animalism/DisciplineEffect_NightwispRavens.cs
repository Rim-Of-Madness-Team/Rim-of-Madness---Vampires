using Vampire.Defs;
using Vampire.Utilities;
using Verse;

namespace Vampire.Disciplines.Animalism
{
    public class DisciplineEffect_NightwispRavens : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            VampireUtility.SummonEffect(target.PositionHeld, CasterPawn.Map, CasterPawn, 2f);

            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_NightwispRavens, 1.0f);
        }
    }
}