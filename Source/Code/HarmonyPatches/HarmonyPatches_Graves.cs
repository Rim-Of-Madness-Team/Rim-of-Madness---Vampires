using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Graves(Harmony harmony)
    {
        // GRAVES / RESURRECTION / CORPSES / SLEEPING BEHAVIOR
        //////////////////////////////////////////////////////////////////////////////////            
        //Vampire corpses can resurrect safely inside graves, sarcophogi, and caskets.
        harmony.Patch(AccessTools.Method(typeof(Building_Grave), "GetGizmos"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TheyNeverDie)));
        //Log.Message("65");
        //            //Sets max assignments to be from the size of the coffin.
        //            harmony.Patch(AccessTools.Method(typeof(Building_Grave), "get_MaxAssignedPawnsCount"), null,
        //                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CouplesLikeBiggerCaskets)));
        //            //Allows coffins to assign multiple characters
        //harmony.Patch(AccessTools.Method(typeof(Building_Grave), "TryAssignPawn"),
        //new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_AssignToCoffin)), null);
        //Patches corpse generation for vampires.
        harmony.Patch(AccessTools.Method(typeof(Pawn), "MakeCorpse",
                new[]
                {
                    typeof(Building_Grave), typeof(bool), typeof(float)
                }),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_MakeCorpse)));
        //Log.Message("66");
        //Makes vampires use one blood point to be forced awake from slumber.
        //            harmony.Patch(AccessTools.Method(typeof(Pawn_JobTracker), "EndCurrentJob"),
        //                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_EndCurrentJob)), null);
        //Vampires should tire very much during the daylight hours.
        harmony.Patch(AccessTools.Method(typeof(Need_Rest), "NeedInterval"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_SleepyDuringDaylight)));
        //Log.Message("67");
        //Vampires should not have memories like SleptInCold and SleptInHeat
        harmony.Patch(AccessTools.Method(typeof(Toils_LayDown), "ApplyBedThoughts"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_ApplyBedThoughts)));
        //Log.Message("68");
    }


    public static IEnumerable<Gizmo> GraveGizmoGetter(Pawn AbilityUser, Building_Grave grave)
    {
        var dFlag = false;
        var dReason = "";
        if ((AbilityUser?.BloodNeed()?.CurBloodPoints ?? 0) <= 0)
        {
            dFlag = true;
            dReason = "ROMV_NoBloodRemaining".Translate();
        }

        var bloodAwaken = DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricAwaken");
        if (!AbilityUser?.Dead ?? false)
            yield return new Command_Action
            {
                defaultLabel = bloodAwaken.label,
                defaultDesc = bloodAwaken.GetDescription(),
                icon = bloodAwaken.uiIcon,
                action = delegate
                {
                    AbilityUser.BloodNeed().AdjustBlood(-1);
                    grave.EjectContents();
                    if (grave.def == VampDefOf.ROMV_HideyHole)
                        grave.Destroy();
                },
                disabled = dFlag,
                disabledReason = dReason
            };

        var bloodResurrection =
            DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricResurrection");
        if (AbilityUser?.Corpse?.GetRotStage() < RotStage.Dessicated)
            yield return new Command_Action
            {
                defaultLabel = bloodResurrection.label,
                defaultDesc = bloodResurrection.GetDescription(),
                icon = bloodResurrection.uiIcon,
                action = delegate
                {
                    AbilityUser.Drawer.Notify_DebugAffected();
                    ResurrectionUtility.Resurrect(AbilityUser);
                    MoteMaker.ThrowText(AbilityUser.PositionHeld.ToVector3(), AbilityUser.MapHeld,
                        StringsToTranslate.AU_CastSuccess);
                    AbilityUser.BloodNeed().AdjustBlood(-99999999);
                    HealthUtility.AdjustSeverity(AbilityUser, VampDefOf.ROMV_TheBeast, 1.0f);
                    var MentalState_VampireBeast =
                        DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
                    AbilityUser.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null,
                        true);
                    Find.LetterStack.ReceiveLetter("ROMV_TheBeast".Translate(),
                        "ROMV_TheBeastDesc".Translate(AbilityUser.Label), VampDefOf.ROMV_FrenzyMessage, AbilityUser);
                },
                disabled = (AbilityUser?.BloodNeed()?.CurBloodPoints ?? 0) < 0
            };
    }

    //Building_Grave
    public static void Vamp_TheyNeverDie(Building_Grave __instance, ref IEnumerable<Gizmo> __result)
    {
        if (__instance?.Corpse is { } c && c.InnerPawn is { } p)
            if (p.Faction == Faction.OfPlayer && p.IsVampire(true))
                __result = __result.Concat(GraveGizmoGetter(p, __instance));

        if (__instance?.ContainedThing is Pawn q)
            if (q.Faction == Faction.OfPlayer && q.IsVampire(true))
                __result = __result.Concat(GraveGizmoGetter(q, __instance));
    }


    // Verse.Pawn
    public static bool Vamp_MakeCorpse(Pawn __instance, Building_Grave assignedGrave, bool inBed, float bedRotation,
        ref Corpse __result)
    {
        if (__instance.IsVampire(true))
        {
            if (__instance.holdingOwner != null)
            {
                Log.Warning(
                    "We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" +
                    __instance.ParentHolder);
                __result = null;
                return false;
            }

            var corpse = (VampireCorpse)ThingMaker.MakeThing(ThingDef.Named("ROMV_VampCorpse"));
            corpse.InnerPawn = __instance;
            corpse.BloodPoints = __instance.BloodNeed().CurBloodPoints;
            if (VampireUtility.GetAllInjuries(__instance)
                    ?.Where(x => x.def == HediffDefOf.Burn && !x.IsTended())?.Count() > 3)
                corpse.BurnedToAshes = true;

            if (assignedGrave != null) corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);

            if (inBed) corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);

            __result = corpse;
            return false;
        }

        return true;
    }

    // RimWorld.Need_Rest
    public static void Vamp_SleepyDuringDaylight(Need_Rest __instance)
    {
        if (VampireSettings.Get.slumberToggle)
        {
            var pawn = (Pawn)AccessTools.Field(typeof(Need_Rest), "pawn").GetValue(__instance);
            if (pawn != null && pawn.IsVampire(true))
            {
                if (VampireUtility.IsDaylight(pawn))
                    __instance.CurLevel = Mathf.Min(0.1f, __instance.CurLevel);
                else
                    __instance.CurLevel = 1.0f;
            }
        }
    }


    // RimWorld.Toils_LayDown
    public static void Vamp_ApplyBedThoughts(Pawn actor)
    {
        if (actor.needs.mood == null) return;

        if (actor.IsVampire(true))
        {
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
        }
    }
}