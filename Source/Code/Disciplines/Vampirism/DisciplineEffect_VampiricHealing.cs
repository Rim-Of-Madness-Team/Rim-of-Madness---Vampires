using Verse;

namespace Vampire;

public class DisciplineEffect_VampiricHealing : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        VampireGen.RemoveMortalHediffs(target);
        VampireUtility.Heal(target);
    }
}