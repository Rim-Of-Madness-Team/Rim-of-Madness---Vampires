using Vampire.Utilities;
using Verse;

namespace Vampire.Disciplines.Vampirism
{
    public class DisciplineEffect_VampiricHealing : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            VampireUtility.Heal(target);
        }
    }
}
