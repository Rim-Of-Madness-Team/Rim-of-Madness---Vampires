using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class ScenPart_StartingVampires : ScenPart
{
    private BloodlineDef bloodline = VampDefOf.ROMV_ClanTremere;
    private int curVampires;
    private IntRange generationRange = new(10, 13);
    private int maxVampires = 1;
    private string maxVampiresBuf = "";
    private readonly bool randomBloodline = false;
    private bool spawnInCoffins;
    private float vampChance = 0.5f;


    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        var scenPartRect = listing.GetScenPartRect(this, RowHeight * 4f + 31f);
        if (Widgets.ButtonText(scenPartRect.TopPartPixels(RowHeight),
                this?.bloodline?.LabelCap ?? "ROMV_UnknownBloodline".Translate()))
            FloatMenuUtility.MakeMenu(PossibleBloodlines(), bl => bl.LabelCap, bl => delegate { bloodline = bl; });
        //Widgets.IntRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref this.generationRange, 4, this.maxGeneration, "ROMV_VampireGeneration");
        DoVampModifierEditInterface(new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, 31f));
    }

    // RimWorld.ScenPart_PawnModifier
    protected void DoVampModifierEditInterface(Rect rect)
    {
        var rect2 = new Rect(rect.x, rect.y + RowHeight, rect.width, 31);

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect2, "ROMV_VampireGeneration".Translate(generationRange.min + "-" + generationRange.max));

        var rect3 = new Rect(rect.x, rect.y + RowHeight * 2, rect.width, 31);
        Widgets.IntRange(rect3, 21, ref generationRange, 1, 13,
            ""); //HorizontalSlider(rect3, vampChance, 0f, 1f, false, "", "", "");

        //Widgets.TextFieldNumeric<float>(rect4, ref this, ref this.numOfVampsBuffer, 1, 50);
        var rect5 = new Rect(rect.x, rect.y + RowHeight * 3, rect.width, 31);
        var rect6 = rect5.LeftPart(0.333f).Rounded();
        var rect7 = rect5.RightPart(0.666f).Rounded();

        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect6, "ROMV_StartInCoffins".Translate());

        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.CheckboxLabeled(rect7, "", ref spawnInCoffins);
        var rect8 = new Rect(rect.x, rect.y + RowHeight * 4, rect.width, 31);
        var rect9 = rect8.LeftPart(0.666f).Rounded();
        var rect10 = rect8.RightPart(0.333f).Rounded();
        Text.Anchor = TextAnchor.MiddleRight;

        Widgets.Label(rect9, "ROMV_MaxVampires".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.TextFieldNumeric(rect10, ref maxVampires, ref maxVampiresBuf, 1, 100);
    }


    private IEnumerable<BloodlineDef> PossibleBloodlines()
    {
        return from x in DefDatabase<BloodlineDef>.AllDefsListForReading
            where x.scenarioCanAdd
            select x;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        //private BloodlineDef bloodline = VampDefOf.ROMV_ClanTremere;
        //private int numOfVamps = 1;
        //private string numOfVampsBuffer = "";
        //private IntRange generationRange = new IntRange(10, 13);
        //private int maxGeneration = 15;
        //private bool spawnInCoffins = false;
        Scribe_Defs.Look(ref bloodline, "bloodline");
        Scribe_Values.Look(ref generationRange, "generationRange");
        Scribe_Values.Look(ref vampChance, "vampChance", 0.5f);
        Scribe_Values.Look(ref maxVampires, "maxVampires", 1);
        Scribe_Values.Look(ref spawnInCoffins, "spawnInCoffins");
    }

    public override string Summary(Scenario scen)
    {
        return "ROMV_StartingVampiresSummary".Translate(new object[]
        {
            GetChanceOrMax() ?? "1",
            GenRangeToString() ?? "",
            this?.bloodline?.ToString() ?? "ROMV_UnknownBloodline".Translate(),
            GetInCoffinsString() ?? ""
        }).CapitalizeFirst();
    }

    public string GetChanceOrMax()
    {
        return this?.vampChance == 1.0f
            ? maxVampires.ToString()
            : "ROMV_ChanceOf".Translate(vampChance.ToStringPercent()).RawText;
    }

    public string GetInCoffinsString()
    {
        return this?.spawnInCoffins ?? false ? " " + "ROMV_StartInCoffins".Translate().RawText : "";
    }

    public string GenRangeToString()
    {
        return VampireStringUtility.AddOrdinal(this?.generationRange.min ?? 10) + "-" +
            VampireStringUtility.AddOrdinal(this?.generationRange.max ?? 15) ?? "";
    }

    public override void Randomize()
    {
        base.Randomize();
        vampChance = Rand.Range(0.2f, 0.8f);
        bloodline = PossibleBloodlines().RandomElement();
        generationRange.max = Rand.Range(10, 15);
        generationRange.min = Rand.Range(8, generationRange.max);
        spawnInCoffins = Rand.Value > 0.3 ? true : false;
    }

    public override void PostMapGenerate(Map map)
    {
        if (Find.GameInitData == null) return;
        if (spawnInCoffins)
        {
            var usingDropPods = Find.Scenario.AllParts.Any(x =>
                x is ScenPart_PlayerPawnsArriveMethod s &&
                (PlayerPawnsArriveMethod)AccessTools.Field(typeof(ScenPart_PlayerPawnsArriveMethod), "method")
                    .GetValue(s) == PlayerPawnsArriveMethod.DropPods);
            var list = new List<List<Thing>>();
            foreach (var current in Find.GameInitData.startingAndOptionalPawns)
                if (current.RaceProps.Humanlike &&
                    current?.health?.hediffSet?.hediffs.FirstOrDefault(y => y.def.defName.Contains("Vampirism")) !=
                    null)
                {
                    var loc = current.PositionHeld;
                    var casket = (Building_Casket)ThingMaker.MakeThing(VampDefOf.ROMV_RoyalCoffin, ThingDefOf.WoodLog);
                    casket.SetFaction(Faction.OfPlayer);
                    if (current.Spawned) current.DeSpawn();
                    if (current.holdingOwner != null) current.holdingOwner.Take(current);
                    if (!usingDropPods) GenPlace.TryPlaceThing(casket, loc, map, ThingPlaceMode.Near);
                    casket.GetDirectlyHeldThings().TryAdd(current);
                    if (usingDropPods) list.Add(new List<Thing> { casket });
                }

            var instaDrop = Find.GameInitData.QuickStarted;
            if (usingDropPods)
                DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, instaDrop, true);
        }
    }

    public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed)
    {
        if (Find.CurrentMap == null)
        {
            curVampires = Find.GameInitData.startingAndOptionalPawns.FindAll(x =>
                    x?.health?.hediffSet?.hediffs.FirstOrDefault(y => y.def.defName.Contains("Vampirism")) != null)
                ?.Count ?? 0;
            var def = randomBloodline ? PossibleBloodlines().RandomElement() : bloodline;
            if (pawn.RaceProps.Humanlike && context == PawnGenerationContext.PlayerStarter)
                if (!pawn?.story?.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) ?? false)
                    if (Rand.Value < vampChance && curVampires < maxVampires)
                    {
                        curVampires++;
                        var hediffDefToApply = VampDefOf.ROM_VampirismRandom;
                        if (def == VampDefOf.ROMV_ClanGargoyle) hediffDefToApply = VampDefOf.ROM_VampirismGargoyle;
                        if (def == VampDefOf.ROMV_ClanLasombra) hediffDefToApply = VampDefOf.ROM_VampirismLasombra;
                        if (def == VampDefOf.ROMV_ClanPijavica) hediffDefToApply = VampDefOf.ROM_VampirismPijavica;
                        if (def == VampDefOf.ROMV_ClanTremere) hediffDefToApply = VampDefOf.ROM_VampirismTremere;
                        if (def == VampDefOf.ROMV_ClanTzimize) hediffDefToApply = VampDefOf.ROM_VampirismTzimisce;
                        HealthUtility.AdjustSeverity(pawn, hediffDefToApply, 0.5f);

                        pawn.story.HairColor = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColor, 20);
                        var ticksToAdd = Rand.Range(GenDate.TicksPerYear, GenDate.TicksPerYear * 200);
                        pawn.ageTracker.AgeBiologicalTicks += ticksToAdd;
                        pawn.ageTracker.AgeChronologicalTicks += ticksToAdd;
                        if (pawn.health.hediffSet.hediffs is { } hediffs)
                        {
                            hediffs.RemoveAll(x => x.IsPermanent() ||
                                                   x.def == HediffDefOf.BadBack ||
                                                   x.def == HediffDefOf.Frail ||
                                                   x.def == HediffDefOf.Cataract ||
                                                   x.def == HediffDef.Named("HearingLoss") ||
                                                   x.def == HediffDef.Named("HeartArteryBlockage"));
                            if (hediffs.FirstOrDefault(x => x is HediffVampirism_VampGiver) is
                                HediffVampirism_VampGiver v)
                            {
                                var gen = generationRange.RandomInRange;
                                //Log.Message("Applied generation: " + gen);
                                v.Generation = gen;
                            }
                        }
                        //VampireGen.TryGiveVampirismHediff(pawn, generationRange.RandomInRange, def, null, false);
                    }
        }
    }
}