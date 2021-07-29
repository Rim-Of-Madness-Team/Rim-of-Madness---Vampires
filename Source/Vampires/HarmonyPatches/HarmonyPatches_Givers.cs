using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
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
                         x.GetName().Name != "Bugs")
                                   from assemblyType in domainAssembly.GetTypes()
                                   where typeof(ThinkNode_JobGiver).IsAssignableFrom(assemblyType)
                                   select assemblyType).ToArray();

            if (!listOfJobGivers.NullOrEmpty())
            {
                foreach (var jobGiver in listOfJobGivers)
                {
                    try
                    {
                        MethodInfo tryGiveJob = AccessTools.Method(jobGiver, "TryGiveJob");
                        if (tryGiveJob?.DeclaringType == jobGiver)
                            harmony.Patch(AccessTools.Method(jobGiver, "TryGiveJob"), null,
                                new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveJob_VampireGeneral)), null);
                    }
#pragma warning disable 168
                    catch (Exception e)
#pragma warning restore 168
                    {
                        /*////Log.Message(e.ToString());*/
                    }
                }
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
                         x.GetName().Name != "Bugs")
                                   from assemblyType in domainAssembly.GetTypes()
                                   where typeof(JoyGiver).IsAssignableFrom(assemblyType)
                                   select assemblyType).ToArray();

            if (!listOfJoyGivers.NullOrEmpty())
            {
                foreach (var joyGiver in listOfJoyGivers)
                {
                    try
                    {
                        MethodInfo tryGiveJob = AccessTools.Method(joyGiver, "TryGiveJob");
                        if (tryGiveJob?.DeclaringType == joyGiver)
                            harmony.Patch(AccessTools.Method(joyGiver, "TryGiveJob"), null,
                                new HarmonyMethod(typeof(HarmonyPatches), nameof(TryGiveJob_VampireGeneral)), null);
                    }
#pragma warning disable 168
                    catch (Exception e)
#pragma warning restore 168
                    {
                        /*////Log.Message(e.ToString());*/
                    }
                }
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
                         x.GetName().Name != "Bugs")
                                    from assemblyType in domainAssembly.GetTypes()
                                    where typeof(WorkGiver).IsAssignableFrom(assemblyType)
                                    select assemblyType).ToArray();

            if (!listOfWorkGivers.NullOrEmpty())
            {
                foreach (var workGiver in listOfWorkGivers)
                {
                    try
                    {
                        MethodInfo hasJobOnCellInfo = AccessTools.Method(workGiver, "HasJobOnCell");
                        if (hasJobOnCellInfo?.DeclaringType == workGiver)
                            harmony.Patch(AccessTools.Method(workGiver, "HasJobOnCell"), null,
                                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_HasJobOnCell)), null);
                    }
#pragma warning disable 168
                    catch (Exception e)
#pragma warning restore 168
                    {
                        /*////Log.Message(e.ToString());*/
                    }
                }
            }
        }


        public static void Vamp_HasJobOnCell(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (!pawn.IsVampire()) return;
            if (pawn.VampComp().CurrentSunlightPolicy == SunlightPolicy.NoAI) return;
            if (!(pawn.MapHeld is Map m) || !c.IsValid || !c.InBounds(m)) return;
            if (pawn.Drafted || c.IsSunlightSafeFor(pawn)) return;
            __result = false;
        }




        public static void TryGiveJob_VampireGeneral(Pawn pawn, ref Job __result)
        {
            if (__result != null && pawn.IsVampire())
            {
                if (__result.def == JobDefOf.Ingest)
                {
                    __result = null;
                    return;
                }

                if (!pawn.Drafted && __result.def != JobDefOf.Wait_Wander && __result.def != JobDefOf.GotoWander &&
                    !__result.playerForced && !__result.IsSunlightSafeFor(pawn))
                    __result = null;
            }
        }



    }
}
