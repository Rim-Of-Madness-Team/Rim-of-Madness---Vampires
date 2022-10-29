using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_Embrace : JobDriver
{
    private const float BaseFeedTime = 320f;
    private const float BaseEmbraceTime = 1000f;
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
        pawn.VampComp().MostRecentVictim = Victim;
        BloodVictim.TransferBloodTo(1, BloodFeeder);
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
                     DoEffect, ShouldContinueFeeding, GetActor()?.Faction != TargetA.Thing?.Faction, false))
            yield return t;
        yield return new Toil
        {
            initAction = delegate
            {
                var p = (Pawn)TargetA;
                if (!p.Dead) p.Kill(null);
                job.SetTarget(TargetIndex.A, p.Corpse);
                pawn.Reserve(TargetA, job);
            }
        };
        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
        yield return new Toil
        {
            initAction = delegate { pawn.VampComp().Bloodline.EmbraceWorker.TryEmbrace(pawn, Victim); }
        };
    }

    public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
    {
        if (victim.Dead) return false;
        if (victim.BloodNeed().CurBloodPoints < 1) return false;
        return true;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}