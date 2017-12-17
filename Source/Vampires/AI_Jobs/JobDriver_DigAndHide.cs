using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Vampire.Defs;
using Verse;
using Verse.AI;

namespace Vampire.AI_Jobs
{
    public class JobDriver_DigAndHide : JobDriver
    {
        protected const int BaseWorkAmount = 8900;

        private float workLeft = -1000f;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            if (TargetLocA != pawn.PositionHeld) yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                workLeft = BaseWorkAmount;
                job.SetTarget(TargetIndex.B, pawn);
            };
            doWork.tickAction = delegate
            {
                if (GetActor().Downed || GetActor().Dead || GetActor().pather.MovingNow)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                workLeft -= pawn.skills.GetSkill(SkillDefOf.Melee).Level;// (StatDefOf.ConstructionSpeed, true);
                if (workLeft <= 0f)
                {
                    Thing thing = ThingMaker.MakeThing(VampDefOf.ROMV_HideyHole);
                    thing.SetFaction(pawn.Faction);
                    GenSpawn.Spawn(thing, TargetLocA, Map);

                    Pawn actor = pawn;
                    Building_Casket pod = (Building_Casket)thing;

                    actor.DeSpawn();
                    pod.GetDirectlyHeldThings().TryAdd(actor);

                    ReadyForNextToil();
                    return;
                }
                //JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f);
            };
            doWork.WithProgressBar(TargetIndex.B, () => 1f - (float)workLeft / (float)BaseWorkAmount);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            //doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.pawn, null));
            //doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref workLeft, "workLeft");
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
