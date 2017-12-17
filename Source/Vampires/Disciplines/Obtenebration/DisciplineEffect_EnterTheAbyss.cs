using System.Linq;
using RimWorld;
using Verse;

namespace Vampire.Disciplines.Obtenebration
{
    public class DisciplineEffect_EnterTheAbyss : AbilityUser.Verb_UseAbility
    {
        public void Effect()
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            if (TargetsAoE.FirstOrDefault(x => x is LocalTargetInfo y && y.Cell != default(IntVec3)) is LocalTargetInfo t)
            {
                if (t.Cell.Standable(CasterPawn.MapHeld))
                {
                    MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess);
                    CasterPawn.Position = t.Cell;
                    return;
                }
                MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastFailure);
            }
        }
        

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return true;
        }

        protected override bool TryCastShot()
        {
            CasterPawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            Effect();
            return base.TryCastShot();
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            CasterPawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            Effect();
            outResult = true;
        }
    }
}
