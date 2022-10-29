using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire;

public class PawnTemporary : Pawn
{
    public static readonly int ticksToDestroy = 1800; //30 seconds

    private Effecter effecter;
    private Pawn master;
    private bool setup;
    private int ticksLeft;
    private int ticksUntilNextTryGiveJob = -1;

    public Pawn Master
    {
        get => master;
        set => master = value;
    }

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

            if (def == VampDefOf.ROMV_BatSpectralRace)
            {
                if (Master == null) Log.Warning("No master for " + def.LabelCap + ". Cancelling FeedAndReturn job.");
                if (Master != null)
                {
                    if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null &&
                            x.Faction.HostileTo(Master.Faction) &&
                            this.CanReserve(x)) is { } target)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndReturn, target, Master);
                        jobs.StopAll();
                        jobs.TryTakeOrderedJob(newJob);
                    }
                    else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master &&
                                 !x.IsVampire(true) && x.RaceProps.Humanlike &&
                                 this.CanReserve(x)) is { } tTwo)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndReturn, tTwo, Master);
                        jobs.StopAll();
                        jobs.TryTakeOrderedJob(newJob);
                    }
                }
            }

            if (def == VampDefOf.ROMV_BloodMistRace)
            {
                if (Master == null) Log.Warning("No master for " + def.LabelCap + ". Cancelling FeedAndDestroy job.");
                if (Master != null)
                {
                    if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null &&
                            x.Faction.HostileTo(Master.Faction) &&
                            this.CanReserve(x)) is { } target)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, target, Master);
                        jobs.StartJob(newJob, JobCondition.InterruptForced);
                        //jobs.TryTakeOrderedJob(newJob);
                    }
                    else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master &&
                                 !x.IsVampire(true) && x.RaceProps.Humanlike &&
                                 this.CanReserve(x)) is { } tTwo)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, tTwo, Master);
                        jobs.StartJob(newJob, JobCondition.InterruptForced);
                        //jobs.TryTakeOrderedJob(newJob);
                    }
                }
            }
        }

        ticksUntilNextTryGiveJob--;
        if (ticksUntilNextTryGiveJob < 0)
        {
            ticksUntilNextTryGiveJob = new IntRange(500, 700).RandomInRange;
            if (def == VampDefOf.ROMV_BatSpectralRace && CurJob != null && CurJob.def != VampDefOf.ROMV_FeedAndReturn)
            {
                if (Master == null) Log.Warning("No master for " + def.LabelCap + ". Cancelling FeedAndReturn job.");
                if (Master != null)
                {
                    if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null &&
                            x.Faction.HostileTo(Master.Faction) &&
                            this.CanReserve(x)) is { } target)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndReturn, target, Master);
                        jobs.StartJob(newJob, JobCondition.InterruptForced);
                        //jobs.TryTakeOrderedJob(newJob);
                    }
                    else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master &&
                                 !x.IsVampire(true) &&
                                 x.RaceProps.Humanlike &&
                                 this.CanReserve(x)) is { } tTwo)
                    {
                        var newJob = new Job(VampDefOf.ROMV_FeedAndReturn, tTwo, Master);
                        jobs.StartJob(newJob, JobCondition.InterruptForced);
                        //jobs.TryTakeOrderedJob(newJob);
                    }
                }
            }
            /*if (def == VampDefOf.ROMV_BloodMistRace && CurJob != null && CurJob.def != VampDefOf.ROMV_FeedAndDestroy)
            {
                if (Master == null)
                {
                    Log.Warning("No master for " + def.LabelCap + ". Cancelling FeedAndReturn job.");
                }
                if (Master != null)
                {
                    if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x.Faction.HostileTo(Master.Faction) && 
                    this.CanReserve(x)) is Pawn target)
                    {
                        Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, target, Master);
                        jobs.TryTakeOrderedJob(newJob);
                    }
                    else if (Master.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x.Faction != null && x != Master && !x.IsVampire(true) && x.RaceProps.Humanlike &&
                    this.CanReserve(x)) is Pawn tTwo)
                    {
                        Job newJob = new Job(VampDefOf.ROMV_FeedAndDestroy, tTwo, Master);
                        jobs.TryTakeOrderedJob(newJob);
                    }
                }
            }*/
            //The above should no longer be needed as the action cannot be interrupted.
        }

        ticksLeft--;
        if (ticksLeft <= 0) Destroy();

        if (Spawned)
        {
            if (effecter == null)
            {
                var progressBar = EffecterDefOf.ProgressBar;
                effecter = progressBar.Spawn();
            }
            else
            {
                LocalTargetInfo target = this;
                if (Spawned) effecter.EffectTick(this, TargetInfo.Invalid);
                var mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                if (mote != null)
                {
                    var result = 1f - (ticksToDestroy - ticksLeft) / (float)ticksToDestroy;

                    mote.progress = Mathf.Clamp01(result);
                    mote.offsetZ = -0.5f;
                }
            }
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (effecter != null) effecter.Cleanup();
        VampireUtility.SummonEffect(PositionHeld, MapHeld, this, 2f);
        base.DeSpawn(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft");
        Scribe_References.Look(ref master, "master");
    }
}