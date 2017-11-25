using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public class Projectile_BloodBoil : Projectile_AbilityBase
    {
        public override void Impact_Override(Thing hitThing)
        {
            base.Impact_Override(hitThing);
            if (hitThing is Pawn p && p.BloodNeed() is Need_Blood bn)
            {
                GenExplosion.DoExplosion(p.PositionHeld, p.MapHeld, 3.9f, DamageDefOf.Flame, p);
                bn.AdjustBlood(-7);
                int num = GenRadial.NumCellsInRadius(3.9f);
                for (int i = 0; i < num; i++)
                {
                    FilthMaker.MakeFilth(hitThing.PositionHeld + GenRadial.RadialPattern[i], hitThing.MapHeld, ((Pawn)hitThing).RaceProps.BloodDef, ((Pawn)hitThing).LabelIndefinite(), 1);
                }
                List<BodyPartRecord> parts = p.health.hediffSet.GetNotMissingParts().ToList().FindAll(x => x.depth == BodyPartDepth.Inside);
                for (int j = 0; j < 4; j++)
                {
                    if (!p.Dead)
                        p.TakeDamage(new DamageInfo(DamageDefOf.Burn, Rand.Range(8, 13), -1, this.Caster, parts.RandomElement()));
                }
            }
        }
    }
}
