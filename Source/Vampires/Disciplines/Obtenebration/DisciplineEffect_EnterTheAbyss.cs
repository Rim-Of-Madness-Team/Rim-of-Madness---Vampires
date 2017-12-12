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
        public void Effect()
        {
            this.CasterPawn.Drawer.Notify_DebugAffected();
            if (TargetsAoE.FirstOrDefault(x => x is LocalTargetInfo y && y.Cell != default(IntVec3)) is LocalTargetInfo t)
            {
                if (t.Cell.Standable(this.CasterPawn.MapHeld))
                {
                    MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
                    this.CasterPawn.Position = t.Cell;
                    return;
                }
                MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastFailure, -1f);
            }
        }
        

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return true;
        }

        protected override bool TryCastShot()
        {
            this.CasterPawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            Effect();
            return base.TryCastShot();
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            this.CasterPawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            Effect();
            outResult = true;
        }
    }
}
