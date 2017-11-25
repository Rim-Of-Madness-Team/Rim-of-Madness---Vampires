using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class JobDriver_FeedAndDestroy : JobDriver
    {
        private float workLeft = -1f;
        private const float BaseFeedTime = 320f;
        private const float BaseEmbraceTime = 1000f;
        private int bloodCarried = 0;

        protected Pawn Victim
        {
            get
            {
                if (base.job.targetA.Thing is Pawn p) return p;
                if (base.job.targetA.Thing is Corpse c) return c.InnerPawn;
                else return null;
            }
        }
        protected Pawn Master
        {
            get
            {
                if (base.job.targetB.Thing is Pawn p) return p;
                if (base.job.targetB.Thing is Corpse c) return c.InnerPawn;
                else return null;
            }
        }
        protected CompVampire CompVictim => Victim.GetComp<CompVampire>();
        protected CompVampire CompFeeder => this.GetActor().GetComp<CompVampire>();
        protected Need_Blood BloodVictim => CompVictim.BloodPool;
        protected Need_Blood BloodFeeder => CompFeeder.BloodPool;

        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        private void DoEffect()
        {
            this.BloodVictim.TransferBloodTo(1, Master.BloodNeed());
            if (Victim.health.hediffSet.GetNotMissingParts().ToList().FindAll(x => x.depth == BodyPartDepth.Inside) is List<BodyPartRecord> parts)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!Victim.Dead)
                    {
                        Victim.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(8, 12), -1, this.GetActor(), parts.RandomElement()));
                    }
                }
            }
        }

        public override string GetReport()
        {
            return base.GetReport();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                return this.pawn == this.Victim;
            });
            this.FailOnAggroMentalState(TargetIndex.A);
            foreach (Toil t in JobDriver_Feed.MakeFeedToils(this.job.def, this, this.GetActor(), this.TargetA, null, null, workLeft, DoEffect, ShouldContinueFeeding, true, false))
            {
                yield return t;
            }
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
            yield return Toils_Goto.GotoThing(TargetIndex.B, Master.PositionHeld).FailOn(() => (Master == null) || (!Master.Spawned || Master.Dead));
        }

        public bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
        {
            if (victim.Dead)
            {
                return false;
            }
            if (victim.BloodNeed().CurBloodPoints < 1)
            {
                return false;
            }
            if (bloodCarried > 3)
            {
                return false;
            }
            return true;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
