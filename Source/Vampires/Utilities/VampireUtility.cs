using AbilityUser;
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
    public static class VampireUtility
    {
        public static CompVampire VampComp(this Pawn pawn)
        {
            return pawn.GetComp<CompVampire>();
        }

        public static readonly Color VampColor = new Color(0.6f, 0.5f, 0.9f);

        public static int RandHigherGeneration => Rand.Range(7, 13);
        public static int RandLowerGeneration => Rand.Range(3, 6);
        public static Faction RandVampFaction => Find.FactionManager.AllFactions
            .Where(x => x.def == VampDefOf.ROMV_Camarilla ||
                        x.def == VampDefOf.ROMV_Anarch ||
                        x.def == VampDefOf.ROMV_Sabbat)
            .RandomElement();
        public static BloodlineDef RandBloodline => DefDatabase<BloodlineDef>.AllDefs
            .Where(x => x != VampDefOf.ROMV_Caine && x != VampDefOf.ROMV_TheThree).RandomElement();
        public static bool IsDaylight(Pawn p) => (GenLocalDate.HourInteger(p) >= 6 && GenLocalDate.HourInteger(p) <= 17) && !Find.World.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);
        public static bool IsDaylight(Map m) => (GenLocalDate.HourInteger(m) >= 6 && GenLocalDate.HourInteger(m) <= 17) && !Find.World.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);

        // Verse.Pawn
        public static string MainDesc(Pawn pawn)
        {
            string text = "ROMV_VampireDesc".Translate(new object[]
            {
                HediffVampirism.AddOrdinal(pawn.VampComp().Generation),
                pawn.VampComp().Bloodline.LabelCap
            });
            return text.CapitalizeFirst();
        }

        public static bool IsZero(IntVec3 loc)
        {
            return loc.x == 0 && loc.y == 0 && loc.z == 0;
        }



        public static void Heal(Pawn target, int maxInjuries = 4, int maxInjuriesPerBodyPartInit = 2)
        {
            int maxInjuriesPerBodyPart;
            foreach (BodyPartRecord rec in target.health.hediffSet.GetInjuredParts())
            {
                if (maxInjuries > 0)
                {
                    maxInjuriesPerBodyPart = maxInjuriesPerBodyPartInit;
                    foreach (Hediff_Injury current in from injury in target.health.hediffSet.GetHediffs<Hediff_Injury>() where injury.Part == rec select injury)
                    {
                        if (maxInjuriesPerBodyPart > 0)
                        {
                            if (current.CanHealNaturally() && !current.IsOld()) // basically check for scars and old wounds
                            {
                                current.Heal((int)current.Severity + 1);
                                maxInjuries--;
                                maxInjuriesPerBodyPart--;
                            }
                        }
                    }
                }
            }
        }

        public static IntVec3 FindCellSafeFromSunlight(Pawn pawn)
        {
            return CellFinderLoose.RandomCellWith(x => !IsZero(x) && x.IsValid && x.InBounds(pawn.MapHeld) && x.Roofed(pawn.MapHeld) && x.Walkable(pawn.MapHeld)
                    && pawn.Map.reachability.CanReach(pawn.PositionHeld, x, PathEndMode.OnCell, TraverseMode.ByPawn, Danger.Deadly), pawn.MapHeld, 1000);
        }

 
        public static void AdjustTimeTables(Pawn pawn)
        {
            if (pawn.IsVampire() && pawn.timetable is Pawn_TimetableTracker t)
            {
                t.times = new List<TimeAssignmentDef>(24);
                for (int i = 0; i < 24; i++)
                {
                    TimeAssignmentDef item;
                    if (i <= 5 || i > 18)
                    {
                        item = TimeAssignmentDefOf.Anything;
                    }
                    else
                    {
                        item = TimeAssignmentDefOf.Sleep;
                    }
                    t.times.Add(item);
                }
            }
        }

        // Make new vampires exhausted.
        public static void MakeSleepy(Pawn pawn)
        {
            if (pawn?.VampComp() is CompVampire v && pawn?.needs?.rest is Need_Rest r)
            {
                r.CurLevelPercentage = 0.05f;
            }
        }

        // RimWorld.ParentRelationUtility
        public static bool IsVampire(this Pawn pawn)
        {
            if (pawn != null && pawn?.GetComp<CompVampire>() is CompVampire v && v.IsVampire)
                return true;
            return false;
        }


        public static int GrapplerModifier(Pawn grappler)
        {
            int result = 0;
            if (grappler.IsVampire())
            {
                result += 20 - grappler.VampComp().Generation;
            }
            if (grappler.def == VampDefOf.ROMV_BatSpectralRace)
            {
                result += 5;
            }
            return result;
        }

        //public static bool CanGrapple(this Pawn grappler, Pawn victim)
        //{
        //    if (victim == null || victim.Dead)
        //    {
        //        return true;
        //    }

        //    //Check downed
        //    if (victim.Downed)
        //    {
        //        MoteMaker.ThrowText(grappler.DrawPos, grappler.Map, "ROMV_DownedGrapple".Translate(), -1f);
        //        return true;
        //    }

        //    if (victim.IsPrisonerOfColony && RestraintsUtility.InRestraints(victim))
        //    {
        //        MoteMaker.ThrowText(grappler.DrawPos, grappler.Map, "ROMV_PrisonerGrapple".Translate(), -1f);
        //        return true;
        //    }

        //    //Check line of sight.
        //    //if (!victim.CanSee(grappler))
        //    //{
        //    //    MoteMaker.ThrowText(grappler.DrawPos, grappler.Map, "ROMV_SneakGrapple".Translate(), -1f);
        //    //    return true;
        //    //}

        //    //Grapple check.
            
        //    int roll = Rand.Range(1, 20);
        //    int modifier = (int)grappler.RaceProps.baseBodySize;
        //    modifier += (grappler.RaceProps.Humanlike) ? grappler.skills.GetSkill(SkillDefOf.Melee).Level : 0;
        //    modifier += VampireUtility.GrapplerModifier(grappler);
        //    int difficulty = (int)victim.RaceProps.baseBodySize;
        //    difficulty += (victim.RaceProps.Humanlike) ? victim?.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 1 : 1;

        //    if (roll + modifier > difficulty)
        //    {
        //        MoteMaker.ThrowText(grappler.DrawPos, grappler.Map, roll + " + " + modifier + " = " + (roll + modifier) + " vs " + difficulty + " : " + StringsToTranslate.AU_CastSuccess, -1f);
        //        return true;
        //    }
        //    MoteMaker.ThrowText(grappler.DrawPos, grappler.Map, roll + " + " + modifier + " = " + (roll + modifier) + " vs " + difficulty + " : " + StringsToTranslate.AU_CastFailure, -1f);
        //    return false;
        //}

        // RimWorld.SiegeBlueprintPlacer
        public static IntVec3 FindHideyHoleSpot(ThingDef holeDef, Rot4 rot, IntVec3 center, Map map)
        {
            if (GenConstruct.CanPlaceBlueprintAt(holeDef, center, rot, map, false, null).Accepted)
            {
                return center;
            }
            CellRect cellRect = CellRect.CenteredOn(center, 8);
            cellRect.ClipInsideMap(map);
            IntVec3 randomCell = cellRect.RandomCell;
            if (!CellFinder.TryFindRandomCellNear(center, map, 5, (IntVec3 c) => c.Standable(map) &&
                (GenConstruct.CanPlaceBlueprintAt(holeDef, c, rot, map, false, null).Accepted) &&
                (map?.reachability?.CanReach(c, randomCell, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)) ?? false), out randomCell))
            {
                Log.Error("Found no place to build hideyhole for burning vampire.");
                randomCell = IntVec3.Invalid;
            }
            return randomCell;
        }


    }
}
