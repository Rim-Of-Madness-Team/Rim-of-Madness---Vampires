using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public class Projectile_BloodStealer : Projectile_AbilityBase
    {
        public override void Impact_Override(Thing hitThing)
        {
            base.Impact_Override(hitThing);

            if (hitThing is Pawn p && p?.BloodNeed() is Need_Blood bn && p.MapHeld != null)
            {
                MoteMaker.ThrowText(p.DrawPos, p.MapHeld, "-2", -1f);
                bn.AdjustBlood(-2);
                if (p.MapHeld != null && p.PositionHeld.IsValid)
                {
                    Projectile_BloodReturner projectile =
                        (Projectile_BloodReturner)GenSpawn.Spawn(ThingDef.Named("ROMV_BloodProjectile_Returner"), hitThing.PositionHeld, hitThing.MapHeld);
                    projectile.Launch(hitThing, this.origin.ToIntVec3(), null);
                }
            }
        }
        
    }
}
