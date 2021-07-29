using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Misc(Harmony harmony)
        {

            // MISC
            ////////////////////////////////////////////////////////////////////////////////

            //BestAttackTarget
            //Presence Level Cooldowns
            harmony.Patch(AccessTools.Method(typeof(AttackTargetFinder), "BestAttackTarget"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(BestAttackTarget)));
            //Log.Message("69");
            //Caravan patches
            //harmony.Patch(AccessTools.Method(typeof(Dialog_FormCaravan), "CheckForErrors"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(CheckForErrors_Vampires)));
            //TODO Fixing



            ////Log.Message("71");
            //Allows skill adjustments
            harmony.Patch(AccessTools.Method(typeof(SkillRecord), "get_Level"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampLevel)));
            //Log.Message("72");
            //Patches to remove vampires from daylight raids.
            harmony.Patch(AccessTools.Method(typeof(Scenario), "Notify_PawnGenerated"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontGenerateVampsInDaylight)));
            //Log.Message("73");
            //Players can't slaughter temporary summons
            harmony.Patch(AccessTools.Method(typeof(Designator_Slaughter), "CanDesignateThing"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CantSlaughterTemps)), null);
            //Log.Message("74");
            //Allows scenarios to create longer/shorter days.
            harmony.Patch(AccessTools.Method(typeof(GenCelestial), "CelestialSunGlowPercent"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CelestialSunGlowPercent)));
            //Log.Message("75");
            //            //Vampires should not calculate the pain of their internal organs.
            //            harmony.Patch(AccessTools.Method(typeof(HediffSet), "CalculatePain"), null,
            //                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CalculatePain)));
            //Vampires do not need warm clothes alerts.
            harmony.Patch(AccessTools.Method(typeof(Alert_NeedWarmClothes), "GetReport"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontNeedWarmClothesReports)));
            //Log.Message("76");

            //Hides corpses of temporary things from the filter menus
            harmony.Patch(AccessTools.Method(typeof(Listing_TreeThingFilter), "Visible", new Type[] { typeof(ThingDef) }),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(CorpsesAreNotVisible)));
            //Log.Message("77");

            //Prevents the game from kicking players out of spawned maps when their vampire hides in a hidey hole.
            harmony.Patch(AccessTools.Method(typeof(MapPawns), "get_AnyPawnBlockingMapRemoval"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_LetVampiresKeepMapsOpen)));
            //Log.Message("78");

            //Vampires no longer suffer global work speed reduction at night.
            harmony.Patch(AccessTools.Method(typeof(StatPart_Glow), "FactorFromGlow"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresAlwaysWorkHard)));
            //Log.Message("79");

            //Vampire guests and visitors should leave after their time is passed.
            harmony.Patch(AccessTools.Method(typeof(LordMaker), "MakeNewLord"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresGuestTracker)));
            //Log.Message("80");



            harmony.Patch(AccessTools.Method(typeof(PawnAddictionHediffsGenerator),
                    "GenerateAddictionsAndTolerancesFor"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateAddictionsAndTolerancesFor_PreFix)), null);
            //Log.Message("85");

            harmony.Patch(AccessTools.Method(typeof(JobGiver_PackFood),
                    "TryGiveJob"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampsDontPackFood)), null);
            //Log.Message("86");

            harmony.Patch(AccessTools.Method(typeof(Pawn_DrugPolicyTracker),
                    nameof(Pawn_DrugPolicyTracker.ShouldTryToTakeScheduledNow)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresDontHaveDrugSchedules)), null);
            //Log.Message("87");

            // Patch PawnCapacitiesHandler.CapableOf to ignore lungs on vampires
            harmony.Patch(AccessTools.Method(typeof(PawnCapacitiesHandler),
                nameof(PawnCapacitiesHandler.CapableOf)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresDontNeedLungs)), null);

            // Attempt to patch DeathRattle's AddCustomHediffs method
            try
            {
                harmony.Patch(AccessTools.Method(AccessTools.TypeByName("DeathRattle.Harmony.ShouldBeDeadFromRequiredCapacityPatch"), "AddCustomHediffs"),
                    new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresDontNeedLungsDeathRattle)), null);
            }
            catch 
            {
            }
        }


        private static float AlertNeedWarmClothes_LowestTemperatureComing(Map map)
        {
            Twelfth twelfth = GenLocalDate.Twelfth(map);
            float a = GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth);
            for (int i = 0; i < 3; i++)
            {
                twelfth = twelfth.NextTwelfth();
                a = Mathf.Min(a, GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth));
            }

            return Mathf.Min(a, map.mapTemperature.OutdoorTemp);
        }


        public static void BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags,
            Predicate<Thing> validator, float minDist, float maxDist,
            IntVec3 locus, float maxTravelRadiusFromLocus, bool canBash, ref IAttackTarget __result)
        {
            if (searcher?.Thing is Pawn pSearch && __result?.Thing is Pawn p && p.IsVampire() &&
                p.VampComp().Sheet.Disciplines.FirstOrDefault(x => x.Def.defName == "ROMV_Presence") is Discipline d)
            {
                HediffDef defToApply = null;
                switch (d.Level)
                {
                    default:
                        break;
                    case 1:
                        defToApply = VampDefOf.ROMV_PresenceICooldownHediff;
                        break;
                    case 2:
                        defToApply = VampDefOf.ROMV_PresenceIICooldownHediff;
                        break;
                    case 3:
                        defToApply = VampDefOf.ROMV_PresenceIIICooldownHediff;
                        break;
                    case 4:
                        defToApply = VampDefOf.ROMV_PresenceIVCooldownHediff;
                        break;
                }

                if (defToApply != null)
                {
                    HealthUtility.AdjustSeverity(pSearch, defToApply, 1.0f);
                }
            }
        }



        public static void VampLevel(SkillRecord __instance, ref int __result)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!__instance.TotallyDisabled)
            {
                if (p.health.hediffSet.hediffs.FindAll(x =>
                        x.TryGetComp<HediffComp_SkillOffset>() is HediffComp_SkillOffset hSkill &&
                        hSkill.Props.skillDef == __instance.def) is List<Hediff> hediffs &&
                    !hediffs.NullOrEmpty())
                {
                    foreach (Hediff hediff in hediffs)
                    {
                        __result += hediff.TryGetComp<HediffComp_SkillOffset>().Props.offset;
                    }
                }
            }
        }


        // RimWorld.Scenario
        public static void Vamp_DontGenerateVampsInDaylight(Scenario __instance, Pawn pawn,
            PawnGenerationContext context)
        {
            if (!pawn.IsVampire())
                return;

            Map currentMap = Find.CurrentMap;
            if (currentMap == null)
                return;

            var recentVampires = Find.World.GetComponent<WorldComponent_VampireTracker>().recentVampires;
            if (VampireUtility.IsDaylight(currentMap) && pawn.Faction != Faction.OfPlayerSilentFail &&
                pawn?.health?.hediffSet?.hediffs is List<Hediff> hdiffs)
            {
                hdiffs.RemoveAll(x => x.def == VampDefOf.ROM_Vampirism);
                if (recentVampires?.ContainsKey(pawn) ?? false)
                {
                    recentVampires.Remove(pawn);
                }
            }
            else
            {
                //Log.Message("Added " + pawn.Label + " to recent vampires list");
                recentVampires?.Add(pawn, 1);
            }
        }


        // RimWorld.Designator_Slaughter
        public static bool Vamp_CantSlaughterTemps(Thing t, ref AcceptanceReport __result)
        {
            if (t is PawnTemporary)
            {
                __result = false;
                return false;
            }

            return true;
        }



        // RimWorld.GenCelestial
        public static void Vamp_CelestialSunGlowPercent(float latitude, int dayOfYear, float dayPercent,
            ref float __result)
        {
            if (Find.Scenario?.AllParts?.FirstOrDefault(x => x.def.scenPartClass == typeof(ScenPart_LongerNights)) is
                ScenPart_LongerNights p)
            {
                //////Log.Message("Sun glow adjusted");
                __result = Mathf.Clamp01(__result - p.nightsLength);
            }

            if (VampireSettings.Get.sunDimming > 0f)
            {
                __result = Mathf.Clamp01(__result - VampireSettings.Get.sunDimming);
            }
        }


        //Alert_NeedWarmClothes
        public static void Vamp_DontNeedWarmClothesReports(Alert_NeedWarmClothes __instance, ref AlertReport __result)
        {
            if (__result.culpritsTargets?.Count() > 0)
            {
                var vamps = __result.culpritsTargets.Where(x => x.Thing is Pawn y && y.IsVampire());
                if (vamps?.Count() > 0)
                {
                    var p = vamps.First().Thing;
                    float num = AlertNeedWarmClothes_LowestTemperatureComing(p.MapHeld);
                    var colonists = new List<Pawn>(p.MapHeld.mapPawns.FreeColonistsSpawned.Where(x => !x.IsVampire()));
                    if (!colonists.NullOrEmpty())
                    {
                        foreach (Pawn pawn in colonists)
                        {
                            if (pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true) > num)
                            {
                                __result = pawn;
                                return;
                            }
                        }
                    }

                    __result = false;
                    return;
                }
            }
        }

        // Verse.Listing_TreeThingFilter
        public static void CorpsesAreNotVisible(ThingDef td, ref bool __result)
        {
            if (td?.ingestible?.sourceDef?.thingClass == typeof(PawnTemporary))
                __result = false;
        }


        // Verse.MapPawns
        public static void
            get_LetVampiresKeepMapsOpen(MapPawns __instance, ref bool __result) //get_AnyPawnBlockingMapRemoval
        {
            //Find some hidey holes~~
            if (__result) return;
            //if (mapTimeoutTicks > 0)
            //{
            //    --mapTimeoutTicks;
            //    __result = true;
            //}            
            var list = __instance?.AllPawns?.FirstOrDefault()?.MapHeld?.listerThings
                           ?.ThingsOfDef(VampDefOf.ROMV_HideyHole) ?? null;
            if (list == null) return;

            //Anyone in a hidey hole? If so, don't kill the map.
            foreach (var t in list)
            {
                var hideyHole = t as Building_HideyHole;
                if (hideyHole != null && hideyHole.ContainedThing is Pawn p && !p.Dead)
                {
                    __result = true;
                }
            }

            //mapTimeoutTicks = 600;
        }

        // RimWorld.StatPart_Glow
        public static void VampiresAlwaysWorkHard(Thing t, ref float __result) //FactorFromGlow
        {
            if (t is Pawn p && p.IsVampire())
                __result = 1.0f;
        }



        public static Dictionary<Pawn, int> VampGuestCache = new Dictionary<Pawn, int>();

        public static void VampiresGuestTracker(Faction faction, LordJob lordJob, Map map,
            IEnumerable<Pawn> startingPawns, ref Lord __result)
        {
            //Only a few lords will have vampires with these issues.
            if (!(lordJob is LordJob_VisitColony) && !(lordJob is LordJob_AssistColony) &&
                !(lordJob is LordJob_TravelAndExit)) return;
            if (startingPawns == null || !startingPawns.Any()) return;

            foreach (var startingPawn in startingPawns)
            {
                if (startingPawn.IsVampire())
                {
                    if (HarmonyPatches.VampGuestCache.ContainsKey(startingPawn))
                    {
                        HarmonyPatches.VampGuestCache.Remove(startingPawn);
                    }

                    int curTicks = Find.TickManager.TicksGame;
                    HarmonyPatches.VampGuestCache.Add(startingPawn, curTicks);
                    //////Log.Message("Vampire tracking: " + startingPawn.Label + " " + curTicks);
                }
            }
        }


        //PawnAddictionHediffsGenerator
        public static bool GenerateAddictionsAndTolerancesFor_PreFix(Pawn pawn)
        {
            if (pawn.IsVampire())
                return false;
            return true;
        }


        //JobGiver_PackFood.TryGiveJob
        public static bool VampsDontPackFood(Pawn pawn, ref Job __result)
        {
            if (pawn.IsVampire())
            {
                __result = null;
                return false;
            }
            return true;
        }

        //Pawn_DrugPolicyTracker.ShouldTryToTakeScheduledNow
        public static bool VampiresDontHaveDrugSchedules(Pawn_DrugPolicyTracker __instance, ThingDef ingestible, ref bool __result)
        {
            if (__instance?.pawn?.IsVampire() == true)
            {
                __result = false;
                return false;
            }

            return true;

        }

        // Exit early is pawn is a vampire and we're checking Breathing.
        public static bool VampiresDontNeedLungs(PawnCapacityDef capacity, ref bool __result, Pawn ___pawn)
        {
            if (capacity == PawnCapacityDefOf.Breathing && ___pawn.IsVampire())
            {
                __result = true;
                return false;
            }
            return true;
        }

        // Exit early is pawn is a vampire and we're checking Breathing.
        public static bool VampiresDontNeedLungsDeathRattle(PawnCapacityDef pawnCapacityDef, ref bool __result, Pawn pawn)
        {
            if (pawnCapacityDef == PawnCapacityDefOf.Breathing && pawn.IsVampire())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
