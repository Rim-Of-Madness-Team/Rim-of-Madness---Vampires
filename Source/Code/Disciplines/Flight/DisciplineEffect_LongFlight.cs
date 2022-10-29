using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_LongFlight : Verb_UseAbility
{
    public virtual void Effect()
    {
        if (TargetsAoE[0] is { } t && t.Cell != default)
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastSuccess);
            var flyingObject = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("ROMV_FlyingObject"), CasterPawn.Position,
                CasterPawn.Map);
            flyingObject.Launch(CasterPawn, t.Cell, CasterPawn);
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