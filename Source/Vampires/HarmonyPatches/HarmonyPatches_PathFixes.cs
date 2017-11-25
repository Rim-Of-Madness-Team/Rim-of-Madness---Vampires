using System;
using System.Collections.Generic;
using System.Linq;
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
            if (__result && pawn.IsVampire() && VampireUtility.IsDaylight(pawn) && (pawn.Faction == Faction.OfPlayerSilentFail && !pawn.Drafted))
            {
                if (!dest.Cell.Roofed(pawn.MapHeld)) __result = false;
            }
        }

        //JobGiver_GetRest 
        public static void FindGroundSleepSpotFor_Vampire(Pawn pawn, ref IntVec3 __result)
        {
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn))
            {
                Map map = pawn.Map;
                for (int i = 0; i < 2; i++)
                {
                    int radius = (i != 0) ? 12 : 4;
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
            if (pawn.IsVampire() && VampireUtility.IsDaylight(pawn))
            {
                __result = false;
            }
        }

        // RimWorld.PawnUtility
        public static void KnownDangerAt_Vamp(IntVec3 c, Pawn forPawn, ref bool __result)
        {
            if (forPawn.IsVampire() && forPawn.MapHeld != null && VampireUtility.IsDaylight(forPawn) && !c.Roofed(forPawn.MapHeld))
            {
                __result = true;
                return;
            }
        }
    }
}
