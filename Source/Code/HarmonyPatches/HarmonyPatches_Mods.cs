using System;
using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Mods(Harmony harmony)
    {
        // MODS
        ///////////////////////////////////////////////////////////////////////////////////

        {
            try
            {
                ((Action)(() =>
                {
                    if (AccessTools.Method(typeof(Need_Bladder), nameof(Need_Bladder.crapPants)) != null)
                        harmony.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"),
                            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoBladderNeed)));


                    if (AccessTools.Method(typeof(JobGiver_UseToilet), "TryGiveJob") != null)
                        harmony.Patch(AccessTools.Method(typeof(JobGiver_UseToilet), "TryGiveJob"),
                            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoBladderNeedDoubleUp)));
                })).Invoke();
            }
#pragma warning disable 168
            catch (TypeLoadException ex)
            {
                /*////Log.Message(ex.ToString());*/
            }
#pragma warning restore 168
        }
    }


    // RimWorld.Pawn_NeedsTracker
    private static bool Vamp_NoBladderNeed(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn").GetValue(__instance);
        if (pawn.IsVampire(true))
        {
            if (nd.defName == "Bladder")
            {
                __result = false;
                return false;
            }

            if (nd.defName == "DBHThirst")
            {
                __result = false;
                return false;
            }
        }

        return true;
    }


    private static bool Vamp_NoBladderNeedDoubleUp(Pawn pawn, ref Job __result)
    {
        if (!pawn.IsVampire(true)) return true;
        __result = null;
        return false;
    }

    // DubsBadHygiene.dubUtils
    public static void Vamp_StopThePoopStorm(Pawn pawn, ref bool __result)
    {
        if (pawn.IsVampire(true)) __result = true;
    }


    // RimWorld.Pawn_NeedsTracker
    private static void Vamp_FullBladder(Pawn_NeedsTracker __instance, ref float __result)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn").GetValue(__instance);
        if (pawn.IsVampire(true)) __result = 1.0f;
    }


    // RimWorld.ForbidUtility
    public static void Vamp_StopThePoopStorm(IntVec3 c, Pawn pawn, ref bool __result)
    {
        if (pawn.IsVampire(true) && VampireUtility.IsDaylight(pawn) && !c.Roofed(pawn.Map))
            __result = true;
    }
}