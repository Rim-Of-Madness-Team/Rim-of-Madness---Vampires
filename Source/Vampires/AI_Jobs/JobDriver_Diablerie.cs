using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (job.targetA.Thing is Pawn p) return p;
                if (job.targetA.Thing is Corpse c) return c.InnerPawn;
                else return null;
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
            BloodVictim.TransferBloodTo(1, BloodFeeder, false);
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
                return pawn == Victim;
            });
            this.FailOnAggroMentalState(TargetIndex.A);
            foreach (Toil t in JobDriver_Feed.MakeFeedToils(job.def, this, GetActor(), TargetA, null, null, workLeft, DoEffect, ShouldContinueFeeding, true, false))
            {
                yield return t;
            }
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Pawn p = (Pawn)TargetA;
                    if (!p.Dead) p.Kill(null);
                    job.SetTarget(TargetIndex.A, p.Corpse);
                    pawn.Reserve(TargetA, job);
                }
            };
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_General.WaitWith(TargetIndex.A, 600, true);
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    VampireCorpse vampCorpse = (VampireCorpse)TargetA.Thing;
                    vampCorpse.Diableried = true;
                    Pawn p = vampCorpse.InnerPawn;
                    pawn.VampComp().Notify_Diablerie(p.VampComp());
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
