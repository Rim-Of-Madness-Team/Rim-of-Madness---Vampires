using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_DigAndHide : JobDriver
{
    protected const int BaseWorkAmount = 8900;
    private float workLeft = -1000f;

    public float WorkLeft
    {
        get
        {
            if (HarmonyPatches.DigHoleAttempts.ContainsKey(GetActor()))
            {
                if (!HarmonyPatches.DigHoleAttempts[GetActor()].ShouldUseData())
                    HarmonyPatches.DigHoleAttempts.Remove(GetActor());
                else
                    return HarmonyPatches.DigHoleAttempts[GetActor()].WorkLeft;
            }

            HarmonyPatches.DigHoleAttempts.Add(GetActor(),
                new DigHoleAttempt(Find.TickManager.TicksGame, BaseWorkAmount));
            HarmonyPatches.DigHoleAttempts[GetActor()].TicksSinceAttempt = Find.TickManager.TicksGame;
            return HarmonyPatches.DigHoleAttempts[GetActor()].WorkLeft;
        }
        set
        {
            if (!HarmonyPatches.DigHoleAttempts.ContainsKey(GetActor()))
                HarmonyPatches.DigHoleAttempts.Add(GetActor(),
                    new DigHoleAttempt(Find.TickManager.TicksGame, BaseWorkAmount));
            HarmonyPatches.DigHoleAttempts[GetActor()].WorkLeft = value;
            HarmonyPatches.DigHoleAttempts[GetActor()].TicksSinceAttempt = Find.TickManager.TicksGame;
        }
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        if (TargetLocA != pawn.PositionHeld) yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
        var doWork = new Toil();
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

            WorkLeft -= pawn.skills.GetSkill(SkillDefOf.Melee).Level; // (StatDefOf.ConstructionSpeed, true);
            if (WorkLeft <= 0f)
            {
                WorkLeft = BaseWorkAmount;
                var thing = ThingMaker.MakeThing(VampDefOf.ROMV_HideyHole);
                thing.SetFaction(pawn.Faction);
                GenSpawn.Spawn(thing, TargetLocA, Map);

                var actor = pawn;
                var pod = (Building_Casket)thing;

                actor.DeSpawn();
                pod.GetDirectlyHeldThings().TryAdd(actor);

                ReadyForNextToil();
            }
            //JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f);
        };
        doWork.WithProgressBar(TargetIndex.B, () => 1f - WorkLeft / BaseWorkAmount);
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

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}