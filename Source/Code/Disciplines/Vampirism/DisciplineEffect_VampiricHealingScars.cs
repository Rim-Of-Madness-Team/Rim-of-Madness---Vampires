using Verse;

namespace Vampire;

public class DisciplineEffect_VampiricHealingScars : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        VampireUtility.Heal(target, 4, 2, true);
    }
}