using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public static class BloodUtility
    {

        // RimWorld.FoodUtility
        public static bool TryFindBestBloodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing bloodSource, out ThingDef bloodDef, bool canUseInventory = true, bool allowForbidden = false)
        {
            bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            Thing thing = null;
            if (canUseInventory)
            {
                if (flag)
                    thing = BloodUtility.BestBloodInInventory(getter, null, 0);
                if (thing != null)
                {
                    if (getter.Faction != Faction.OfPlayer)
                    {
                        bloodSource = thing;
                        bloodDef = thing.def;
                        return true;
                    }
                    CompRottable compRottable = thing.TryGetComp<CompRottable>();
                    if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        bloodSource = thing;
                        bloodDef = thing.def;
                        return true;
                    }
                }
            }
            Thing thing2 = BloodUtility.BestBloodSourceOnMap(getter, eater, desperate, BloodPreferabilty.Highblood, allowForbidden);
            if (thing == null && thing2 == null)
            {
                if (canUseInventory && flag)
                {
                    thing = BloodUtility.BestBloodInInventory(getter, null, 0);
                    if (thing != null)
                    {
                        bloodSource = thing;
                        bloodDef = thing.def;
                        return true;
                    }
                }
                if (thing2 == null && getter == eater)
                {

                    Pawn pawn = BloodUtility.BestPawnToHuntForVampire(getter);
                    if (pawn != null)
                    {
                        bloodSource = pawn;
                        bloodDef = pawn.def;
                        return true;
                    }
                }
                bloodSource = null;
                bloodDef = null;
                return false;
            }
            if (thing == null && thing2 != null)
            {
                bloodSource = thing2;
                bloodDef = thing2.def;
                return true;
            }
            if (thing2 == null && thing != null)
            {
                bloodSource = thing;
                bloodDef = thing.def;
                return true;
            }
            bloodSource = thing;
            bloodDef = thing.def;
            return false;
        }

        // RimWorld.FoodUtility
        public static Thing BestBloodInInventory(Pawn holder, Pawn eater = null, int minBloodPointsRequested = 1)
        {
            if (holder.inventory == null)
            {
                return null;
            }
            if (eater == null)
            {
                eater = holder;
            }
            ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Thing thing = innerContainer[i];
                if (thing.TryGetComp<CompBloodItem>() is CompBloodItem c)
                {
                    if (minBloodPointsRequested >= c.Props.bloodPoints)
                    {
                        return thing;
                    }
                }
            }
            return null;
        }

        // RimWorld.FoodUtility
        public static Thing BestBloodSourceOnMap(Pawn getter, Pawn eater, bool desperate, BloodPreferabilty maxPref = BloodPreferabilty.Highblood, bool allowForbidden = false)
        {
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Error(string.Concat(new object[]
                {
            getter,
            " tried to find blood to bring to ",
            eater,
            " but ",
            getter,
            " is incapable of Manipulation."
                }));
                return null;
            }
            
            BloodPreferabilty minPref = eater?.VampComp()?.Bloodline?.minBloodPref ?? BloodPreferabilty.Any;
            if (desperate)
            {
                minPref = eater?.VampComp()?.Bloodline?.desperateBloodPref ?? BloodPreferabilty.Any;
            }
            Predicate<Thing> foodValidator = delegate (Thing t)
            {

                if (t.TryGetComp<CompBloodItem>() is CompBloodItem bl)
                {
                    if (!allowForbidden && t.IsForbidden(getter))
                    {
                        return false;
                    }
                    if ((int)bl.Props.bloodType < (int)minPref)
                    {
                        return false;
                    }
                    if ((int)bl.Props.bloodType > (int)maxPref)
                    {
                        return false;
                    }
                    if (t.IsBurning() || (!desperate && t.IsNotFresh()) || !getter.CanReserve(t, 1, -1, null, false))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            };
            Thing thing;
            Predicate<Thing> validator = foodValidator;
            thing = BloodUtility.SpawnedBloodItemScan(eater, getter.Position, getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
            return thing;
        }

        // RimWorld.FoodUtility
        private static Thing SpawnedBloodItemScan(Pawn eater, IntVec3 root, List<Thing> searchSet,
                                        PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f,
                                        Predicate<Thing> validator = null)
        {
            if (searchSet == null)
            {
                return null;
            }
            Pawn pawn = traverseParams.pawn ?? eater;
            int num = 0;
            int num2 = 0;
            Thing result = null;
            //float num3 = -3.40282347E+38f;
            for (int i = 0; i < searchSet.Count; i++)
            {
                Thing thing = searchSet[i];
                num2++;
                float num4 = (float)(root - thing.Position).LengthManhattan;
                if (num4 <= maxDistance)
                {
                    if (pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams))
                    {
                        if (thing.Spawned)
                        {
                            if (validator == null || validator(thing))
                            {
                                result = thing;
                                num++;
                            }
                        }
                    }
                }
            }
            return result;
        }

        // RimWorld.FoodUtility
        private static Pawn BestPawnToHuntForVampire(Pawn predator, bool desperate = false)
        {
            //if (predator.meleeVerbs.TryGetMeleeVerb() == null)
            //{
            //    return null;
            //}
            //bool flag = false;
            //float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
            //if (summaryHealthPercent < 0.25f)
            //{
            //    flag = true;
            //}
            List<Pawn> allPawnsSpawned = predator.Map.mapPawns.AllPawnsSpawned;
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn pawn2 = allPawnsSpawned[i];
                if (predator.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
                {
                    if (predator != pawn2)
                    {
                        if (BloodUtility.IsAcceptableVictimFor(predator, pawn2, desperate))
                        {
                            if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                            {
                                //if (!pawn2.IsForbidden(predator))
                                //{
                                    if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
                                    {
                                    //Log.Message("Potential Prey: " + pawn2.Label);
                                        float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2) + ((pawn2.RaceProps.Humanlike) ? 200 : 0);
                                    //Log.Message("Potential Prey Score: " + preyScoreFor);

                                    if (preyScoreFor > num || pawn == null)
                                        {
                                            num = preyScoreFor;
                                            pawn = pawn2;
                                        }
                                    }
                                //}
                            }
                        }
                    }
                }
            }
            return pawn;
        }

        // RimWorld.FoodUtility
        public static float GetPreyScoreFor(Pawn predator, Pawn prey)
        {
            float num = prey.kindDef.combatPower / predator.kindDef.combatPower;
            float num2 = prey.health.summaryHealth.SummaryHealthPercent;
            float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor;
            float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
            if (prey.Downed)
            {
                num2 = Mathf.Min(num2, 0.2f);
            }
            float num3 = -lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
            if (prey.RaceProps.Humanlike)
            {
                num3 -= 35f;
            }
            return num3;
        }


        public static bool IsAcceptableVictimFor(Pawn vampire, Pawn victim, bool desperate)
        {
            if (victim == null || vampire == null) return false;
            if (victim.Dead || vampire.Dead) return false;
            if (!victim.Spawned || !vampire.Spawned) return false;
            if (victim?.BloodNeed() is Need_Blood targetBlood)
            {
                if (vampire?.BloodNeed() is Need_Blood eaterBlood)
                {
                    if (VampireUtility.IsDaylight(victim) && !victim.PositionHeld.Roofed(victim.Map))
                        return false;

                    if (victim.IsVampire())
                        return false;

                    if (!vampire.CanReserve(victim))
                        return false;

                    if (vampire.MentalStateDef == HediffWithComps_BeastHunger.MentalState_VampireBeast)
                        return true;

                    if (victim.RaceProps.Animal)
                    {
                        if (eaterBlood.preferredFeedMode > PreferredFeedMode.AnimalLethal)
                            return false;
                        if (eaterBlood.preferredFeedMode == PreferredFeedMode.AnimalNonLethal &&
                            targetBlood.CurBloodPoints == 1)
                            return false;
                    }

                    if (victim.RaceProps.Humanlike)
                    {
                        if (eaterBlood.preferredFeedMode < PreferredFeedMode.HumanoidNonLethal)
                            return false;
                        if (eaterBlood.preferredFeedMode == PreferredFeedMode.HumanoidNonLethal &&
                            targetBlood.CurBloodPoints <= 2)
                        {
                            return false;
                        }
                    }
                    
                    if (!desperate && (int)BloodTypeUtility.BloodType(victim) < (int)vampire.VampComp().Bloodline.minBloodPref)
                        return false;
                    else if ((int)BloodTypeUtility.BloodType(victim) < (int)vampire.VampComp().Bloodline.desperateBloodPref)
                        return false;
                    return true;
                }
            }
            return false;
        }

        public static bool WillConsume(BloodType bl, BloodPreferabilty pref)
        {
            if ((int)bl < (int)pref) return false;
            return true;
        }

        public static Need_Blood BloodNeed(this Pawn pawn)
        {
            return pawn?.needs?.TryGetNeed<Need_Blood>() ?? null;
        }


        // RimWorld.FoodUtility
        public static int WillConsumeStackCountOf(Pawn ingester, ThingDef def)
        {
            int num = Mathf.Min(10, BloodUtility.StackCountForBlood(def, ingester.VampComp().BloodPool.BloodWanted));
            if (num < 1)
            {
                num = 1;
            }
            return num;
        }

        // RimWorld.FoodUtility
        public static int StackCountForBlood(ThingDef def, float nutrition)
        {
            if (def.GetCompProperties<CompProperties_BloodItem>() is CompProperties_BloodItem bloodItem)
            {
                return Mathf.Max(Mathf.RoundToInt(nutrition / def.GetCompProperties<CompProperties_BloodItem>().bloodPoints), 1);
            }
            return 0;
        }


    }
}
