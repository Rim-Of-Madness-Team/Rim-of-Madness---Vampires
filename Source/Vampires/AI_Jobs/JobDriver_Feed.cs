using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Vampire
{
    public class JobDriver_Feed : JobDriver
    {
        private float workLeft = -1f;
        public static float BaseFeedTime = 320f;
        
        protected Pawn Victim => base.job.targetA.Thing as Pawn;
        protected CompVampire CompVictim => Victim.GetComp<CompVampire>();
        protected CompVampire CompFeeder => this.GetActor().GetComp<CompVampire>();
        protected Need_Blood BloodVictim => Victim.BloodNeed();
        protected Need_Blood BloodFeeder => this.GetActor().BloodNeed();

        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        public virtual void DoEffect()
        {
            this.BloodVictim.TransferBloodTo(1, BloodFeeder);
        }
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                return this.pawn == this.Victim;
            });
            this.AddEndCondition(delegate
            {
                if (!CompFeeder.BloodPool.IsFull)
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Succeeded;
            });
            foreach (Toil t in MakeFeedToils(this.job.def, this, this.pawn, this.TargetA, VampDefOf.ROMV_IWasBittenByAVampire, VampDefOf.ROMV_IGaveTheKiss, workLeft, DoEffect, ShouldContinueFeeding))
            {
                yield return t;
            }
        }


        public static IEnumerable<Toil> MakeFeedToils(JobDef job, JobDriver thisDriver, Pawn actor, LocalTargetInfo TargetA, ThoughtDef victimThoughtDef, ThoughtDef actorThoughtDef, float workLeft, Action effect, Func<Pawn, Pawn, bool> stopCondition, bool needsGrapple = true, bool cleansWound = true)
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return gotoToil;
            Toil grappleToil = new Toil()
            {
                initAction = delegate
                {
                    MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);

                    workLeft = JobDriver_Feed.BaseFeedTime;
                    Pawn victim = TargetA.Thing as Pawn; 
                    if (victim != null)
                    {

                        if (actor.InAggroMentalState || victim.InAggroMentalState || victim.Faction != actor.Faction)
                        {
                            if (needsGrapple)
                            {
                                if (!JecsTools.GrappleUtility.TryGrapple(actor, victim))
                                {
                                    thisDriver.EndJobWith(JobCondition.Incompletable);
                                    PawnUtility.ForceWait(actor, (int)(BaseFeedTime * 0.15f));
                                    return;
                                }
                            }
                        }
                        if (!AllowFeeding(actor, victim))
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        }
                        VampireBiteUtility.MakeNew(actor, victim);
                        victim.stances.stunner.StunFor((int)BaseFeedTime);
                    }
                }
            };
            yield return grappleToil;
            Toil feedToil = new Toil()
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
                                if (victimThoughtDef != null) victimThought = (Thought_Memory)ThoughtMaker.MakeThought(victimThoughtDef);
                                if (victimThought != null)
                                {
                                    victim.needs.mood.thoughts.memories.TryGainMemory(victimThought, null);
                                }
                            }
                            if (actor?.needs?.mood?.thoughts?.memories != null)
                            {
                                Thought_Memory actorThought = null;
                                if (actorThoughtDef != null) actorThought = (Thought_Memory)ThoughtMaker.MakeThought(actorThoughtDef);
                                if (actorThought != null)
                                {
                                    actor.needs.mood.thoughts.memories.TryGainMemory(actorThought, null);
                                }
                            }
                            

                            if (workLeft <= 0f)
                            {
                                if (actor?.VampComp() is CompVampire v && v.IsVampire && actor.Faction == Faction.OfPlayer)
                                {
                                    MoteMaker.ThrowText(actor.DrawPos, actor.Map, "XP +" + 15, -1f);
                                    v.XP += 15;
                                }
                                workLeft = BaseFeedTime;
                                MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
                                effect();
                                if (victim != null && !victim.Dead && needsGrapple)
                                {
                                    if (!JecsTools.GrappleUtility.TryGrapple(actor, victim))
                                    {
                                        thisDriver.EndJobWith(JobCondition.Incompletable);
                                    }
                                }

                                if (!stopCondition(actor, victim))
                                {
                                    thisDriver.ReadyForNextToil();
                                    if (cleansWound) VampireBiteUtility.CleanBite(actor, victim);
                                }
                                else
                                {
                                    if (victim != null && !victim.Dead)
                                    {
                                        victim.stances.stunner.StunFor((int)BaseFeedTime);
                                        PawnUtility.ForceWait((Pawn)TargetA.Thing, (int)BaseFeedTime, actor);

                                    }
                                }
                            }
                        }
                        else
                            thisDriver.ReadyForNextToil();
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
            feedToil.WithProgressBar(TargetIndex.A, () => 1f - workLeft / (float)BaseFeedTime, false, -0.5f);
            feedToil.PlaySustainerOrSound(delegate
            {
                return ThingDefOf.Beer.ingestible.ingestSound;
            });
            yield return feedToil;
        }

        public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
        {
            if (feeder == null)
            {
                return false;
            }
            if (victim == null)
            {
                return false;
            }
            if (feeder?.BloodNeed() == null)
            {
                return false;
            }
            if (victim?.BloodNeed() == null)
            {
                return false;
            }
            if (feeder?.BloodNeed()?.IsFull ?? false)
            {
                return false;
            }
            if (victim?.BloodNeed()?.CurBloodPoints == 0)
            {
                return false;
            }
            if (feeder?.health?.hediffSet?.GetFirstHediffOfDef(VampDefOf.ROMV_TheBeast)?.CurStageIndex != 3)
            {
                if (victim?.BloodNeed()?.CurBloodPoints <= 2)
                {

                    if (victim?.RaceProps?.Humanlike ?? false)
                    {

                        if (feeder?.BloodNeed()?.preferredFeedMode != PreferredFeedMode.HumanoidLethal)
                        {

                            return false;
                        }
                    }
                    else
                    {
                        if (feeder?.BloodNeed()?.preferredFeedMode != PreferredFeedMode.AnimalLethal)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        

        public static bool AllowFeeding(Pawn feeder, Pawn victim)
        {
            return true;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
