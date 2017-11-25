using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections;
using Harmony;

namespace Vampire
{
    public class ScenPart_StartingVampires : ScenPart
    {
        private bool randomBloodline = false;
        private BloodlineDef bloodline = VampDefOf.ROMV_ClanTremere;
        private float vampChance = 0.5f;
        private int maxVampires = 1;
        private string maxVampiresBuf = "";
        private int curVampires = 0;
        private IntRange generationRange = new IntRange(10, 13);
        private int maxGeneration = 15;
        private bool spawnInCoffins = false;
        

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 5f + 31f);
            if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), this?.bloodline?.LabelCap ?? "ROMV_UnknownBloodline".Translate(), true, false, true))
            {
                FloatMenuUtility.MakeMenu<BloodlineDef>(this.PossibleBloodlines(), (BloodlineDef bl) => bl.LabelCap, (BloodlineDef bl) => delegate
                {
                    this.bloodline = bl;
                });
            }
            //Widgets.IntRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref this.generationRange, 4, this.maxGeneration, "ROMV_VampireGeneration");
            //DoVampModifierEditInterface(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f));
        }

        // RimWorld.ScenPart_PawnModifier
        protected void DoVampModifierEditInterface(Rect rect)
        {
            Rect rect2 = new Rect(rect.x, rect.y + ScenPart.RowHeight * 2, rect.width, 31);
            Rect rect3 = rect2.LeftPart(0.333f).Rounded();
            Rect rect4 = rect2.RightPart(0.666f).Rounded();

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect3, "ROMV_Chance".Translate());

            Text.Anchor = TextAnchor.UpperLeft;
            this.vampChance = Widgets.HorizontalSlider(rect4, this.vampChance, 0f, 1f, false, "", "", "");
            //Widgets.TextFieldNumeric<float>(rect4, ref this, ref this.numOfVampsBuffer, 1, 50);
            Rect rect5 = new Rect(rect.x, rect.y + ScenPart.RowHeight * 3, rect.width, 31);
            Rect rect6 = rect5.LeftPart(0.333f).Rounded();
            Rect rect7 = rect5.RightPart(0.666f).Rounded();

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect6, "ROMV_StartInCoffins".Translate());

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.CheckboxLabeled(rect7, "", ref this.spawnInCoffins, false);
            Rect rect8 = new Rect(rect.x, rect.y + ScenPart.RowHeight * 4, rect.width, 31);
            Rect rect9 = rect8.LeftPart(0.666f).Rounded();
            Rect rect10 = rect8.RightPart(0.333f).Rounded();
            Text.Anchor = TextAnchor.MiddleRight;

            Widgets.Label(rect9, "ROMV_MaxVampires".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric<int>(rect10, ref this.maxVampires, ref this.maxVampiresBuf, 1, 100);
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
            Scribe_Defs.Look<BloodlineDef>(ref this.bloodline, "bloodline");
            Scribe_Values.Look<IntRange>(ref this.generationRange, "generationRange", default(IntRange), false);
            Scribe_Values.Look<float>(ref this.vampChance, "vampChance", 0.5f);
            Scribe_Values.Look<int>(ref this.maxVampires, "maxVampires", 1);
            Scribe_Values.Look<bool>(ref this.spawnInCoffins, "spawnInCoffins", false);
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
            return (this?.vampChance == 1.0f) ? maxVampires.ToString() : "ROMV_ChanceOf".Translate(vampChance.ToStringPercent());
        }

        public string GetInCoffinsString()
        {
            return (this?.spawnInCoffins ?? false) ? " " + "ROMV_StartInCoffins".Translate() : "";
        }

        public string GenRangeToString()
        {
            return (HediffVampirism.AddOrdinal(this?.generationRange.min ?? 10) + "-" + HediffVampirism.AddOrdinal(this?.generationRange.max ?? 15)) ?? "";
        }

        public override void Randomize()
        {
            base.Randomize();
            this.vampChance = Rand.Range(0.2f, 0.8f);
            this.bloodline = this.PossibleBloodlines().RandomElement<BloodlineDef>();
            this.generationRange.max = Rand.Range(10, 15);
            this.generationRange.min = Rand.Range(8, this.generationRange.max);
            this.spawnInCoffins = (Rand.Value > 0.3) ? true : false;
        }
        
        public override void PostMapGenerate(Map map)
        {
            if (Find.GameInitData == null)
            {
                return;
            }
            if (spawnInCoffins)
            {
                bool usingDropPods = Find.Scenario.AllParts.Any(x => x is ScenPart_PlayerPawnsArriveMethod s && ((PlayerPawnsArriveMethod)AccessTools.Field(typeof(ScenPart_PlayerPawnsArriveMethod), "method").GetValue(s)) == PlayerPawnsArriveMethod.DropPods);
                List<List<Thing>> list = new List<List<Thing>>();
                foreach (Pawn current in Find.GameInitData.startingPawns)
                {
                    if (current.RaceProps.Humanlike && current?.health?.hediffSet?.hediffs.FirstOrDefault(y => y.def.defName.Contains("Vampirism")) != null)
                    {
                        IntVec3 loc = current.PositionHeld;
                        Building_Casket casket = (Building_Casket)ThingMaker.MakeThing(VampDefOf.ROMV_RoyalCoffin, ThingDefOf.WoodLog);
                        casket.SetFaction(Faction.OfPlayer);
                        if (current.Spawned) current.DeSpawn();
                        if (current.holdingOwner != null) current.holdingOwner.Take(current);
                        if (!usingDropPods) GenPlace.TryPlaceThing(casket, loc, map, ThingPlaceMode.Near);
                        casket.GetDirectlyHeldThings().TryAdd(current);
                        if (usingDropPods) list.Add(new List<Thing> { casket });
                    }
                }
                bool instaDrop = Find.GameInitData.QuickStarted;
                if (usingDropPods)
                {
                    DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, instaDrop, true, true);
                }
            }
        }
        
        public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context)
        {
            if (Find.VisibleMap == null)
            {
                curVampires = Find.GameInitData.startingPawns.FindAll(x => x?.health?.hediffSet?.hediffs.FirstOrDefault(y => y.def.defName.Contains("Vampirism")) != null)?.Count() ?? 0;
                BloodlineDef def = (randomBloodline) ? PossibleBloodlines().RandomElement() : bloodline;
                if (pawn.RaceProps.Humanlike && context == PawnGenerationContext.PlayerStarter)
                {
                    if (!pawn?.story?.WorkTagIsDisabled(WorkTags.Violent) ?? false)
                    {
                        if (Rand.Value < vampChance && curVampires < maxVampires)
                        {
                            curVampires++;
                            HediffDef hediffDefToApply = VampDefOf.ROM_VampirismRandom;
                            if (def == VampDefOf.ROMV_ClanGargoyle) hediffDefToApply = VampDefOf.ROM_VampirismGargoyle;
                            if (def == VampDefOf.ROMV_ClanLasombra) hediffDefToApply = VampDefOf.ROM_VampirismLasombra;
                            if (def == VampDefOf.ROMV_ClanPijavica) hediffDefToApply = VampDefOf.ROM_VampirismPijavica;
                            if (def == VampDefOf.ROMV_ClanTremere) hediffDefToApply = VampDefOf.ROM_VampirismTremere;
                            if (def == VampDefOf.ROMV_ClanTzimize) hediffDefToApply = VampDefOf.ROM_VampirismTzimisce;
                            HealthUtility.AdjustSeverity(pawn, hediffDefToApply, 0.5f);
                            pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, 20);
                            int ticksToAdd = Rand.Range(GenDate.TicksPerYear, GenDate.TicksPerYear * 200);
                            pawn.ageTracker.AgeBiologicalTicks += ticksToAdd;
                            pawn.ageTracker.AgeChronologicalTicks += ticksToAdd;
                            if (pawn.health.hediffSet.hediffs is List<Hediff> hediffs)
                            {
                                hediffs.RemoveAll(x => x.IsOld() ||
                                x.def == HediffDefOf.BadBack ||
                                x.def == HediffDefOf.Frail ||
                                x.def == HediffDefOf.Cataract ||
                                x.def == HediffDef.Named("HearingLoss") ||
                                x.def == HediffDef.Named("HeartArteryBlockage"));
                            }
                            //VampireGen.TryGiveVampirismHediff(pawn, generationRange.RandomInRange, def, null, false);
                        }
                    }
                }
            }
        }
    }
}
