using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    static partial class HarmonyPatches
    {
        // RimWorld.Dialog_FormCaravan
        public static void CheckForErrors_Vampires(List<Pawn> pawns, ref bool __result)
        {
            if (pawns.Any(x => x.IsVampire() && pawns.Any(y => !y.IsVampire())))
            {
                Messages.Message("ROMV_Caravan_WarningMixedWithVampires".Translate(), MessageTypeDefOf.RejectInput);
                __result = false;
                return;
            }
        }

        //Caravan
        public static void get_Resting_Vampires(Caravan __instance, ref bool __result)
        {
            if (__instance.PawnsListForReading.Any(x => x.IsVampire()))
            {
                float num = GenDate.HourFloat((long)GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(__instance.Tile).x);
                __result = num >= 6f && num <= 17f;
            }
        }
    }
}
