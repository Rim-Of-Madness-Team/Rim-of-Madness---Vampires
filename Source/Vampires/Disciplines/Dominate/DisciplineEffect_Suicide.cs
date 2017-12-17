using Verse;

namespace Vampire.Disciplines.Dominate
{
    public class DisciplineEffect_Suicide : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, HediffDef.Named("HeartAttack"), 1.0f);
        }
    }
}
