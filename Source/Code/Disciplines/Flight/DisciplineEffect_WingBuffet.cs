using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_WingBuffet : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        GenExplosion.DoExplosion(target.PositionHeld, target.MapHeld, 1.9f, DamageDefOf.Stun, CasterPawn);
    }
}