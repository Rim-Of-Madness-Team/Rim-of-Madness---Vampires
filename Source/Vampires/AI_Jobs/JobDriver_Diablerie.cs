﻿using RimWorld;
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

        private bool DiablerieInteractionGiven = false;
        
        private void DoEffect()
        {
            pawn.VampComp().MostRecentVictim = Victim;
            BloodVictim.TransferBloodTo(1, BloodFeeder, false);
            if (!Victim.InAggroMentalState)
            {
                Victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "ROMV_AttemptedDiablerie".Translate(this.pawn.Named("PAWN")), false, false, null);
            }

            if (DiablerieInteractionGiven) return;
            DiablerieInteractionGiven = true;
            Find.LetterStack.ReceiveLetter("ROMV_AttemptedDiablerie".Translate(this.pawn.Named("PAWN")), "ROMV_VampireDiablerieAttemptedLetter".Translate(this.pawn.Named("PAWN"), Victim.Named("VICTIM")), LetterDefOf.ThreatSmall, pawn);
            MoteMaker.MakeInteractionBubble(GetActor(), Victim, VampDefOf.ROMV_VampireDiablerieAttempt.interactionMote, VampDefOf.ROMV_VampireDiablerieAttempt.GetSymbol());
            if (this?.Victim?.needs?.mood?.thoughts?.memories is MemoryThoughtHandler m)
            {
                m.TryGainMemory(VampDefOf.ROMV_VampireDiablerieAttempt.recipientThought, GetActor());
            }
            Find.PlayLog.Add(new PlayLogEntry_Interaction(VampDefOf.ROMV_VampireDiablerieAttempt, this.GetActor(), this.Victim, null));
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
                    if (!p.Dead) 
                    {
                        p.Kill(null);
                    };
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
                victim.TakeDamage(new DamageInfo(VampDefOf.ROMV_Drain, dmg, 1f, -1, feeder));
            }
            return true;
        }

        public override bool TryMakePreToilReservations(bool uhuh)
        {
            return true;
        }
    }
}
