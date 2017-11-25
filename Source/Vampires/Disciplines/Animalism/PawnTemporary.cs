using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class PawnTemporary : Pawn
    {
        private bool setup = false;
        private Pawn master = null;
        private int ticksLeft;
        private int ticksUntilNextTryGiveJob = -1;

        public Pawn Master { get => master; set => master = value; }
        public static readonly int ticksToDestroy = 1800; //30 seconds

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            ticksLeft = ticksToDestroy;
            base.SpawnSetup(map, respawningAfterLoad);
        }
        
        public override void Tick()
        {
            base.Tick();
            if (setup == false && Find.TickManager.TicksGame % 10 == 0)
            {
                setup = true;
                
                if (this.def == VampDefOf.ROMV_BatSpectralRace)
                {
                    if (Master == null)
                    {
                        Log.Warning("No master for " + this.def.LabelCap + ". Cancelling FeedAndReturn job.");
                    }
                    if (Master != null)
                    {
                        if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x.Faction.HostileTo(Master.Faction) &&
                        this.CanReserve(x)) is Pawn target)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndReturn, target, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                        else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master && !x.IsVampire() && x.RaceProps.Humanlike &&
                        this.CanReserve(x)) is Pawn tTwo)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndReturn, tTwo, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                    }
                }
                if (this.def == VampDefOf.ROMV_BloodMistRace)
                {
                    if (Master == null)
                    {
                        Log.Warning("No master for " + this.def.LabelCap + ". Cancelling FeedAndReturn job.");
                    }
                    if (Master != null)
                    {
                        if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x.Faction.HostileTo(Master.Faction) &&
                        this.CanReserve(x)) is Pawn target)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, target, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                        else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master && !x.IsVampire() && x.RaceProps.Humanlike && 
                        this.CanReserve(x)) is Pawn tTwo)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, tTwo, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                    }
                }
            }

            ticksUntilNextTryGiveJob--;
            if (ticksUntilNextTryGiveJob < 0)
            {
                ticksUntilNextTryGiveJob = new IntRange(500, 700).RandomInRange;
                if (this.def == VampDefOf.ROMV_BatSpectralRace && this.CurJob != null && this.CurJob.def != VampDefOf.ROMV_FeedAndReturn)
                {
                    if (Master == null)
                    {
                        Log.Warning("No master for " + this.def.LabelCap + ". Cancelling FeedAndReturn job.");
                    }
                    if (Master != null)
                    {
                        if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x.Faction.HostileTo(Master.Faction) && 
                        this.CanReserve(x)) is Pawn target)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndReturn, target, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                        else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master && !x.IsVampire() && 
                        x.RaceProps.Humanlike && 
                        this.CanReserve(x)) is Pawn tTwo)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndReturn, tTwo, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                    }
                }
                if (this.def == VampDefOf.ROMV_BloodMistRace && this.CurJob != null && this.CurJob.def != VampDefOf.ROMV_FeedAndDestroy)
                {
                    if (Master == null)
                    {
                        Log.Warning("No master for " + this.def.LabelCap + ". Cancelling FeedAndReturn job.");
                    }
                    if (Master != null)
                    {
                        if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x.Faction.HostileTo(Master.Faction) && 
                        this.CanReserve(x)) is Pawn target)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, target, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                        else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master && !x.IsVampire() && x.RaceProps.Humanlike &&
                        this.CanReserve(x)) is Pawn tTwo)
                        {
                            Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, tTwo, Master);
                            this.jobs.TryTakeOrderedJob(newJob, JobTag.Misc);
                        }
                    }
                }
            }

            ticksLeft--;
            if (ticksLeft <= 0) this.Destroy();

            if (Spawned)
            {
                if (effecter == null)
                {
                    EffecterDef progressBar = EffecterDefOf.ProgressBar;
                    effecter = progressBar.Spawn();
                }
                else
                {
                    LocalTargetInfo target = this;
                    if (this.Spawned)
                    {
                        effecter.EffectTick(this, TargetInfo.Invalid);
                    }
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                    if (mote != null)
                    {
                        float result = 1f - (float)(PawnTemporary.ticksToDestroy - this.ticksLeft) / (float)PawnTemporary.ticksToDestroy;

                        mote.progress = Mathf.Clamp01(result);
                        mote.offsetZ = -0.5f;
                    }
                }
            }
        }

        Effecter effecter = null;

        public override void DeSpawn()
        {
            if (effecter != null) effecter.Cleanup();
            base.DeSpawn();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0);
        }
    }
}
