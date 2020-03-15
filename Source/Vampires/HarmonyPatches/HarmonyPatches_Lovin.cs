using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Lovin(Harmony harmony)
        {
            // LOVIN
            ////////////////////////////////////////////////////////////////////////////////////////////
            //Vampires should not worry about sleeping in the same coffin.
            harmony.Patch(
                AccessTools.Method(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FineSleepingAlone)), null);
            ////Log.Message("20");
            //Vampires had trouble with lovin' due to a food check.
            harmony.Patch(AccessTools.Method(typeof(LovePartnerRelationUtility), "GetLovinMtbHours"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_LovinFoodFix)), null);
            ////Log.Message("21");
        }


        // RimWorld.ThoughtWorker_WantToSleepWithSpouseOrLover
        public static void Vamp_FineSleepingAlone(Pawn p, ref ThoughtState __result)
        {
            if (p != null && p.IsVampire())
                __result = false;
        }


        // RimWorld.LovePartnerRelationUtility
        public static bool Vamp_LovinFoodFix(Pawn pawn, Pawn partner, ref float __result)
        {
            if (pawn.IsVampire() || partner.IsVampire())
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

                if (pawn?.needs?.food is Need_Food food && food.Starving ||
                    partner?.needs?.food is Need_Food foodPartner && foodPartner.Starving)
                {
                    __result = -1f;
                    return false;
                }

                if (pawn?.health?.hediffSet?.BleedRateTotal > 0f || partner?.health?.hediffSet?.BleedRateTotal > 0f)
                {
                    __result = -1f;
                    return false;
                }

                float num = LovinMtbSinglePawnFactor(pawn);
                if (num <= 0f)
                {
                    __result = -1f;
                    return false;
                }

                float num2 = LovinMtbSinglePawnFactor(partner);
                if (num2 <= 0f)
                {
                    __result = -1f;
                    return false;
                }

                float num3 = 12f;
                num3 *= num;
                num3 *= num2;
                num3 /= Mathf.Max(pawn.relations.SecondaryRomanceChanceFactor(partner), 0.1f);
                num3 /= Mathf.Max(partner.relations.SecondaryRomanceChanceFactor(pawn), 0.1f);
                num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, (float)pawn.relations.OpinionOf(partner));
                __result = num3 * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f,
                               (float)partner.relations.OpinionOf(pawn));
                return false;
            }

            return true;
        }



        // RimWorld.LovePartnerRelationUtility
        private static float LovinMtbSinglePawnFactor(Pawn pawn)
        {
            try
            {
                float num = 1f;
                var num2 = pawn?.health?.hediffSet?.PainTotal ?? -1f;
                if (num2 > -1f)
                {
                    num /= 1f - num2;
                    if (pawn?.health?.capacities?.GetLevel(PawnCapacityDefOf.Consciousness) != null)
                    {
                        float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
                        if (level < 0.5f)
                        {
                            num /= level * 2f;
                        }
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
}
