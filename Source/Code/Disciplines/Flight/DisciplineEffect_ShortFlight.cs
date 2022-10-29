using AbilityUser;
using Verse;

namespace Vampire;

public class DisciplineEffect_ShortFlight : Verb_UseAbility
{
    public virtual void Effect()
    {
        if (TargetsAoE[0] is { } t && t.Cell != default)
        {
            var caster = CasterPawn;
            LongEventHandler.QueueLongEvent(delegate
            {
                var flyingObject = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("ROMV_FlyingObject"),
                    CasterPawn.Position, CasterPawn.Map);
                flyingObject.Launch(CasterPawn, t.Cell, CasterPawn);
            }, "LaunchingFlyer", false, null);
        }
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