using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static Vampire.VampireTracker;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Givers(Harmony harmony)
    {
        //Patches all JobGivers to consider sunlight for vampires before they do them.
        var listOfJobGivers = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies().Where(
                x => x.GetName().Name != "Harmony" &&
                     x.GetName().Name != "DraftingPatcher" &&
                     x.GetName().Name != "AnimalRangedVerbsUnlocker" &&
                     x.GetName().Name != "ExplosionTypes" &&
                     x.GetName().Name != "NewAnimalSubproducts" &&
                     x.GetName().Name != "NewHatcher" &&
                     x.GetName().Name != "SmurfeRims" &&
                     x.GetName().Name != "AnimalJobs" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "AnimalVehicles" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name !=
                     "AnimalWeaponFramework" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "Bugs")
            from assemblyType in domainAssembly.GetTypes()
            where typeof(ThinkNode_JobGiver).IsAssignableFrom(assemblyType)
            select assemblyType).ToArray();

        if (!listOfJobGivers.NullOrEmpty())
            foreach (var jobGiver in listOfJobGivers)
                try
                {
                    var tryGiveJob = AccessTools.Method(jobGiver, "TryGiveJob");
                    if (tryGiveJob?.DeclaringType == jobGiver)
                        harmony.Patch(AccessTools.Method(jobGiver, "TryGiveJob"), null,
                            new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveJob_VampireGeneral)));
                }
#pragma warning disable 168
                catch (Exception e)
#pragma warning restore 168
                {
                    /*////Log.Message(e.ToString());*/
                }

        //Patches all JoyGivers to consider sunlight for vampires before they do them.
        var listOfJoyGivers = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies().Where(
                x => x.GetName().Name != "Harmony" &&
                     x.GetName().Name != "DraftingPatcher" &&
                     x.GetName().Name != "AnimalRangedVerbsUnlocker" &&
                     x.GetName().Name != "ExplosionTypes" &&
                     x.GetName().Name != "NewAnimalSubproducts" &&
                     x.GetName().Name != "NewHatcher" &&
                     x.GetName().Name != "SmurfeRims" &&
                     x.GetName().Name != "AnimalJobs" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "AnimalVehicles" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name !=
                     "AnimalWeaponFramework" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "Bugs")
            from assemblyType in domainAssembly.GetTypes()
            where typeof(JoyGiver).IsAssignableFrom(assemblyType)
            select assemblyType).ToArray();

        if (!listOfJoyGivers.NullOrEmpty())
            foreach (var joyGiver in listOfJoyGivers)
                try
                {
                    var tryGiveJob = AccessTools.Method(joyGiver, "TryGiveJob");
                    if (tryGiveJob?.DeclaringType == joyGiver)
                        harmony.Patch(AccessTools.Method(joyGiver, "TryGiveJob"), null,
                            new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveJob_VampireGeneral)));
                }
#pragma warning disable 168
                catch (Exception e)
#pragma warning restore 168
                {
                    /*////Log.Message(e.ToString());*/
                }

        //Patches all WorkGivers to consider sunlight for vampires before they do them.
        var listOfWorkGivers = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies().Where(
                x => x.GetName().Name != "Harmony" &&
                     x.GetName().Name != "DraftingPatcher" &&
                     x.GetName().Name != "AnimalRangedVerbsUnlocker" &&
                     x.GetName().Name != "ExplosionTypes" &&
                     x.GetName().Name != "NewAnimalSubproducts" &&
                     x.GetName().Name != "NewHatcher" &&
                     x.GetName().Name != "SmurfeRims" &&
                     x.GetName().Name != "AnimalJobs" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "AnimalVehicles" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name !=
                     "AnimalWeaponFramework" && // added to stop errors with Advanced Animal Framework
                     x.GetName().Name != "Bugs")
            from assemblyType in domainAssembly.GetTypes()
            where typeof(WorkGiver).IsAssignableFrom(assemblyType)
            select assemblyType).ToArray();

        if (!listOfWorkGivers.NullOrEmpty())
            foreach (var workGiver in listOfWorkGivers)
                try
                {
                    var hasJobOnCellInfo = AccessTools.Method(workGiver, "HasJobOnCell");
                    if (hasJobOnCellInfo?.DeclaringType == workGiver)
                        harmony.Patch(AccessTools.Method(workGiver, "HasJobOnCell"), null,
                            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_HasJobOnCell)));
                }
#pragma warning disable 168
                catch (Exception e)
#pragma warning restore 168
                {
                    /*////Log.Message(e.ToString());*/
                }
    }


    public static void Vamp_HasJobOnCell(Pawn pawn, IntVec3 c, ref bool __result)
    {
        if (pawn == null) return;
        if (!pawn.IsVampire(true)) return;
        if (!VampireSettings.Get.aiToggle) return;

        if (GetSunlightPolicy(pawn) == SunlightPolicy.NoAI) return;
        if (!(pawn.MapHeld is { } m) || !c.IsValid || !c.InBounds(m)) return;
        if (pawn.Drafted || c.IsSunlightSafeFor(pawn)) return;

        __result = false;
    }


    public static void TryGiveJob_VampireGeneral(Pawn pawn, ref Job __result)
    {
        if (pawn == null) return;
        if (!pawn.IsVampire(true)) return;
        if (!VampireSettings.Get.aiToggle) return;

        if (__result?.def == JobDefOf.Ingest)
        {
            __result = null;
            return;
        }

        if (!pawn.Drafted && __result?.def != JobDefOf.Wait_Wander && __result?.def != JobDefOf.GotoWander &&
            __result?.playerForced == false && __result?.IsSunlightSafeFor(pawn) == false)
            __result = null;
    }
}