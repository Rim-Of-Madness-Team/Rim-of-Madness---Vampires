using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_AI(Harmony harmony)
    {
        #region AI Error Handling

        // AI ERROR HANDLING
        ///////////////////////////////////////////////////////////////////////////////////            
        //Prevents blood items from spawning in people's inventories as food -- I mean -- ew
        harmony.Patch(AccessTools.Method(typeof(Pawn_InventoryTracker), "TryAddItemNotForSale"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_BloodItemsDontSpawnForNormies)));
        //Log.Message("37");
        //Lord_AI patches
        harmony.Patch(AccessTools.Method(typeof(Trigger_UrgentlyHungry), "ActivateOn"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(ActivateOn_Vampire)));
        //Log.Message("38");
        //Patches so that wardens do not try to feed vampires
        harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "get_CanBeBroughtFood"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_WardensDontFeedVamps)));
        //Log.Message("39");
        //Guests were also checking for "food" related items.
        harmony.Patch(AccessTools.Method(typeof(GatheringsUtility), "ShouldGuestKeepAttendingGathering"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_GuestFix)));
        //Log.Message("40");
        //Removes more guest food checks
        harmony.Patch(AccessTools.Method(typeof(JobGiver_EatInGatheringArea), "TryGiveJob"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontEatAtTheParty)));
        //Log.Message("41");
        //Blood Mists should not attack, but drain their target.
        harmony.Patch(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(BloodMist_NoAttack)));
        //Log.Message("42");
        //Removes food check.
        harmony.Patch(AccessTools.Method(typeof(SickPawnVisitUtility), "CanVisit"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_CanVisit)));
        //Log.Message("43");
        //Patches out binging behavior
        harmony.Patch(AccessTools.Method(typeof(JobGiver_Binge), "TryGiveJob"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DontBinge)));
        //Log.Message("44");
        //Vampires should never skygaze during sunrise...
        harmony.Patch(AccessTools.Method(typeof(JobDriver_Skygaze), "GetReport"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_QuitWatchingSunrisesAlreadyJeez)));
        //Log.Message("45");
        //Log.Message("46");
        //Log.Message("47");

        #endregion
    }


    // Verse.Pawn_InventoryTracker
    private static bool Vamp_BloodItemsDontSpawnForNormies(Pawn_InventoryTracker __instance, Thing item)
    {
        if (__instance?.pawn?.IsVampire(true) == false)
            if (item?.def?.thingCategories?.Contains(VampDefOfTwo.ROMV_Blood) ?? false)
                return false;

        return true;
    }


    // Verse.AI.Group.Trigger_UrgentlyHungry
    public static bool ActivateOn_Vampire(Lord lord, TriggerSignal signal, ref bool __result)
    {
        if (lord?.ownedPawns == null || lord?.ownedPawns?.Count == 0) return true;
        if (!(lord?.ownedPawns?.Any(x => x.IsVampire(true) || x.Dead || x.RaceProps.IsMechanoid) ?? false)) return true;
        if (signal.type == TriggerSignalType.Tick)
            for (var i = 0; i < lord?.ownedPawns?.Count; i++)
                if (lord?.ownedPawns[i]?.needs?.food?.CurCategory >= HungerCategory.UrgentlyHungry)
                {
                    __result = true;
                    return false;
                }

        __result = false;
        return false;
    }


    // RimWorld.Pawn_GuestTracker
    public static void Vamp_WardensDontFeedVamps(Pawn_GuestTracker __instance, ref bool __result)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_GuestTracker), "pawn").GetValue(__instance);
        if (pawn.IsVampire(true)) __result = false;
    }


    // RimWorld.GatheringsUtility
    public static bool Vamp_GuestFix(Pawn p, ref bool __result)
    {
        if (p.IsVampire(true))
        {
            __result = !p.Downed && p?.health?.hediffSet?.BleedRateTotal <= 0f &&
                       p?.needs?.rest?.CurCategory < RestCategory.Exhausted &&
                       (!p?.health?.hediffSet?.HasTendableNonInjuryNonMissingPartHediff() ?? true) && p.Awake() &&
                       !p.InAggroMentalState && !p.IsPrisoner;
            return false;
        }

        return true;
    }


    //public class JobGiver_EatInPartyArea : ThinkNode_JobGiver
    //{
    public static bool Vamp_DontEatAtTheParty(Pawn pawn, ref Job __result)
    {
        if (pawn.IsVampire(true))
        {
            __result = null;
            return false;
        }

        return true;
    }

    //Rimworld.JobGiver_AIFightEnemy
    public static void BloodMist_NoAttack(Pawn pawn, ref Job __result)
    {
        if (pawn is PawnTemporary && pawn.def == VampDefOf.ROMV_BloodMistRace && __result != null &&
            __result.def == JobDefOf.AttackMelee)
            //Cancel any melee attacks
            __result = null;
    }

    // RimWorld.SickPawnVisitUtility
    public static bool Vamp_CanVisit(ref bool __result, Pawn pawn, Pawn sick, JoyCategory maxPatientJoy)
    {
        if (sick.IsVampire(true))
        {
            __result = sick.IsColonist && !sick.Dead && pawn != sick && sick.InBed() && sick.Awake() &&
                       !sick.IsForbidden(pawn) && sick.needs.joy != null &&
                       sick.needs.joy.CurCategory <= maxPatientJoy &&
                       InteractionUtility.CanReceiveInteraction(sick) &&
                       pawn.CanReserveAndReach(sick, PathEndMode.InteractionCell, Danger.None) &&
                       !AboutToRecover(sick);
            return false;
        }

        return true;
    }

    // RimWorld.JobGiver_Binge
    public static void Vamp_DontBinge(Pawn pawn, ref Job __result)
    {
        if (pawn.IsVampire(true)) __result = null;
    }


    // RimWorld.JobDriver_Skygaze
    public static void Vamp_QuitWatchingSunrisesAlreadyJeez(JobDriver_Skygaze __instance, ref string __result)
    {
        if (__instance.pawn is { } p && p.IsVampire(true))
            if (GenLocalDate.DayPercent(p) < 0.5f)
                __instance.EndJobWith(JobCondition.InterruptForced);
    }


    // RimWorld.SickPawnVisitUtility
    private static bool AboutToRecover(Pawn pawn)
    {
        if (pawn.Downed) return false;

        if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) &&
            !HealthAIUtility.ShouldSeekMedicalRest(pawn)) return true;

        if (pawn.health.hediffSet.HasImmunizableNotImmuneHediff()) return false;

        var num = 0f;
        var hediffs = pawn.health.hediffSet.hediffs;
        for (var i = 0; i < hediffs.Count; i++)
        {
            var hediff_Injury = hediffs[i] as Hediff_Injury;
            if (hediff_Injury != null && (hediff_Injury.CanHealFromTending() || hediff_Injury.CanHealNaturally() ||
                                          hediff_Injury.Bleeding))
                num += hediff_Injury.Severity;
        }

        return num < 8f * pawn.RaceProps.baseHealthScale;
    }


    // Verse.AI.Pawn_JobTracker
    public static void Vamp_EndCurrentJob(Pawn_JobTracker __instance, JobCondition condition, bool startNewJob)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_JobTracker), "pawn").GetValue(__instance);
        if (pawn.IsVampire(true))
            if (__instance.curJob != null && __instance.curDriver.pawn.GetPosture() != PawnPosture.Standing &&
                !pawn.Downed &&
                __instance.curJob.def != JobDefOf.Lovin)
            {
                if (pawn.BloodNeed() is { } bN)
                    bN.AdjustBlood(-1);
                else
                    Log.Warning("Vampires :: Failed to show blood need.");
            }
    }
}