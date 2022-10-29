using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_Feed : JobDriver
{
    public static float BaseFeedTime = 320f;
    public static float BaseCoolantThrowupChance = 0.95f;
    private readonly float workLeft = -1f;

    protected Pawn Victim => job.targetA.Thing as Pawn;
    protected CompVampire CompVictim => Victim.GetComp<CompVampire>();
    protected CompVampire CompFeeder => GetActor().GetComp<CompVampire>();
    protected Need_Blood BloodVictim => Victim.BloodNeed();
    protected Need_Blood BloodFeeder => GetActor().BloodNeed();


    public virtual void DoEffect()
    {
        if (BloodFeeder == null || Victim == null || BloodVictim == null) return;

        BloodVictim.TransferBloodTo(1, BloodFeeder);
        if (Victim.IsCoolantUser() && !pawn.IsCoolantUser())
            if (Rand.Value <= BaseCoolantThrowupChance)
            {
                pawn.VampComp().MostRecentVictim = Victim;
                EndJobWith(JobCondition.Incompletable);
                pawn.jobs.StartJob(new Job(JobDefOf.Vomit, pawn.PositionHeld));
            }
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
        foreach (var t in MakeFeedToils(job.def, this, pawn, TargetA, VampDefOf.ROMV_IWasBittenByAVampire,
                     VampDefOf.ROMV_IGaveTheKiss, workLeft, DoEffect, ShouldContinueFeeding)) yield return t;
    }


    public static IEnumerable<Toil> MakeFeedToils(JobDef job, JobDriver thisDriver, Pawn actor, LocalTargetInfo TargetA,
        ThoughtDef victimThoughtDef, ThoughtDef actorThoughtDef, float workLeft, Action effect,
        Func<Pawn, Pawn, bool> stopCondition, bool needsGrapple = true, bool cleansWound = true,
        bool neverGiveUp = false)
    {
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        var gotoToil =
            actor?.Faction == TargetA.Thing?.Faction && !actor.InAggroMentalState &&
            !((Pawn)TargetA.Thing).InAggroMentalState
                ? Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                : Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return gotoToil;
        var grappleToil = new Toil
        {
            initAction = delegate
            {
                MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);

                workLeft = BaseFeedTime;
                var victim = TargetA.Thing as Pawn;
                if (victim != null)
                {
//                        if (!AllowFeeding(actor, victim))
//                        {
//                            actor.jobs.EndCurrentJob(JobCondition.Incompletable);
//                        }
                    if (actor.InAggroMentalState || victim.InAggroMentalState || victim.Faction != actor.Faction)
                        if (needsGrapple)
                        {
                            var grappleBonus = actor is PawnTemporary ? 100 : 0;
                            if (!JecsTools.GrappleUtility.TryGrapple(actor, victim, grappleBonus))
                            {
                                thisDriver.EndJobWith(JobCondition.Incompletable);
                                PawnUtility.ForceWait(actor, (int)(BaseFeedTime * 0.15f));
                                return;
                            }
                        }

                    if (actor.IsVampire(true))
                        VampireBiteUtility.MakeNew(actor, victim);
                    victim.stances.stunner.StunFor((int)BaseFeedTime, actor);
                }
            }
        };
        yield return grappleToil;
        var feedToil = new Toil
        {
            tickAction = delegate
            {
                //try
                //{
                if (TargetA.Thing is Pawn victim && victim.Spawned && !victim.Dead)
                {
                    workLeft--;
                    VampireWitnessUtility.HandleWitnessesOf(job, actor, victim);
                    if (victim?.needs?.mood?.thoughts?.memories != null)
                    {
                        Thought_Memory victimThought = null;
                        if (victimThoughtDef != null)
                            victimThought = (Thought_Memory)ThoughtMaker.MakeThought(victimThoughtDef);
                        if (victimThought != null) victim.needs.mood.thoughts.memories.TryGainMemory(victimThought);
                    }

                    if (actor?.needs?.mood?.thoughts?.memories != null)
                    {
                        Thought_Memory actorThought = null;
                        if (actorThoughtDef != null)
                            actorThought = (Thought_Memory)ThoughtMaker.MakeThought(actorThoughtDef);
                        if (actorThought != null) actor.needs.mood.thoughts.memories.TryGainMemory(actorThought);
                    }


                    if (workLeft <= 0f)
                    {
                        if (actor?.VampComp() is { } v && v.IsVampire && actor.Faction == Faction.OfPlayer)
                        {
                            MoteMaker.ThrowText(actor.DrawPos, actor.Map, "XP +" + 15);
                            v.XP += 15;
                            workLeft = BaseFeedTime;
                            MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
                        }

                        effect();
                        if (victim != null && !victim.Dead && needsGrapple)
                        {
                            var victimBonus = victim.VampComp() is { } victimVampComp
                                ? -victimVampComp.Generation + 14
                                : 0;
                            var actorBonus = 0;
                            if (actor?.VampComp() is { } v2 && v2.IsVampire) actorBonus = -v2.Generation + 14;
                            if (!JecsTools.GrappleUtility.TryGrapple(actor, victim, actorBonus, victimBonus))
                                thisDriver.EndJobWith(JobCondition.Incompletable);
                        }

                        if (!stopCondition(actor, victim))
                        {
                            thisDriver.ReadyForNextToil();
                            if (actor.IsVampire(true) && cleansWound) VampireBiteUtility.CleanBite(actor, victim);
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
                }
                else
                {
                    thisDriver.ReadyForNextToil();
                }
                //}
                //catch(Exception e)
                //{
                //    Log.Message(e.ToString());
                //    thisDriver.ReadyForNextToil();
                //}
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
        if (feeder?.health?.hediffSet?.GetFirstHediffOfDef(VampDefOf.ROMV_TheBeast)?.CurStageIndex != 3)
            if (victim.BloodNeed().DrainingIsDeadly)
            {
                if (victim?.RaceProps?.Humanlike ?? false)
                {
                    if (feeder?.BloodNeed()?.preferredFeedMode != PreferredFeedMode.HumanoidLethal) return false;
                }
                else
                {
                    if (feeder?.BloodNeed()?.preferredFeedMode != PreferredFeedMode.AnimalLethal) return false;
                }
            }

        return true;
    }


//        public static bool AllowFeeding(Pawn feeder, Pawn victim)
//        {
//            return victim != null && victim.PositionHeld.IsValid && victim.PositionHeld.IsSunlightSafeFor(feeder);
//        }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}