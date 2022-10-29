using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Beds(Harmony harmony)
    {
        // BEDS
        ///////////////////////////////////////////////////////////////////////////
        //Add overrides to methods if CompVampBed is active.
        //harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "Draw"),
        //    new HarmonyMethod(typeof(HarmonyPatches), nameof(Draw_VampBed)));
        //Log.Message("13");
        harmony.Patch(AccessTools.Method(typeof(Building_Casket), "Accepts"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Accepts_VampBed)));
        //Log.Message("14");
        //harmony.Patch(AccessTools.Method(typeof(Building_Grave), "get_Graphic"),
        //    new HarmonyMethod(typeof(HarmonyPatches), nameof(get_Graphic_VampBed)));
        //Log.Message("15");
        harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "GetFloatMenuOptions"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(GetFloatMenuOptions_VampBed)));
        //Log.Message("16");
        //Adds comfort to vampire beds.
        harmony.Patch(AccessTools.Method(typeof(PawnUtility), "GainComfortFromCellIfPossible"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_BedComfort)));
        //Log.Message("17");
        //Caskets and coffins do not auto-assign to colonists.
        harmony.Patch(AccessTools.Method(typeof(Pawn_Ownership), "ClaimBedIfNonMedical"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_BedsForTheUndead)));
        //Log.Message("18");
        harmony.Patch(AccessTools.Method(typeof(RestUtility), "IsValidBedFor"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IsValidBedFor)));
        //Log.Message("19");
    }

    public static bool Draw_VampBed(ThingWithComps __instance)
    {
        // if (__instance is not Building_Casket casket) return true;
        // if (__instance.GetComp<CompVampBed>() is not { } b) return true;
        // if (casket.Spawned && casket.GetDirectlyHeldThings()?.Count != 0) return false;
        // casket.Draw();
        // return false;
        if (__instance is not Building_Bed) return true;
        if (
            __instance.def.defName == "ROMV_SarcophagusBed" ||
            __instance.def.defName == "ROMV_SimpleCoffinBed" ||
            __instance.def.defName == "ROMV_RoyalCoffinBed" ||
            __instance.def.defName == "ROMV_RoyalCoffinDoubleBed"
        )
            return false;
        return true;
    }


    public static bool Accepts_VampBed(Building_Casket __instance, Thing thing, ref bool __result)
    {
        if (__instance.GetComps<CompVampBed>() is CompVampBed b)
        {
            if (!__instance.HasAnyContents && thing is Pawn p && p.IsVampire(true))
            {
                __result = true;
                return false;
            }

            __result = __instance.Accepts(thing);
            return false;
        }

        return true;
    }


    public static bool get_Graphic_VampBed(Building_Grave __instance, ref Graphic __result)
    {
        if (__instance.def.GetCompProperties<CompProperties_VampBed>() is { } b)
        {
            if (!__instance.HasAnyContents)
            {
                __result = __instance.DefaultGraphic;
                return false;
            }

            if (__instance.def.building.fullGraveGraphicData == null)
            {
                __result = __instance.DefaultGraphic;
                return false;
            }

            if (AccessTools.Field(typeof(Building_Grave), "cachedGraphicFull").GetValue(__instance) == null)
                AccessTools.Field(typeof(Building_Grave), "cachedGraphicFull").SetValue(__instance,
                    __instance.def.building.fullGraveGraphicData.GraphicColoredFor(__instance));

            __result = (Graphic)AccessTools.Field(typeof(Building_Grave), "cachedGraphicFull")
                .GetValue(__instance);
            return false;
        }

        return true;
    }


    public static void GetFloatMenuOptions_VampBed(ThingWithComps __instance, Pawn selPawn,
        ref IEnumerable<FloatMenuOption> __result)
    {
        if (__instance.GetComps<CompVampBed>() is CompVampBed b)
            if (selPawn?.IsVampire(true) ?? false)
                __result = __result.Concat(new[]
                {
                    new FloatMenuOption("ROMV_EnterTorpor".Translate(new object[]
                        {
                            selPawn.Label
                        }),
                        delegate { selPawn.jobs.TryTakeOrderedJob(new Job(VampDefOf.ROMV_EnterTorpor, __instance)); })
                });
    }

    // RimWorld.PawnUtility
    public static void Vamp_BedComfort(Pawn p)
    {
        if (Find.TickManager.TicksGame % 10 == 0)
        {
            var edifice = p.Position.GetEdifice(p.Map);
            if (edifice != null)
                if (edifice.TryGetComp<CompVampBed>() is { } vBed && vBed.Bed != null)
                {
                    var statValue = vBed.Bed.GetStatValue(StatDefOf.Comfort);
                    if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
                        p.needs.comfort.ComfortUsed(statValue);
                }
        }
    }


    public static bool Vamp_BedsForTheUndead(Pawn_Ownership __instance, Building_Bed newBed)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_Ownership), "pawn").GetValue(__instance);
        if (pawn != null && !pawn.IsVampire(true) && newBed.IsVampireBed()) return false;

        return true;
    }

    //RestUtility
    public static void Vamp_IsValidBedFor(Thing bedThing, Pawn sleeper, Pawn traveler,
        bool checkSocialProperness, bool allowMedBedEvenIfSetToNoCare, bool ignoreOtherReservations,
        GuestStatus? guestStatus, ref bool __result)
    {
        if (sleeper != null && !sleeper.IsVampire(true) && bedThing.IsVampireBed()) __result = false;
    }
}