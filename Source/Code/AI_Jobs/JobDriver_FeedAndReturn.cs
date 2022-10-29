using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_FeedAndReturn : JobDriver
{
    private const float BaseFeedTime = 320f;
    private const float BaseEmbraceTime = 1000f;
    private int bloodCarried;
    private readonly float workLeft = -1f;

    protected Pawn Victim
    {
        get
        {
            if (job.targetA.Thing is Pawn p) return p;
            if (job.targetA.Thing is Corpse c) return c.InnerPawn;
            return null;
        }
    }

    protected Pawn Master
    {
        get
        {
            if (job.targetB.Thing is Pawn p) return p;
            if (job.targetB.Thing is Corpse c) return c.InnerPawn;
            return null;
        }
    }

    protected CompVampire CompVictim => Victim.GetComp<CompVampire>();
    protected CompVampire CompFeeder => GetActor().GetComp<CompVampire>();
    protected Need_Blood BloodVictim => CompVictim.BloodPool;
    protected Need_Blood BloodFeeder => CompFeeder.BloodPool;

    public override void Notify_Starting()
    {
        base.Notify_Starting();
    }

    private void DoEffect()
    {
        bloodCarried++;
        BloodVictim.TransferBloodTo(1, Master.BloodNeed());
        MoteMaker.ThrowText(Master.DrawPos, Master.Map, "+1 Blood");
    }

    public override string GetReport()
    {
        return base.GetReport();
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        //this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(delegate { return pawn == Victim; });
        this.FailOnAggroMentalState(TargetIndex.A);
        foreach (var t in JobDriver_Feed.MakeFeedToils(job.def, this, GetActor(), TargetA, null, null, workLeft,
                     DoEffect, ShouldContinueFeeding)) yield return t;
        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
        yield return Toils_Goto.GotoThing(TargetIndex.B, Master.PositionHeld)
            .FailOn(() => Master == null || !Master.Spawned || Master.Dead);
    }

    public bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
    {
        if (victim.Dead) return false;
        if (victim.BloodNeed().CurBloodPoints < 1) return false;
        if (bloodCarried > 3) return false;
        return true;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}