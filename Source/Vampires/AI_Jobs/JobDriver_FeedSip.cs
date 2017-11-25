using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Vampire
{
    public class JobDriver_FeedSip : JobDriver
    {

        private float workLeft = -1f;
        public static float BaseFeedTime = 320f;

        protected Pawn Victim => (Pawn)base.job.targetA.Thing;
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
            foreach (Toil t in MakeFeedToils(this, this.pawn, this.TargetA, workLeft, DoEffect, ShouldContinueFeeding))
            {
                yield return t;
            }
        }

        public static IEnumerable<Toil> MakeFeedToils(JobDriver thisDriver, Pawn actor, LocalTargetInfo TargetA, float workLeft, Action effect, Func<Pawn, Pawn, bool> stopCondition)
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return gotoToil;
            Toil grappleToil = new Toil()
            {
                initAction = delegate
                {
                    MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);

                    workLeft = JobDriver_Feed.BaseFeedTime;
                    Pawn victim = (Pawn)TargetA.Thing;
                    if (victim != null)
                    {

                        if (victim.InAggroMentalState || victim.Faction != actor.Faction)
                        {
                            if (!JecsTools.GrappleUtility.CanGrapple(actor, victim))
                            {
                                thisDriver.EndJobWith(JobCondition.Incompletable);
                            }
                        }
                        GenClamor.DoClamor(actor, 10f, ClamorType.Harm);
                        if (!AllowFeeding(actor, victim))
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        }
                        if (actor?.VampComp()?.Bloodline?.bloodlineHediff?.CompProps<HediffCompProperties_VerbGiver>()?.verbs is List<VerbProperties> verbProps)
                        {
                            if (actor?.VerbTracker?.AllVerbs is List<Verb> verbs)
                            {
                                if (verbs.Find(x => verbProps.Contains(x.verbProps)) is Verb_MeleeAttack v)
                                {
                                    victim.TakeDamage(new DamageInfo(v.verbProps.meleeDamageDef, v.verbProps.meleeDamageBaseAmount, -1, actor));
                                }
                            }
                        }
                        victim.stances.stunner.StunFor((int)BaseFeedTime);
                    }
                }
            };
            yield return grappleToil;
            Toil feedToil = new Toil()
            {
                tickAction = delegate
                {
                    Pawn victim = (Pawn)TargetA.Thing;
                    if (victim == null || !victim.Spawned || victim.Dead)
                        thisDriver.ReadyForNextToil();
                    workLeft--;

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
                        if ((victim.HostileTo(actor.Faction) || victim.IsPrisoner) && !JecsTools.GrappleUtility.CanGrapple(actor, victim))
                        {
                            thisDriver.EndJobWith(JobCondition.Incompletable);
                        }

                        if (!stopCondition(actor, victim))
                        {
                            thisDriver.ReadyForNextToil();
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
            if (victim?.BloodNeed()?.CurBloodPoints <= 5)
            {
                return false;
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
