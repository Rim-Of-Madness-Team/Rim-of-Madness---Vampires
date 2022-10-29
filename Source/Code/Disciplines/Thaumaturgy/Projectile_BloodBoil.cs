using System.Linq;
using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class Projectile_BloodBoil : Projectile_AbilityBase
{
    public override void Impact_Override(Thing hitThing)
    {
        base.Impact_Override(hitThing);
        if (hitThing is Pawn p && p.BloodNeed() is { } bn)
        {
            GenExplosion.DoExplosion(p.PositionHeld, p.MapHeld, 3.9f, DamageDefOf.Flame, p);
            bn.AdjustBlood(-7);
            var num = GenRadial.NumCellsInRadius(3.9f);
            for (var i = 0; i < num; i++)
                FilthMaker.TryMakeFilth(hitThing.PositionHeld + GenRadial.RadialPattern[i], hitThing.MapHeld,
                    ((Pawn)hitThing).RaceProps.BloodDef, ((Pawn)hitThing).LabelIndefinite());
            var parts = p.health.hediffSet.GetNotMissingParts().ToList().FindAll(x => x.depth == BodyPartDepth.Inside);
            for (var j = 0; j < 4; j++)
                if (!p.Dead)
                    p.TakeDamage(new DamageInfo(DamageDefOf.Burn, Rand.Range(8, 13), 1f, -1, Caster,
                        parts.RandomElement()));
        }
    }
}