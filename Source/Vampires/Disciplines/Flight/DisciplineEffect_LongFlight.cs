using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{

    public class DisciplineEffect_LongFlight : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                CasterPawn.Drawer.Notify_DebugAffected();
                MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
                FlyingObject flyingObject = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("ROMV_FlyingObject"), this.CasterPawn.Position, this.CasterPawn.Map);
                flyingObject.Launch(this.CasterPawn, t.Cell, this.CasterPawn);
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
}
