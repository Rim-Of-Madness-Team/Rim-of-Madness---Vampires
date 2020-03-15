using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Vampire
{
    static partial class HarmonyPatches
    {

        public static void HarmonyPatches_Caravan(Harmony harmony)
        {

            ////Log.Message("70");
            harmony.Patch(AccessTools.Method(typeof(Caravan), "get_NightResting"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_Resting_Vampires)));


            //Checks for food in caravans (Prefix)
            harmony.Patch(AccessTools.Method(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood",
                    new Type[]
                    {
                        typeof(List<TransferableOneWay>), typeof(int),
                        typeof(IgnorePawnsInventoryMode), typeof(Faction),
                        typeof(WorldPath), typeof(float), typeof(int)
                    }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ApproxDaysWorthOfFood_PreFix)), null);
            //Log.Message("81");
            //
            //
            //            //Checks for food in caravans (Postfix)
            harmony.Patch(AccessTools.Method(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood",
                    new Type[]
                    {
                        typeof(List<TransferableOneWay>), typeof(int),
                        typeof(IgnorePawnsInventoryMode), typeof(Faction),
                        typeof(WorldPath), typeof(float), typeof(int)
                    }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ApproxDaysWorthOfFood_PostFix)));
            //Log.Message("82");

            //Checks for food in caravans (Prefix)
            harmony.Patch(AccessTools.Method(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood",
                    new Type[]
                    {
                        typeof(List<Pawn>), typeof(List<ThingDefCount>),
                        typeof(int), typeof(IgnorePawnsInventoryMode),
                        typeof(Faction), typeof(WorldPath), typeof(float),
                        typeof(int), typeof(bool)
                    }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ApproxDaysWorthOfFoodPawns_PreFix)), null);
            //Log.Message("83");
            //
            //
            //            //Checks for food in caravans (Postfix)
            harmony.Patch(AccessTools.Method(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood",
                    new Type[]
                    {
                        typeof(List<Pawn>), typeof(List<ThingDefCount>),
                        typeof(int), typeof(IgnorePawnsInventoryMode),
                        typeof(Faction), typeof(WorldPath), typeof(float),
                        typeof(int), typeof(bool)
                    }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(ApproxDaysWorthOfFoodPawns_PostFix)));
            //Log.Message("84");

        }

        // RimWorld.Dialog_FormCaravan
        public static void CheckForErrors_Vampires(List<Pawn> pawns, ref bool __result)
        {
            if (pawns.Any(x => x.IsVampire() && pawns.Any(y => ((y?.RaceProps?.Humanlike ?? false) && !y.IsVampire()))))
            {
                Messages.Message("ROMV_Caravan_WarningMixedWithVampires".Translate(), MessageTypeDefOf.RejectInput);
                __result = false;
                return;
            }
        }

        //Caravan
        public static bool get_Resting_Vampires(Caravan __instance, ref bool __result)
        {
            int countNonVampHumanoids = __instance.PawnsListForReading.Count(x => !x.NonHumanlikeOrWildMan() && !x.IsVampire());
            int countVampHumanoids = __instance.PawnsListForReading.Count(x => !x.NonHumanlikeOrWildMan() && x.IsVampire());

            //In a mixed caravan, pawns can travel during the day.
            if (countNonVampHumanoids >= countVampHumanoids)
            {
                return true;
            }

            //In a vampire only caravan, they can travel only at night.
            float num = GenDate.HourFloat((long)GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(__instance.Tile).x);
            __result = num >= 6f && num <= 17f;
            return false;
        }

        private static List<TransferableOneWay> caravanTransferrables = new List<TransferableOneWay>();

        //public static class DaysWorthOfFoodCalculator
        //{
        public static void ApproxDaysWorthOfFood_PreFix(ref List<TransferableOneWay> transferables, int tile,
            IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null,
            float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
        {
            if (!transferables.NullOrEmpty())
            {
                caravanTransferrables = new List<TransferableOneWay>();
                caravanTransferrables.AddRange(transferables.FindAll(x =>
                    x.HasAnyThing && x.AnyThing is Pawn y && y.IsVampire()));
                transferables.RemoveAll(x => x.HasAnyThing && x.AnyThing is Pawn y && y.IsVampire());
            }
        }

        //public static class DaysWorthOfFoodCalculator
        //{
        public static void ApproxDaysWorthOfFood_PostFix(ref List<TransferableOneWay> transferables, int tile,
            IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null,
            float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
        {
            if (transferables == null)
            {
                transferables = new List<TransferableOneWay>();
            }

            transferables.AddRange(caravanTransferrables);
        }

        private static List<Pawn> caravanVampires = new List<Pawn>();

        //public static class DaysWorthOfFoodCalculator
        //{
        public static void ApproxDaysWorthOfFoodPawns_PreFix(ref List<Pawn> pawns, List<ThingDefCount> extraFood,
            int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path,
            float nextTileCostLeft, int caravanTicksPerMove, bool assumeCaravanMoving)
        {
            if (!pawns.NullOrEmpty())
            {
                caravanVampires = new List<Pawn>();
                caravanVampires.AddRange(pawns.FindAll(x => x.IsVampire()));
                pawns.RemoveAll(x => x.IsVampire());
            }
        }

        //public static class DaysWorthOfFoodCalculator
        //{
        public static void ApproxDaysWorthOfFoodPawns_PostFix(ref List<Pawn> pawns, List<ThingDefCount> extraFood,
            int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path,
            float nextTileCostLeft, int caravanTicksPerMove, bool assumeCaravanMoving)
        {
            if (pawns == null)
            {
                pawns = new List<Pawn>();
            }

            pawns.AddRange(caravanVampires);
        }
    }
}