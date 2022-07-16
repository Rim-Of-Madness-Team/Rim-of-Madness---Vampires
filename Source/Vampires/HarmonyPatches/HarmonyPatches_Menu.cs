using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Menu(Harmony harmony)
        {

            #region Menus / On-Screen Messages / Alerts

            // MENUS / ON-SCREEN MESSAGES / ALERTS
            ////////////////////////////////////////////////////////////////////////////////////

            //Log.Message("48");
            //Adds debug/dev tools for making vampires.

            //Removed from RimWorld 1.1. Patch is no longer needed as it has been implemented into the base game.

            //harmony.Patch(AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoListingItems_MapTools"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(DoListingItems_MapTools_Vamp)));
            //Log.Message("49");
            //Adds blood extraction/transfer recipes to all living organisms
            harmony.Patch(AccessTools.Method(typeof(ThingDef), "get_AllRecipes"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_AllRecipes_BloodFeedable)));
            //Log.Message("50");
            //Adds blood extraction recipes to all living organisms
            harmony.Patch(AccessTools.Method(typeof(Bill_Medical), "Notify_DoBillStarted"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Notify_DoBillStarted_Debug)), null);
            //Log.Message("51");
            //The Doctor alert will no longer check a vampire to see if it's fed.
            harmony.Patch(AccessTools.Method(typeof(Alert_NeedDoctor), "get_Patients"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_Patients_Vamp)), null);
            //Log.Message("52");
            //Shows the atrophied organs of the vampire as unused.
            harmony.Patch(AccessTools.Method(typeof(HealthCardUtility), "GetPawnCapacityTip"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GetPawnCapacityTip)));
            //Log.Message("53");
            harmony.Patch(AccessTools.Method(typeof(HealthCardUtility), "GetEfficiencyLabel"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GetEfficiencyLabel)));
            //Log.Message("54");
            //Vampire player should know about the rest curse.
            harmony.Patch(AccessTools.Method(typeof(Need), "GetTipString"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_RestTextToolTip)));
            //Log.Message("55");

            #endregion


        }




        // Verse.ThingDef
        public static void get_AllRecipes_BloodFeedable(ThingDef __instance, ref List<RecipeDef> __result)
        {
            if (!__result.NullOrEmpty())
            {
                if ((__instance?.race?.Animal ?? false) || (__instance?.race?.Humanlike ?? false))
                {
                    if (!__result.Contains(VampDefOf.ROMV_ExtractBloodVial))
                        __result.Add(VampDefOf.ROMV_ExtractBloodVial);
                    if (!__result.Contains(VampDefOf.ROMV_ExtractBloodPack))
                        __result.Add(VampDefOf.ROMV_ExtractBloodPack);
                    if (!__result.Contains(VampDefOf.ROMV_ExtractBloodWine))
                        __result.Add(VampDefOf.ROMV_ExtractBloodWine);

                    //Add blood transfer for humanlikes
                    if (__instance?.race?.Humanlike ?? false)
                    {

                        if (!__result.Contains(VampDefOf.ROM_TransferBloodPack))
                            __result.Add(VampDefOf.ROM_TransferBloodPack);
                    }

                    //Add blood transfer for animals
                    if (__instance?.race?.Animal ?? false)
                    {
                        if (!__result.Contains(VampDefOf.ROM_TransferBloodPackAnimal))
                            __result.Add(VampDefOf.ROM_TransferBloodPackAnimal);
                    }
                }
            }
        }


        //Bill_Medical
        public static bool Notify_DoBillStarted_Debug(Bill_Medical __instance, Pawn billDoer)
        {
            if (__instance.recipe == VampDefOf.ROMV_ExtractBloodPack ||
                __instance.recipe == VampDefOf.ROMV_ExtractBloodVial ||
                __instance.recipe == VampDefOf.ROMV_ExtractBloodWine)
            {
                //Pawn GiverPawn = (Pawn)AccessTools.Method(typeof(Bill_Medical), "get_GiverPawn").Invoke(__instance, null);
                //if (!GiverPawn.Dead && __instance.recipe.anesthetize && HealthUtility.TryAnesthetize(GiverPawn))
                //{

                //}
                return false;
            }

            return true;
        }



        //public class Alert_NeedDoctor : Alert
        public static bool get_Patients_Vamp(ref IEnumerable<Pawn> __result)
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    HashSet<Pawn> pawns = new HashSet<Pawn>(maps[i].mapPawns.FreeColonistsSpawned);
                    List<Pawn> Patients = new List<Pawn>();
                    if (pawns != null && pawns.Count > 0 && pawns.FirstOrDefault(x => x.IsVampire(true)) != null)
                    {
                        if (pawns.FirstOrDefault(x =>
                                !x.Downed && x.workSettings != null &&
                                x.workSettings.WorkIsActive(WorkTypeDefOf.Doctor)) != null)
                        {
                            foreach (Pawn p2 in pawns)
                            {
                                if (p2.IsVampire(true))
                                {
                                    if (HealthAIUtility.ShouldBeTendedNowByPlayer(p2))
                                        Patients.Add(p2);
                                }
                                else
                                {
                                    if (p2.Downed &&
                                        (p2?.needs?.food?.CurCategory ?? HungerCategory.Fed) < HungerCategory.Fed &&
                                        p2.InBed() || HealthAIUtility.ShouldBeTendedNowByPlayer(p2))
                                    {
                                        Patients.Add(p2);
                                    }
                                }
                            }
                        }

                        __result = null;
                        __result = Patients;
                        return false;
                    }
                }
            }

            return true;
        }


        // RimWorld.HealthCardUtility
        public static void Vamp_GetPawnCapacityTip(Pawn pawn, PawnCapacityDef capacity, ref string __result)
        {
            if (pawn.IsVampire(true) &&
                (
                    capacity == PawnCapacityDefOf.Breathing ||
                    capacity == PawnCapacityDefOf.BloodPumping ||
                    capacity == PawnCapacityDefOf.BloodFiltration ||
                    capacity == PawnCapacityDefOf.Eating ||
                    capacity == PawnCapacityDefOf.Metabolism))
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine(capacity.LabelCap + ": 0%");
                s.AppendLine();
                s.AppendLine("AffectedBy".Translate());
                s.AppendLine("  " + "ROMV_HI_Vampirism".Translate());
                s.AppendLine("  " + "ROMV_HI_UnusedCapacities".Translate().AdjustedFor(pawn));
                __result = s.ToString();
            }
        }


        // RimWorld.HealthCardUtility
        public static void GetEfficiencyLabel(ref Pair<string, Color> __result, Pawn pawn, PawnCapacityDef activity)
        {
            if (pawn.IsVampire(true) &&
                (
                    activity == PawnCapacityDefOf.Breathing ||
                    activity == PawnCapacityDefOf.BloodPumping ||
                    activity == PawnCapacityDefOf.BloodFiltration ||
                    activity == PawnCapacityDefOf.Eating ||
                    activity == PawnCapacityDefOf.Metabolism))
            {
                __result = new Pair<string, Color>("ROMV_HI_Unused".Translate(), VampireUtility.VampColor);
            }
        }


        //Need
        public static void Vamp_RestTextToolTip(Need __instance, ref string __result)
        {
            if (__instance is Need_Rest)
            {
                Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);
                if (pawn != null && pawn.IsVampire(true))
                {
                    StringBuilder s = new StringBuilder();
                    s.Append(__result);
                    s.AppendLine("\n\n" + "ROMV_RestAddedTip".Translate());
                    __result = s.ToString();
                }
            }
        }

    }
}
