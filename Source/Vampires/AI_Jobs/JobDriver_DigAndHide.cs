using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vampire
{
    public class JobDriver_DigAndHide : JobDriver
    {
        protected const int BaseWorkAmount = 8900;

        private float workLeft = -1000f;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
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
                    Thing thing = ThingMaker.MakeThing(VampDefOf.ROMV_HideyHole, null);
                    thing.SetFaction(pawn.Faction, null);
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
            Scribe_Values.Look(ref workLeft, "workLeft", 0f, false);
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
