using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace Vampire
{
    static partial class HarmonyPatches
    {
        public static void HarmonyPatches_Royalty(Harmony harmony)
        {
            //Vampires need to keep their titles if they are not truly dead.
            harmony.Patch(AccessTools.Method(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_KeepTitlesWhileDead)));

            //Vampires, once resurrected, should regain their titles.
            harmony.Patch(AccessTools.Method(typeof(Pawn_RoyaltyTracker), "Notify_Resurrected"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_RestoreTitlesWhenResurrected)));

            //Royal coffins should count as royal beds.
            harmony.Patch(AccessTools.Method(typeof(RoomRequirement_ThingAnyOf), "Met"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_RoyalCoffinsCountAsBeds)));

            //Royalty is permitted to drink blood.
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "InappropriateForTitle"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_RoyaltyConsumeBloodToo)));

            //Blood Wine is not somethings non-vampires go to drink
            harmony.Patch(AccessTools.Method(typeof(JoyGiver_SocialRelax), "TryFindIngestibleToNurse"), null, 
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampsCanDrinkBloodWineNotHumans)));

            //Allow the monument maker to have sarcophagus and coffins by fixing the sleeping spot error
            harmony.Patch(AccessTools.Property(typeof(MonumentMarker), nameof(MonumentMarker.FirstDisallowedBuilding)).GetGetMethod(), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampExceptionsForMonumentMarkers)));


        }
        public static void VampExceptionsForMonumentMarkers(MonumentMarker __instance, ref Thing __result)
        {
            if (__result?.def?.defName == "ROMV_SarcophagusBed")
            {
                //Log.ErrorOnce("VampExceptionTriggered", 828676);
                if (!__instance.Spawned)
                {
                    __result = null;
                    return;
                }
                List<SketchTerrain> terrain = __instance.sketch.Terrain;
                for (int i = 0; i < terrain.Count; i++)
                {
                    var tmpAllowedBuildings = (List<ThingDef>)AccessTools.Field(typeof(MonumentMarker), "tmpAllowedBuildings").GetValue(__instance);
                    tmpAllowedBuildings.Clear();
                    SketchThing sketchThing;
                    List<SketchThing> list;
                    __instance.sketch.ThingsAt(terrain[i].pos, out sketchThing, out list);

                    //Add sarcophagus sleep spot
                    tmpAllowedBuildings.Add(__result.def);

                    if (sketchThing != null)
                    {
                        tmpAllowedBuildings.Add(sketchThing.def);
                    }
                    if (list != null)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            tmpAllowedBuildings.Add(list[j].def);
                        }
                    }
                    List<Thing> thingList = (terrain[i].pos + __instance.Position).GetThingList(__instance.Map);
                    for (int k = 0; k < thingList.Count; k++)
                    {
                        if (thingList[k].def.IsBuildingArtificial && !thingList[k].def.IsBlueprint && !thingList[k].def.IsFrame && !tmpAllowedBuildings.Contains(thingList[k].def))
                        {
                            __result = thingList[k];
                            return;
                        }
                    }
                }
                __result = null;
                return;
            }
        }



        //JoyGiver_SocialRelax.TryFindIngestibleToNurse
        public static void VampsCanDrinkBloodWineNotHumans(JoyGiver_SocialRelax __instance, IntVec3 center, Pawn ingester, ref Thing ingestible, ref bool __result)
        {
            if (ingester.IsVampire(true) || ingestible?.def?.graphicData?.texPath != "Things/Item/Resource/BloodWine")
                return;

            Thing newIngestible = null;
            __result = VanillaModified_TryFindIngestibleToNurse(__instance, center, ingester, out newIngestible);
            ingestible = newIngestible;
        }

        private static bool VanillaModified_TryFindIngestibleToNurse(JoyGiver_SocialRelax __instance, IntVec3 center, Pawn ingester, out Thing ingestible)
        {
            if (ingester.IsTeetotaler())
            {
                ingestible = null;
                return false;
            }
            if (ingester.drugs == null)
            {
                ingestible = null;
                return false;
            }
            AccessTools.Field(typeof(JoyGiver_SocialRelax), "nurseableDrugs").SetValue(__instance, new List<ThingDef>());
            DrugPolicy currentPolicy = ingester.drugs.CurrentPolicy;
            for (int i = 0; i < currentPolicy.Count; i++)
            {
                if (currentPolicy[i].allowedForJoy && currentPolicy[i].drug.ingestible.nurseable)
                {
                    ((List<ThingDef>)AccessTools.Field(typeof(JoyGiver_SocialRelax), "nurseableDrugs").GetValue(__instance)).Add(currentPolicy[i].drug);
                }
            }
            ((List<ThingDef>)AccessTools.Field(typeof(JoyGiver_SocialRelax), "nurseableDrugs").GetValue(__instance)).Shuffle();
            for (int j = 0; j < ((List<ThingDef>)AccessTools.Field(typeof(JoyGiver_SocialRelax), "nurseableDrugs").GetValue(__instance)).Count; j++)
            {
                List<Thing> list = ingester.Map.listerThings.ThingsOfDef(((List<ThingDef>)AccessTools.Field(typeof(JoyGiver_SocialRelax), "nurseableDrugs").GetValue(__instance))[j]);
                if (list.Count > 0)
                {
                    Predicate<Thing> validator = delegate (Thing t)
                    {
                        if (t.def?.graphicData?.texPath == "Things/Item/Resource/BloodWine")
                        {
                            return false;
                        }
                        if (ingester.CanReserve(t))
                        {
                            return !t.IsForbidden(ingester);
                        }
                        return false;
                    };
                    ingestible = GenClosest.ClosestThing_Global_Reachable(center, ingester.Map, list, PathEndMode.OnCell, TraverseParms.For(ingester), 40f, validator);
                    if (ingestible != null)
                    {
                        return true;
                    }
                }
            }
            ingestible = null;
            return false;
        }

        // RimWorld.FoodUtility
        public static bool Vamp_RoyaltyConsumeBloodToo(ThingDef food, Pawn p, bool allowIfStarving, ref bool __result)
        {
            if (!p.IsVampire(true))
                return true;

            if (food.thingCategories.FirstOrDefault(x => x.defName == "ROMV_Blood") != null)
            {
                __result = false;
                return false;
            }
            return true;
        }

        //RoomRequirement_ThingAnyOf
        public static void Vamp_RoyalCoffinsCountAsBeds(RoomRequirement_ThingAnyOf __instance, Room r, ref bool __result)
        {
            foreach (ThingDef thing in __instance.things)
            {
                if (thing == ThingDefOf.RoyalBed)
                {
                    if (r.ContainsThing(ThingDef.Named("ROMV_RoyalCoffin")))
                    {
                        __result = true;
                        return;
                    }
                    if (r.ContainsThing(ThingDef.Named("ROMV_RoyalCoffinDouble")))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }

        //Pawn_RoyaltyTracker
        public static bool Vamp_RestoreTitlesWhenResurrected(Pawn_RoyaltyTracker __instance)
        {
            var pawn = (Pawn)AccessTools.Field(typeof(Pawn_RoyaltyTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire(true))
            {

                var dict = Find.World.GetComponent<WorldComponent_VampireTracker>().tempVampireTitles;
                if (!dict.ContainsKey(pawn))
                    return true;

                var titleList = dict[pawn];
                foreach (var item in titleList)
                {
                    pawn.royalty.SetTitle(item.faction, item.def, false, false, false);
                }
                dict.Remove(pawn);
                return false;
            }
            return true;
        }

        public static bool keepTitles = true;
        //Pawn_RoyaltyTracker
        public static bool Vamp_KeepTitlesWhileDead(Pawn_RoyaltyTracker __instance)
        {
            var pawn = (Pawn)AccessTools.Field(typeof(Pawn_RoyaltyTracker), "pawn").GetValue(__instance);
            if (pawn.IsVampire(true) && keepTitles)
            {
                //If no royalty exists, why bother?
                if (pawn?.royalty?.AllTitlesForReading?.FirstOrDefault() == null)
                    return true;

                var dict = Find.World.GetComponent<WorldComponent_VampireTracker>().tempVampireTitles;
                if (dict.ContainsKey(pawn))
                    dict.Remove(pawn);
                dict.Add(pawn, new List<RoyalTitle>(pawn.royalty.AllTitlesForReading));
                return false;
            }
            return true;
        }

    }
}
