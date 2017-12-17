using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

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
        public static bool IsDaylight(Pawn p)
        {
            if (p != null && p.Spawned && p.MapHeld != null)
            {
                return IsDaylight(p.MapHeld);
            }
            return false;
        }
            
        public static void SummonEffect(IntVec3 loc, Map map, Thing summoner, float size)
        {
            GenExplosion.DoExplosion(loc, map, size, DamageDefOf.EMP, summoner, -1, DamageDefOf.Stun.soundExplosion);

        }

        //=> (GenLocalDate.HourInteger(p) >= 6 && GenLocalDate.HourInteger(p) <= 17) && !Find.World.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);
        public static bool IsDaylight(Map m)
        {
            float num = GenCelestial.CurCelestialSunGlow(m);
            if (GenCelestial.IsDaytime(num))
            {
                return true;
            }
            return false;
        }

        //=> (GenLocalDate.HourInteger(m) >= 6 && GenLocalDate.HourInteger(m) <= 17) && !Find.World.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);

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


        public static void GiveVampXP(this Pawn vampire, int amount=15)
        {
            if (vampire?.VampComp() is CompVampire v && v.IsVampire && vampire.Faction == Faction.OfPlayer)
            {
                MoteMaker.ThrowText(vampire.DrawPos + new Vector3(0, 0, 0.1f), vampire.Map, "XP +" + amount, -1f);
                v.XP += amount;
            }
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

        public static Color ColorAndroidCoolant = new Color(153, 217, 234);
        public static Color ColorAndroidCoolantVitae = new Color(183, 217, 234);
        public static Color ColorBlood = new Color(0.73f, 0.02f, 0.02f);
        public static Color ColorVitae = new Color(0.65f, 0.008f, 0.008f);
        public static bool IsAndroid(this Pawn pawn)
        {
            if (pawn != null && pawn?.def?.race?.hediffGiverSets?.FirstOrDefault(x => x.defName == "ChjAndroidStandard") != null)
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


    }
}
