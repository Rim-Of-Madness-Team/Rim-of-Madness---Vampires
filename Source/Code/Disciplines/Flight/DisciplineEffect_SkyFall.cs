using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_SkyFall : Verb_UseAbility
{
    public virtual void Effect()
    {
        if (TargetsAoE[0] is { } t && t.Thing is Pawn target)
            LongEventHandler.QueueLongEvent(delegate
            {
                var flyingObject = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("ROMV_FlyingObject"),
                    CasterPawn.Position, CasterPawn.Map);
                flyingObject.damageLaunched = false;
                flyingObject.timesToDamage = 3;
                flyingObject.explosion = true;
                flyingObject.Launch(CasterPawn, target, CasterPawn,
                    new DamageInfo(DamageDefOf.Blunt, Rand.Range(15, 25), 1f, -1, CasterPawn));
            }, "LaunchingFlyerSkyFall", false, null);
    }

    public override void PostCastShot(bool inResult, out bool outResult)
    {
        if (inResult)
        {
            Effect();
            outResult = true;
        }

        outResult = inResult;
    }
}