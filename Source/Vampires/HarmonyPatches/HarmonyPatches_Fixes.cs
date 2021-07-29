using HarmonyLib;
using RimWorld;
using Verse;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Fixes(Harmony harmony) {

			// Allows vampires to meditate and recreate
			Log.Message("Attempting to patch meditation");
            harmony.Patch(AccessTools.Method(typeof(MeditationUtility), "CanMeditateNow"),
				new HarmonyMethod(typeof(HarmonyPatches), nameof(VampCanMeditateNow)));
			Log.Message("Successfully patched meditation");
        }

		public static bool VampCanMeditateNow(ref bool __result, Pawn pawn) {
			bool flag = pawn.IsVampire();
			bool result;
			if (flag) {
				bool flag2 = pawn.needs.rest != null && pawn.needs.rest.CurCategory >= RestCategory.VeryTired;
				if (flag2) {
					__result = false;
					result = false;
				} else {
					bool starving = pawn.needs.TryGetNeed<Need_Blood>().Starving;
					if (starving) {
						__result = false;
						result = false;
					} else {
						bool flag3 = !pawn.Awake();
						if (flag3) {
							__result = false;
							result = false;
						} else {
							bool flag4 = pawn.health.hediffSet.BleedRateTotal <= 0f;
							if (flag4) {
								bool flag5 = HealthAIUtility.ShouldSeekMedicalRest(pawn);
								if (flag5) {
									Pawn_TimetableTracker timetable = pawn.timetable;
									bool flag6 = ((timetable != null) ? timetable.CurrentAssignment : null) != TimeAssignmentDefOf.Meditate;
									if (flag6) {
										__result = false;
										return false;
									}
								}
								bool flag7 = !HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn);
								if (flag7) {
									__result = true;
									return false;
								}
							}
							__result = false;
							result = false;
						}
					}
				}
			} else {
				result = true;
			}
			return result;
		}
	}
}
