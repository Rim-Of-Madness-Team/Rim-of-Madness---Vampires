using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Vampire
{
    static partial class HarmonyPatches
    {
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
        public static void get_Resting_Vampires(Caravan __instance, ref bool __result)
        {
            if (__instance.PawnsListForReading.Any(x => x.IsVampire()))
            {
                float num = GenDate.HourFloat((long) GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(__instance.Tile).x);
                __result = num >= 6f && num <= 17f;
            }
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