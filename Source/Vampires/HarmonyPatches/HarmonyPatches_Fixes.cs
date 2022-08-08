using HarmonyLib;
using RimWorld;
using Verse;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Fixes(Harmony harmony)
        {

            // Allows vampires to meditate and recreate
            //Log.Message("Attempting to patch meditation");
            harmony.Patch(AccessTools.Method(typeof(MeditationUtility), nameof(MeditationUtility.CanMeditateNow)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampCanMeditateNow)));
            //Log.Message("Successfully patched meditation");
        }

        public static bool VampCanMeditateNow(ref bool __result, Pawn pawn)
        {
            if (pawn.IsVampire(true))
            {
                if (pawn.needs.rest != null && pawn.needs.rest.CurCategory >= RestCategory.VeryTired)
                {
                    __result = false;
                    return false;
                }
                if (pawn.needs.TryGetNeed<Need_Blood>().Starving)
                {
                    __result = false;
                    return false;
                }
                if (!pawn.Awake())
                {
                    __result = false;
                    return false;
                }
                if (pawn.health.hediffSet.BleedRateTotal <= 0f)
                {
                    if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
                    {
                        Pawn_TimetableTracker timetable = pawn.timetable;
                        if (((timetable != null) ? timetable.CurrentAssignment : null) != TimeAssignmentDefOf.Meditate)
                        {
                            __result = false;
                            return false;
                        }
                    }
                    if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
                    {
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }

            return true;
        }
    }
}
