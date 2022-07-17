using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class VampireTracker
    {
        public enum SunlightPolicy : int
        {
            Relaxed = 0,
            Restricted = 1,
            NoAI = 2
        }

        public static WorldComponent_VampireTracker Get => Find.World.GetComponent<WorldComponent_VampireTracker>();

        public static SunlightPolicy GetSunlightPolicy(Pawn pawn)
        { 
            if (Get?.sunlightPolicies.ContainsKey(pawn) == true)
            {
                return Get.sunlightPolicies[pawn];
            }
            return SunlightPolicy.NoAI;
        }

        public static void SetSunlightPolicy(Pawn pawn, SunlightPolicy sunlightPolicy)
        {
            if (Get?.sunlightPolicies?.ContainsKey(pawn) == true)
            {
                Get.sunlightPolicies[pawn] = sunlightPolicy;
            }
            else
            {
                Get.sunlightPolicies.Add(pawn, sunlightPolicy);
            }
        }



        public static bool IsVampire(Pawn pawn)
        {
            try
            {
                if (!Get.vampiresLoaded)
                {
                    return pawn.IsVampire(false);
                }

                if (Get?.vampireList?.Contains(pawn) == true)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void AddVampire(Pawn pawn)
        {
            Get.vampireList.Add(pawn);
            //Log.Message("Added " + pawn.Label);
        }
        public static void RemoveVampire(Pawn pawn)
        {
            if (Get?.vampireList?.Contains(pawn) == true)
                Get.vampireList.Remove(pawn);
        }
    }
}