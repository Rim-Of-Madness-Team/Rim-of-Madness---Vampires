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
        public static void HarmonyPatches_Age(Harmony harmony)
        {

            // AGING
            ////////////////////////////////////////////////////////////////////////////
            //Vampires and Ghouls do not age like others.
            harmony.Patch(AccessTools.Method(typeof(Pawn_AgeTracker), "BirthdayBiological"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampireBirthdayBiological)), null);
            //Log.Message("35");

            //Nor do they suffer health effects as they age.
            harmony.Patch(
                AccessTools.Method(AccessTools.TypeByName("AgeInjuryUtility"), "GenerateRandomOldAgeInjuries"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GenerateRandomOldAgeInjuries)), null);
            //Log.Message("36");


            harmony.Patch(AccessTools.Method(typeof(HediffGiver_RandomAgeCurved),
                    nameof(HediffGiver_RandomAgeCurved.OnIntervalPassed)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampsDontHaveHeartAttacks)), null);
            //Log.Message("88");

        }


        // Verse.Pawn_AgeTracker
        public static bool VampireBirthdayBiological(Pawn_AgeTracker __instance)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (p.RaceProps.Humanlike && (p.IsVampire(true) || p.IsGhoul()) && PawnUtility.ShouldSendNotificationAbout(p))
            {
                Find.LetterStack.ReceiveLetter("LetterLabelBirthday".Translate(), "ROMV_VampireBirthday".Translate(
                    new object[]
                    {
                        p.Label,
                        p.ageTracker.AgeBiologicalYears
                    }), LetterDefOf.PositiveEvent, p);
                return false;
            }

            return true;
        }


        // RimWorld.AgeInjuryUtility
        public static bool Vamp_GenerateRandomOldAgeInjuries(Pawn pawn, bool tryNotToKillPawn)
        {
            if (VampireGenInProgress || pawn.IsVampire(true) || pawn.IsGhoul())
            {
                return false;
            }

            return true;
        }



        //HediffGiver_RandomAgeCurved
        public static bool VampsDontHaveHeartAttacks(Pawn pawn, Hediff cause)
        {
            if (pawn.IsVampire(true))
                return false;
            return true;
        }

    }
}
