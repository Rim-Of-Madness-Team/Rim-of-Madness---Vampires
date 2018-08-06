using RimWorld;
using System;
using System.Collections.Generic;
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
                    thing = BestBloodInInventory(getter, null, 0);
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
            Thing thing2 = BestBloodSourceOnMap(getter, eater, desperate, BloodPreferabilty.Highblood, allowForbidden);
            if (thing == null && thing2 == null)
            {
                if (canUseInventory && flag)
                {
                    thing = BestBloodInInventory(getter, null, 0);
                    if (thing != null)
                    {
                        bloodSource = thing;
                        bloodDef = thing.def;
                        return true;
                    }
                }
                if (getter == eater)
                {
                    Pawn pawn = BestPawnToHuntForVampire(getter);
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
                    if (t.IsBurning() || !desperate && t.IsNotFresh() || !getter.CanReserve(t))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            };
            Thing thing;
            Predicate<Thing> validator = foodValidator;
            thing = SpawnedBloodItemScan(eater, getter.Position, getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, validator);
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
            var allPawnsSpawned = predator.Map.mapPawns.AllPawnsSpawned;
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn pawn2 = allPawnsSpawned[i];
                if (predator == pawn2) continue;
                if (!IsAcceptableVictimFor(predator, pawn2, desperate)) continue;
                if (!predator.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly)) continue;
                if (tutorialMode && pawn2.Faction == Faction.OfPlayer) continue;
                float preyScoreFor = GetPreyScoreFor(predator, pawn2);
                if (!(preyScoreFor > num) && pawn != null) continue;
                num = preyScoreFor;
                pawn = pawn2;
                
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
            if (predator?.BloodNeed() is Need_Blood vampBlood &&
                prey?.BloodNeed() is Need_Blood victimBlood)
            {
                switch (vampBlood.preferredFeedMode)
                {
                    case PreferredFeedMode.HumanoidLethal:
                        if (prey.RaceProps.Humanlike) num3 += GetHumanHuntScore(predator, prey);
                        break;
                    case PreferredFeedMode.HumanoidNonLethal:
                        if (!victimBlood.DrainingIsDeadly && victimBlood.CurBloodPoints > 3 && prey.RaceProps.Humanlike)
                            num3 += GetHumanHuntScore(predator, prey);
                        break;
                    case PreferredFeedMode.AnimalLethal:
                        if (prey.RaceProps.Animal) num3 += GetAnimalHuntScore(predator, prey);
                            break;
                    case PreferredFeedMode.AnimalNonLethal:
                        if (!victimBlood.DrainingIsDeadly && victimBlood.CurBloodPoints > 1 && prey.RaceProps.Animal) num3 += GetAnimalHuntScore(predator, prey);
                        break;
                }
            }
            return num3;
        }

        public static float GetAnimalHuntScore(Pawn predator, Pawn prey)
        {
            if (prey?.Faction != null)
            {
                if (prey?.Faction == predator.Faction)
                {
                    if (prey?.playerSettings?.Master != null)
                        return -750f;
                    if (prey?.RaceProps?.petness >= 5)
                        return -500f;
                    return 100f;
                }
                else
                {
                    return -750f;
                }
            }
            return 200f;
        }

        public static float GetHumanHuntScore(Pawn predator, Pawn prey)
        {
            //In the predator state, they will attempt to bite everyone
            if (predator.MentalStateDef == HediffWithComps_BeastHunger.MentalState_VampireBeast)
            {
                if (prey.Faction.HostileTo(predator.Faction))
                    return 500f;
                return 200f;
            }
            if (prey.IsPrisoner || prey.IsPrisonerOfColony)
                return 200f;
            if (prey.Faction == predator.Faction)
                return 100f;
            return 10f;
        }


        public static bool IsAcceptableVictimFor(Pawn vampire, Pawn victim, bool desperate)
        {
            if (victim == null || vampire == null) return false;
            if (victim.Dead || vampire.Dead) return false;
            if (!victim.Spawned || !vampire.Spawned) return false;
            if (victim.RaceProps.IsMechanoid) return false;
            if (victim.def == ThingDef.Named("Boomalope")) return false;
            if (victim.def == ThingDef.Named("Boomrat")) return false;
            if (victim?.RaceProps?.FleshType == FleshTypeDefOf.Insectoid) return false;
            if (!victim.PositionHeld.CanArriveBeforeSunlight(vampire)) return false;
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

                    if (eaterBlood.preferredFeedMode == PreferredFeedMode.None)
                        return false;

                    if (victim.RaceProps.Animal)
                    {
                        //Prevents caravan animals from being eaten.
                        if (victim.Faction != null && victim.Faction != Faction.OfPlayerSilentFail)
                            return false;
                        if (eaterBlood.preferredFeedMode > PreferredFeedMode.AnimalLethal)
                            return false;
                        if (eaterBlood.preferredFeedMode == PreferredFeedMode.AnimalNonLethal)
                        {
                            if (targetBlood.CurBloodPoints == 1)
                            {
                                return false;
                            }
                        }
                        Pawn firstDirectRelationPawn = victim.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => !x.Dead);
                        if (firstDirectRelationPawn != null)
                        {
                            return false;
                        }
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
                        
                        if ((victim.IsPrisoner || victim.IsPrisonerOfColony) &&
                           eaterBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.PrisonersOnly)
                                return true;


                        if (!victim.IsPrisoner && !victim.IsPrisonerOfColony)
                        {
                            if (eaterBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.PrisonersOnly)
                                return false;

                            //Don't bite guests!
                            if (!desperate && victim.Faction != vampire.Faction && !victim.HostileTo(vampire))
                                return false;

                        }
                        //if (!desperate && (int)BloodTypeUtility.BloodType(victim) < (int)vampire.VampComp().Bloodline.minBloodPref)
                        //    return false;
                        //else if ((int)BloodTypeUtility.BloodType(victim) < (int)vampire.VampComp().Bloodline.desperateBloodPref)
                        //    return false;
                    }
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
            int num = Mathf.Min(10, StackCountForBlood(def, ingester.VampComp().BloodPool.BloodWanted));
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
