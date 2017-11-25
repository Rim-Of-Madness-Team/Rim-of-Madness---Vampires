using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class JobDriver_Diablerie : JobDriver
    {
        private float workLeft = -1f;
        private const float BaseFeedTime = 320f;
        private const float BaseEmbraceTime = 1000f;

        protected Pawn Victim
        {
            get
            {
                if (base.job.targetA.Thing is Pawn p) return p;
                if (base.job.targetA.Thing is Corpse c) return c.InnerPawn;
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
            this.BloodVictim.TransferBloodTo(1, BloodFeeder);
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
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Pawn p = (Pawn)TargetA;
                    if (!p.Dead) p.Kill(null);
                    this.job.SetTarget(TargetIndex.A, p.Corpse);
                    this.pawn.Reserve(TargetA, this.job);
                }
            };
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Pawn p = ((Corpse)TargetA.Thing).InnerPawn;
                    this.pawn.VampComp().Notify_Diablerie(p.VampComp());
                }
            };
        }

        public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
        {
            if (victim == null || victim.Dead)
            {
                return false;
            }
            if (victim.BloodNeed().CurBloodPoints <= 1)
            {
                int dmg = Rand.Range(1, 20) + feeder.skills.GetSkill(SkillDefOf.Melee).Level;
                victim.TakeDamage(new DamageInfo(VampDefOf.ROMV_Drain, dmg, -1, feeder));
            }
            return true;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
