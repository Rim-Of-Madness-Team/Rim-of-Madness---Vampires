using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_FeedSip : JobDriver
{
    public static float BaseFeedTime = 320f;

    private readonly float workLeft = -1f;

    protected Pawn Victim => (Pawn)job.targetA.Thing;
    protected CompVampire CompVictim => Victim.GetComp<CompVampire>();
    protected CompVampire CompFeeder => GetActor().GetComp<CompVampire>();
    protected Need_Blood BloodVictim => Victim.BloodNeed();
    protected Need_Blood BloodFeeder => GetActor().BloodNeed();

    public override void Notify_Starting()
    {
        base.Notify_Starting();
    }

    public virtual void DoEffect()
    {
        BloodVictim.TransferBloodTo(1, BloodFeeder);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(delegate { return pawn == Victim; });
        AddEndCondition(delegate
        {
            if (!CompFeeder.BloodPool.IsFull) return JobCondition.Ongoing;
            return JobCondition.Succeeded;
        });
        foreach (var t in MakeFeedToils(this, pawn, TargetA, workLeft, DoEffect, ShouldContinueFeeding)) yield return t;
    }

    public static IEnumerable<Toil> MakeFeedToils(JobDriver thisDriver, Pawn actor, LocalTargetInfo TargetA,
        float workLeft, Action effect, Func<Pawn, Pawn, bool> stopCondition)
    {
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        var gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return gotoToil;
        var grappleToil = new Toil
        {
            initAction = delegate
            {
                MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);

                workLeft = JobDriver_Feed.BaseFeedTime;
                var victim = (Pawn)TargetA.Thing;
                if (victim != null)
                {
                    if (victim.InAggroMentalState || victim.Faction != actor.Faction)
                        if (!JecsTools.GrappleUtility.CanGrapple(actor, victim))
                            thisDriver.EndJobWith(JobCondition.Incompletable);
                    GenClamor.DoClamor(actor, 10f, ClamorDefOf.Harm);
                    if (!AllowFeeding(actor, victim)) actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    if (actor?.VampComp()?.Bloodline?.bloodlineHediff?.CompProps<HediffCompProperties_VerbGiver>()
                            ?.verbs is { } verbProps)
                        if (actor?.VerbTracker?.AllVerbs is { } verbs)
                            if (verbs.Find(x => verbProps.Contains(x.verbProps)) is Verb_MeleeAttack v)
                                victim.TakeDamage(new DamageInfo(v.verbProps.meleeDamageDef,
                                    v.verbProps.meleeDamageBaseAmount, v.verbProps.meleeArmorPenetrationBase, -1,
                                    actor));
                    victim.stances.stunner.StunFor((int)BaseFeedTime, actor);
                }
            }
        };
        yield return grappleToil;
        var feedToil = new Toil
        {
            tickAction = delegate
            {
                var victim = (Pawn)TargetA.Thing;
                if (victim == null || !victim.Spawned || victim.Dead)
                    thisDriver.ReadyForNextToil();
                workLeft--;

                if (workLeft <= 0f)
                {
                    if (actor?.VampComp() is { } v && v.IsVampire && actor.Faction == Faction.OfPlayer)
                    {
                        MoteMaker.ThrowText(actor.DrawPos, actor.Map, "XP +" + 15);
                        v.XP += 15;
                    }

                    workLeft = BaseFeedTime;
                    MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
                    effect();
                    if ((victim.HostileTo(actor.Faction) || victim.IsPrisoner) &&
                        !JecsTools.GrappleUtility.CanGrapple(actor, victim))
                        thisDriver.EndJobWith(JobCondition.Incompletable);

                    if (!stopCondition(actor, victim))
                    {
                        thisDriver.ReadyForNextToil();
                    }
                    else
                    {
                        if (victim != null && !victim.Dead)
                        {
                            victim.stances.stunner.StunFor((int)BaseFeedTime, actor);
                            PawnUtility.ForceWait((Pawn)TargetA.Thing, (int)BaseFeedTime, actor);
                        }
                    }
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never
        };
        feedToil.socialMode = RandomSocialMode.Off;
        feedToil.WithProgressBar(TargetIndex.A, () => 1f - workLeft / BaseFeedTime);
        feedToil.PlaySustainerOrSound(delegate { return ThingDefOf.Beer.ingestible.ingestSound; });
        yield return feedToil;
    }

    public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
    {
        if (feeder == null) return false;
        if (victim == null) return false;
        if (feeder?.BloodNeed() == null) return false;
        if (victim?.BloodNeed() == null) return false;
        if (feeder?.BloodNeed()?.IsFull ?? false) return false;
        if (victim?.BloodNeed()?.CurBloodPoints == 0) return false;
        if (victim?.BloodNeed()?.DrainingIsDeadly ?? false) return false;
        if (victim?.BloodNeed()?.CurBloodPoints <= 5) return false;
        return true;
    }


    public static bool AllowFeeding(Pawn feeder, Pawn victim)
    {
        return true;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}