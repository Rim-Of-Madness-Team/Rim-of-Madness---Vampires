using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class JobDriver_MakeBloodBond : JobDriver
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
        protected CompVampire CompThrall => Victim.GetComp<CompVampire>();
        protected CompVampire CompMaster => this.GetActor().GetComp<CompVampire>();
        protected Need_Blood BloodThrall => CompThrall.BloodPool;
        protected Need_Blood BloodMaster => CompMaster.BloodPool;
        
        private void DoEffect()
        {
            this.BloodMaster.TransferBloodTo(1, BloodThrall);
        }
        
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                return this.pawn == this.Victim;
            });
            this.FailOn(delegate
            {
                return BloodMaster.CurBloodPoints == 0;
            });
            this.FailOnAggroMentalState(TargetIndex.A);
            foreach (Toil t in JobDriver_Feed.MakeFeedToils(this.job.def, this, (Pawn)TargetA, this.GetActor(), null, null, workLeft, DoEffect, ShouldContinueFeeding))
            {
                yield return t;
            }
            yield return new Toil()
            {
                initAction = delegate ()
                {

                }
            };
        }

        public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
        {
            return false;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
