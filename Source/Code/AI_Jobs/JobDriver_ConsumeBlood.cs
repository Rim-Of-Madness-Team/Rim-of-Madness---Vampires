using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_ConsumeBlood : JobDriver
{
    public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

    public const TargetIndex IngestibleSourceInd = TargetIndex.A;

    private const TargetIndex TableCellInd = TargetIndex.B;

    private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

    private bool eatingFromInventory;

    private bool usingNutrientPasteDispenser;

    private Thing IngestibleSource => job.GetTarget(TargetIndex.A).Thing;

    private float ChewDurationMultiplier
    {
        get
        {
            var ingestibleSource = IngestibleSource;
            if (ingestibleSource.def.ingestible != null &&
                !ingestibleSource.def.ingestible.useEatingSpeedStat) return 1f;
            return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser");
        Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory");
    }

    public override string GetReport()
    {
        if (usingNutrientPasteDispenser)
            return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
        var thing = pawn.CurJob.targetA.Thing;
        if (thing != null && thing.def.ingestible != null && !thing.def.ingestible.ingestReportString.NullOrEmpty())
            return string.Format(thing.def.ingestible.ingestReportString, pawn.CurJob.targetA.Thing.LabelShort);
        return base.GetReport();
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        usingNutrientPasteDispenser = IngestibleSource is Building_NutrientPasteDispenser;
        eatingFromInventory = pawn.inventory != null && pawn.inventory.Contains(IngestibleSource);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        if (!usingNutrientPasteDispenser)
            this.FailOn(() => !IngestibleSource.Destroyed && !IngestibleSource.IngestibleNow);
        var chew = Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, TargetIndex.A, TargetIndex.B)
            .FailOn(x => !IngestibleSource.Spawned
                         && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource))
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        foreach (var toil in PrepareToIngestToils(chew)) yield return toil;
        yield return chew;
        yield return FinalizeIngest(pawn, TargetIndex.A);
        yield return Toils_Jump.JumpIf(chew, () => pawn?.BloodNeed()?.CurLevelPercentage < 1f);
    }

    private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
    {
        if (usingNutrientPasteDispenser) return PrepareToIngestToils_Dispenser();
        if (pawn.RaceProps.ToolUser) return PrepareToIngestToils_ToolUser(chewToil);
        return PrepareToIngestToils_NonToolUser();
    }

    [DebuggerHidden]
    private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
    {
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
        yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A);
        yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
    }

    [DebuggerHidden]
    private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
    {
        if (eatingFromInventory)
        {
            yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
        }
        else
        {
            yield return ReserveFoodIfWillIngestWholeStack();
            var gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Jump.JumpIf(gotoToPickup,
                () => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Jump.Jump(chewToil);
            yield return gotoToPickup;
            yield return Toils_Ingest.PickupIngestible(TargetIndex.A, pawn);
            var reserveExtraFoodToCollect = Toils_Reserve.Reserve(TargetIndex.C);
            var findExtraFoodToCollect = new Toil();
            findExtraFoodToCollect.initAction = delegate
            {
                if (pawn.inventory.innerContainer.TotalStackCountOfDef(IngestibleSource.def) < job.takeExtraIngestibles)
                {
                    Predicate<Thing> validator = x => pawn.CanReserve(x)
                                                      && !x.IsForbidden(pawn) && x.IsSociallyProper(pawn);
                    var thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                        ThingRequest.ForDef(IngestibleSource.def), PathEndMode.Touch,
                        TraverseParms.For(pawn),
                        12f, validator);
                    if (thing != null)
                    {
                        pawn.CurJob.SetTarget(TargetIndex.C, thing);
                        JumpToToil(reserveExtraFoodToCollect);
                    }
                }
            };
            findExtraFoodToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return Toils_Jump.Jump(findExtraFoodToCollect);
            yield return reserveExtraFoodToCollect;
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Haul.TakeToInventory(TargetIndex.C,
                () => job.takeExtraIngestibles -
                      pawn.inventory.innerContainer.TotalStackCountOfDef(IngestibleSource.def));
            yield return findExtraFoodToCollect;
        }

        yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
        yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
    }

    [DebuggerHidden]
    private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
    {
        yield return ReserveFoodIfWillIngestWholeStack();
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
    }

    private Toil ReserveFoodIfWillIngestWholeStack()
    {
        return new Toil
        {
            initAction = delegate
            {
                if (pawn.Faction == null) return;
                var thing = pawn.CurJob.GetTarget(TargetIndex.A).Thing;
                if (pawn.carryTracker.CarriedThing == thing) return;
                var num = 1; //FoodUtility.WillIngestStackCountOf(this.pawn, thing.def);
                if (num >= thing.stackCount)
                {
                    if (!thing.Spawned)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    pawn.Reserve(thing, job);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }


    public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
    {
        var cell = job.GetTarget(TargetIndex.B).Cell;
        return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, pawn);
    }

    public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip,
        IntVec3 placeCell, Pawn pawn)
    {
        if (pawn.pather.Moving) return false;
        var carriedThing = pawn.carryTracker.CarriedThing;
        if (carriedThing == null || !carriedThing.IngestibleNow) return false;
        if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) &&
            carriedThing.def.ingestible.ingestHoldUsesTable)
        {
            drawPos = new Vector3(placeCell.x + 0.5f, drawPos.y, placeCell.z + 0.5f);
            return true;
        }

        if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
        {
            var holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
            if (holdOffset != null)
            {
                drawPos += holdOffset.offset;
                behind = holdOffset.behind;
                flip = holdOffset.flip;
                return true;
            }
        }

        return false;
    }

    // RimWorld.Toils_Ingest    
    public static Toil FinalizeIngest(Pawn ingester, TargetIndex ingestibleInd)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var thing = curJob.GetTarget(ingestibleInd).Thing;
            if (ingester.needs.mood != null && thing.def.ingestible.chairSearchRadius > 10f)
            {
                if (!(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(actor.Map) &&
                    ingester.GetPosture() == PawnPosture.Standing)
                    ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AteWithoutTable);
                var room = ingester.GetRoom();
                if (room != null)
                {
                    var scoreStageIndex =
                        RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                    if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
                        ingester.needs.mood.thoughts.memories.TryGainMemory(
                            ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex));
                }
            }

            if (thing?.def?.ingestible?.outcomeDoers is { } doers && doers?.Count > 0)
                foreach (var doer in doers)
                    doer.DoIngestionOutcome(ingester, thing);

            var num2 = thing?.TryGetComp<CompBloodItem>()?.Props?.bloodPoints ?? 1;
            if (!ingester.Dead) ingester.BloodNeed().AdjustBlood(num2);
            if (thing.stackCount > 1) thing = thing.SplitOff(1);
            if (!thing.Destroyed) thing.Destroy();
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return pawn.Reserve(TargetA, job);
    }
}