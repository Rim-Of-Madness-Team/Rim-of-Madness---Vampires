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
			//Log.Message("0");
			if (pawn == null)
				return true;
			//Log.Message("0.5");

			var resultBool = true;

			if (pawn.IsVampire(true))
			{
				//Log.Message("1");

				if (pawn?.needs?.rest != null && pawn?.needs?.rest?.CurCategory >= RestCategory.VeryTired)
				{
					__result = false;
					resultBool = false;

					//Log.Message("2");
				}
				else
				{
					if (pawn?.needs?.TryGetNeed<Need_Blood>() is Need_Blood need && need.Starving)
					{
						__result = false;
						resultBool = false;

						//Log.Message("3");
					}
					else
					{
						if (!pawn.Awake())
						{
							__result = false;
							resultBool = false;

							//Log.Message("4");
						}
						else
						{
							if (pawn?.health?.hediffSet?.BleedRateTotal <= 0f)
							{
								if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
								{
									Pawn_TimetableTracker timetable = pawn.timetable;
									if (((timetable != null) ? timetable.CurrentAssignment : null) != TimeAssignmentDefOf.Meditate)
									{
										__result = false;
										resultBool = false;
										//Log.Message("5");
									}
								}
								if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
								{
									__result = true;
									resultBool = false;

									//Log.Message("6");
								}
							}
							__result = false;
							resultBool = false;

							//Log.Message("7");
						}
					}
				}
			}

			//Log.Message("8");
			return resultBool;
		}
	}
}
