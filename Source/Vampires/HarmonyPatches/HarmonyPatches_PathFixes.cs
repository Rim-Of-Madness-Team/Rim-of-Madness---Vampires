using System;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire
{
    public static partial class HarmonyPatches
    {

        // Verse.ReachabilityUtility
        public static void CanReach_Vampire(ref bool __result, Pawn pawn, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBash = false, TraverseMode mode = TraverseMode.ByPawn)
        {
            var inBeastMentalState = pawn?.MentalStateDef == DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
            var inRestrictedSunlightAIMode = pawn?.VampComp()?.CurrentSunlightPolicy == SunlightPolicy.Restricted;
            var isDaylight = VampireUtility.IsDaylight(pawn);
            var isPlayerCharacter = pawn?.Faction == Faction.OfPlayerSilentFail;
            var isNotDrafted = !pawn?.Drafted ?? false;
            var destIsNotRoofed = !dest.Cell.Roofed(pawn?.MapHeld ?? Find.VisibleMap);
            if (__result && pawn.IsVampire() &&
                (inRestrictedSunlightAIMode || inBeastMentalState) &&
                isDaylight && isPlayerCharacter && isNotDrafted && destIsNotRoofed) __result = false;
        }

        //JobGiver_GetRest 
        public static void FindGroundSleepSpotFor_Vampire(Pawn pawn, ref IntVec3 __result)
        {
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn))
            {
                Map map = pawn.Map;
                for (int i = 0; i < 2; i++)
                {
                    int radius = i != 0 ? 12 : 4;
                    IntVec3 result;
                    if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander && x.Roofed(pawn.MapHeld)))
                    {
                        __result = result;
                        return;
                    }
                }
                __result = CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 4, pawn);
                return;
                //return CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 4, pawn);
            }
        }

        // RimWorld.JoyUtility
        public static void EnjoyableOutsideNow_Vampire(Pawn pawn, ref bool __result, StringBuilder outFailReason = null)
        {
            if (pawn.IsVampire() && pawn.IsSunRisingOrDaylight())
            {
                __result = false;
            }
        }

        // RimWorld.PawnUtility
        public static void KnownDangerAt_Vamp(IntVec3 c, Pawn forPawn, ref bool __result)
        {
            if (forPawn.IsVampire() && forPawn.MapHeld != null && forPawn.IsSunRisingOrDaylight() && !c.Roofed(forPawn.MapHeld))
            {
                __result = true;
                return;
            }
        }        
        
//        public static void Vamp_DontWanderStupid(IntVec3 c, Pawn pawn, IntVec3 root, Func<Pawn, IntVec3, bool> validator, int tryIndex, Danger maxDanger, ref bool __result)
//        {
//            if (__result && pawn != null && pawn.IsVampire() && pawn.Spawned && !c.Roofed(pawn.Map) &&
//                pawn.Map != null && pawn.IsSunRising())
//                __result = false;
//        }
//        
    }
}
