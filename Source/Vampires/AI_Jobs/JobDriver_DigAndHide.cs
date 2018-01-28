using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vampire
{
    public class JobDriver_DigAndHide : JobDriver
    {
        private float workLeft = -1000f;
        protected const int BaseWorkAmount = 8900;

        public float WorkLeft
        {
            get
            {
                if (HarmonyPatches.DigHoleAttempts.ContainsKey(this.GetActor()))
                {
                    if (!HarmonyPatches.DigHoleAttempts[this.GetActor()].ShouldUseData())
                        HarmonyPatches.DigHoleAttempts.Remove(this.GetActor());
                    else
                        return HarmonyPatches.DigHoleAttempts[this.GetActor()].WorkLeft;
                }
                HarmonyPatches.DigHoleAttempts.Add(this.GetActor(), new DigHoleAttempt(Find.TickManager.TicksGame, BaseWorkAmount));
                HarmonyPatches.DigHoleAttempts[this.GetActor()].TicksSinceAttempt = Find.TickManager.TicksGame;
                return HarmonyPatches.DigHoleAttempts[this.GetActor()].WorkLeft;
            }
            set
            {
                if (!HarmonyPatches.DigHoleAttempts.ContainsKey(this.GetActor()))
                {
                    HarmonyPatches.DigHoleAttempts.Add(this.GetActor(), new DigHoleAttempt(Find.TickManager.TicksGame, BaseWorkAmount));
                }
                HarmonyPatches.DigHoleAttempts[this.GetActor()].WorkLeft = value;
                HarmonyPatches.DigHoleAttempts[this.GetActor()].TicksSinceAttempt = Find.TickManager.TicksGame;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            if (TargetLocA != pawn.PositionHeld) yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                if (WorkLeft < -500f)
                    WorkLeft = BaseWorkAmount;
                job.SetTarget(TargetIndex.B, pawn);
            };
            doWork.tickAction = delegate
            {
                if (GetActor().Downed || GetActor().Dead || GetActor().pather.MovingNow)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                WorkLeft -= pawn.skills.GetSkill(SkillDefOf.Melee).Level;// (StatDefOf.ConstructionSpeed, true);
                if (WorkLeft <= 0f)
                {
                    WorkLeft = BaseWorkAmount;
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
            doWork.WithProgressBar(TargetIndex.B, () => 1f - (float)WorkLeft / (float)BaseWorkAmount);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            //doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.pawn, null));
            //doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, "workLeft");
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
