using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Thoughts(Harmony harmony)
        {

            // THOUGHTS & FEELINGS
            ////////////////////////////////////////////////////////////////////////////////////
            //Vampires should not dislike the darkness.
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Dark), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TheyDontDislikeDarkness)));
            //Log.Message("56");
            //Vampires should not get cabin fever.
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_NeedOutdoors), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoCabinFever)));
            //Log.Message("57");
            //Vampires do not worry about hot and cold
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Hot), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreHotAndCold)));
            //Log.Message("58");
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Cold), "CurrentStateInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreHotAndCold)));
            //Log.Message("59");


        }



        //ThoughtWorker_Dark
        public static void Vamp_TheyDontDislikeDarkness(Pawn p, ref ThoughtState __result)
        {
            bool temp = __result.Active;
            __result = temp && !p.IsVampire();
        }



        // RimWorld.ThoughtWorker_CabinFever
        public static void Vamp_NoCabinFever(Pawn p, ref ThoughtState __result)
        {
            if (p.IsVampire())
                __result = ThoughtState.Inactive;
        }



        //ThoughtWorker_Hot
        public static void Vamp_IgnoreHotAndCold(Pawn p, ref ThoughtState __result)
        {
            if (p != null && p.IsVampire())
            {
                __result = ThoughtState.Inactive;
            }
        }



    }
}
