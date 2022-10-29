using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Lovin(Harmony harmony)
    {
        // LOVIN
        ////////////////////////////////////////////////////////////////////////////////////////////
        //Vampires should not worry about sleeping in the same coffin.
        harmony.Patch(
            AccessTools.Method(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FineSleepingAlone)));
        ////Log.Message("20");
        //Vampires had trouble with lovin' due to a food check.
        harmony.Patch(AccessTools.Method(typeof(LovePartnerRelationUtility), "GetLovinMtbHours"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_LovinFoodFix)));
        ////Log.Message("21");
    }


    // RimWorld.ThoughtWorker_WantToSleepWithSpouseOrLover
    public static void Vamp_FineSleepingAlone(Pawn p, ref ThoughtState __result)
    {
        if (p != null && p.IsVampire(true))
            __result = false;
    }


    // RimWorld.LovePartnerRelationUtility
    public static bool Vamp_LovinFoodFix(Pawn pawn, Pawn partner, ref float __result)
    {
        if (pawn.IsVampire(true) || partner.IsVampire(true))
        {
            if (pawn.Dead || partner.Dead)
            {
                __result = -1f;
                return false;
            }

            if (DebugSettings.alwaysDoLovin)
            {
                __result = 0.1f;
                return false;
            }

            if ((pawn?.needs?.food is { } food && food.Starving) ||
                (partner?.needs?.food is { } foodPartner && foodPartner.Starving))
            {
                __result = -1f;
                return false;
            }

            if (pawn?.health?.hediffSet?.BleedRateTotal > 0f || partner?.health?.hediffSet?.BleedRateTotal > 0f)
            {
                __result = -1f;
                return false;
            }

            var num = LovinMtbSinglePawnFactor(pawn);
            if (num <= 0f)
            {
                __result = -1f;
                return false;
            }

            var num2 = LovinMtbSinglePawnFactor(partner);
            if (num2 <= 0f)
            {
                __result = -1f;
                return false;
            }

            var num3 = 12f;
            num3 *= num;
            num3 *= num2;
            num3 /= Mathf.Max(pawn.relations.SecondaryRomanceChanceFactor(partner), 0.1f);
            num3 /= Mathf.Max(partner.relations.SecondaryRomanceChanceFactor(pawn), 0.1f);
            num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner));
            __result = num3 * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f,
                partner.relations.OpinionOf(pawn));
            return false;
        }

        return true;
    }


    // RimWorld.LovePartnerRelationUtility
    private static float LovinMtbSinglePawnFactor(Pawn pawn)
    {
        try
        {
            var num = 1f;
            var num2 = pawn?.health?.hediffSet?.PainTotal ?? -1f;
            if (num2 > -1f)
            {
                num /= 1f - num2;
                if (pawn?.health?.capacities?.GetLevel(PawnCapacityDefOf.Consciousness) != null)
                {
                    var level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
                    if (level < 0.5f) num /= level * 2f;
                }

                return num / GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f,
                    pawn.ageTracker.AgeBiologicalYearsFloat);
            }
        }
        catch (Exception e)
        {
            return -1f;
        }

        return -1f;
    }
}