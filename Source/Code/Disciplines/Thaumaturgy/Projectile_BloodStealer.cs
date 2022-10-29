using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class Projectile_BloodStealer : Projectile_AbilityBase
{
    public override void Impact_Override(Thing hitThing)
    {
        base.Impact_Override(hitThing);

        if (hitThing is Pawn p && p?.BloodNeed() is { } bn && p.MapHeld != null)
        {
            MoteMaker.ThrowText(p.DrawPos, p.MapHeld, "-2");
            bn.AdjustBlood(-2);
            if (p.MapHeld != null && p.PositionHeld.IsValid)
            {
                var projectile =
                    (Projectile_BloodReturner)GenSpawn.Spawn(ThingDef.Named("ROMV_BloodProjectile_Returner"),
                        hitThing.PositionHeld, hitThing.MapHeld);
                projectile.Launch(hitThing, Caster, Caster, ProjectileHitFlags.IntendedTarget);

                if (Caster is { } cP && cP.BloodNeed() is { } casterBn)
                {
                    MoteMaker.ThrowText(cP.DrawPos, cP.Map, "+2");
                    casterBn.AdjustBlood(2);
                }
            }
        }
    }
}