using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_EnterTheAbyss : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            //target.Drawer.Notify_DebugAffected();
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                if (t.Cell.Walkable(this.CasterPawn.MapHeld))
                {
                    MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
                    this.CasterPawn.Position = t.Cell;
                    return;
                }
                MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastFailure, -1f);

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
