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
        public static void HarmonyPatches_Pathing(Harmony harmony)
        {
            // PATHING
            //////////////////////////////////////////////////////////////////////////////
            //The wander handler now makes vampires wander indoors (for their safety).
            //            harmony.Patch(AccessTools.Method(typeof(RimWorld.RCellFinder), "CanWanderToCell"), null,
            //                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontWanderStupid)));            
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), "KnownDangerAt"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(KnownDangerAt_Vamp)));
            //Log.Message("07");
            harmony.Patch(
                AccessTools.Method(typeof(JoyUtility), "EnjoyableOutsideNow",
                    new Type[] { typeof(Pawn), typeof(StringBuilder) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(EnjoyableOutsideNow_Vampire)));
            //Log.Message("08");
            harmony.Patch(AccessTools.Method(typeof(JobGiver_GetRest), "FindGroundSleepSpotFor"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FindGroundSleepSpotFor_Vampire)));
            //Log.Message("09");
            harmony.Patch(AccessTools.Method(typeof(JobGiver_TakeCombatEnhancingDrug), "TryGiveJob"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveJob_DrugGiver_Vampire)), null);
            //Log.Message("10");
            harmony.Patch(
                AccessTools.Method(typeof(Verse.ReachabilityUtility), "CanReach",
                    new Type[]
                    {
                        typeof(Pawn), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(Danger), typeof(bool), typeof(bool),
                        typeof(TraverseMode)
                    }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(CanReach_Vampire)));
            //Log.Message("11");
            harmony.Patch(
                AccessTools.Method(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(IntVec3), typeof(Pawn) }),
                null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IsForbidden)));
            //Log.Message("12");

        }


        // Verse.ReachabilityUtility
        public static void CanReach_Vampire(ref bool __result, Pawn pawn, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBashDoors = false, bool canBashFences = false, TraverseMode mode = TraverseMode.ByPawn)
        {
            var inBeastMentalState = pawn?.MentalStateDef == DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
            var inRestrictedSunlightAIMode = pawn?.VampComp()?.CurrentSunlightPolicy == SunlightPolicy.Restricted;
            var isDaylight = VampireUtility.IsDaylight(pawn);
            var isPlayerCharacter = pawn?.Faction == Faction.OfPlayerSilentFail;
            var isNotDrafted = !pawn?.Drafted ?? false;
            var destIsNotRoofed = !dest.Cell.Roofed(pawn?.MapHeld ?? Find.CurrentMap);
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
                __result = CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
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


        public static bool TryGiveJob_DrugGiver_Vampire(Pawn pawn, ref Job __result)
        {
            if (pawn.IsVampire())
            {
                __result = null;
                return false;
            }

            return true;
        }

        // RimWorld.ForbidUtility
        public static void Vamp_IsForbidden(IntVec3 c, Pawn pawn, ref bool __result)
        {
            if (pawn.IsVampire() &&
                (pawn.VampComp().CurrentSunlightPolicy != SunlightPolicy.NoAI && VampireUtility.IsDaylight(pawn)) &&
                !c.Roofed(pawn.Map))
                __result = true;
        }

    }
}
