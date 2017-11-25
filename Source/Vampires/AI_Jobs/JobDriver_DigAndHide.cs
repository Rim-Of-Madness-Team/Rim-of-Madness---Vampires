using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vampire
{
    public class JobDriver_DigAndHide : JobDriver
    {
        protected const int BaseWorkAmount = 12000;

        private float workLeft = -1000f;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            if (TargetLocA != pawn.PositionHeld) yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                this.workLeft = BaseWorkAmount;
                this.job.SetTarget(TargetIndex.B, this.pawn);
            };
            doWork.tickAction = delegate
            {
                if (GetActor().Downed || GetActor().Dead || GetActor().pather.MovingNow)
                {
                    this.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                this.workLeft -= this.pawn.skills.GetSkill(SkillDefOf.Melee).Level;// (StatDefOf.ConstructionSpeed, true);
                if (this.workLeft <= 0f)
                {
                    Thing thing = ThingMaker.MakeThing(VampDefOf.ROMV_HideyHole, null);
                    thing.SetFaction(this.pawn.Faction, null);
                    GenSpawn.Spawn(thing, this.TargetLocA, this.Map);

                    Pawn actor = this.pawn;
                    Building_Casket pod = (Building_Casket)thing;

                    actor.DeSpawn();
                    pod.GetDirectlyHeldThings().TryAdd(actor);

                    this.ReadyForNextToil();
                    return;
                }
                //JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f);
            };
            doWork.WithProgressBar(TargetIndex.B, () => 1f - (float)this.workLeft / (float)BaseWorkAmount);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            //doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.pawn, null));
            //doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
