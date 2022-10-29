using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public static partial class VampireUtility
{
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
            return IsDaylight(p.MapHeld);
        return false;
    }

    public static bool IsVampire(this Pawn pawn, bool aiCheck)
    {
        if (!aiCheck)
        {
            if (pawn != null && pawn?.TryGetComp<CompVampire>() is { } v && v.IsVampire)
                return true;
            return false;
        }

        return VampireTracker.IsVampire(pawn);
    }

    public static bool HasVampireHediffs(this Pawn pawn)
    {
        if (pawn == null) return false;
        return pawn.health.hediffSet.HasHediff(VampDefOf.ROM_Vampirism) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismRandom) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismGargoyle) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismLasombra) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismPijavica) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismTremere) ||
               pawn.health.hediffSet.HasHediff(VampDefOf.ROM_VampirismTzimisce);
    }

    public static bool IsCoolantUser(this Pawn pawn)
    {
        if (DefDatabase<CoolantUsers>.GetNamedSilentFail("ROMV_CoolantUsers") is { } coolantUsers)
        {
            if (coolantUsers.hediffGiverDefs != null)
                foreach (var defString in coolantUsers.hediffGiverDefs)
                    if (pawn != null && pawn?.def?.race?.hediffGiverSets?.FirstOrDefault(x => x.defName == defString) !=
                        null)
                        return true;

            if (coolantUsers.raceThingDefs != null)
                foreach (var raceDef in coolantUsers.raceThingDefs)
                    if (pawn != null && pawn?.def?.defName == raceDef)
                        return true;
        }

        return false;
    }

    internal static void RemoveVampirism(Pawn pawn, bool showMoteText = false, bool showMessages = false)
    {
        if (pawn != null)
        {
            if (pawn.IsVampire(false))
            {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROM_Vampirism) is HediffVampirism vampirism)
                    pawn.health.RemoveHediff(vampirism);
                if (pawn?.health?.hediffSet?.GetFirstHediff<Hediff_AddedPart_Fangs>() is { } fangs)
                {
                    var rec = fangs.Part;
                    pawn.health.RemoveHediff(fangs);
                    pawn.health.RestorePart(rec);
                }

                if (pawn.BloodNeed() is { } needBlood)
                    if (needBlood.CurBloodPoints >= needBlood.MaxBloodPoints)
                        needBlood.CurBloodPoints = needBlood.MaxBloodPoints;
                if (pawn.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROMV_SunExposure) is { } sunExposure)
                    pawn.health.RemoveHediff(sunExposure);

                if (pawn.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROMV_TheBeast) is { } beast)
                    pawn.health.RemoveHediff(beast);
                VampireTracker.RemoveVampire(pawn);


                pawn.Drawer.Notify_DebugAffected();
                if (showMoteText)
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ROMV_NoLongerAVampire".Translate(pawn.LabelShort));
                if (showMessages)
                    Messages.Message("ROMV_NoLongerAVampire".Translate(pawn.LabelCap), MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                if (showMoteText)
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ROMV_WasNotAVampire".Translate(pawn.LabelShort));
                if (showMessages)
                    Messages.Message("ROMV_WasNotAVampire".Translate(pawn.LabelCap), MessageTypeDefOf.RejectInput);
            }
        }
    }

    public static CompVampire VampComp(this Pawn pawn)
    {
        return pawn?.TryGetComp<CompVampire>();
    }

    //=> (GenLocalDate.HourInteger(p) >= 6 && GenLocalDate.HourInteger(p) <= 17) && !Find.World.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);
    public static bool IsDaylight(Map m)
    {
        var num = GenCelestial.CurCelestialSunGlow(m);
        if (GenCelestial.IsDaytime(num) &&
            IsForcedDarknessConditionInactive(m))
            return true;
        return false;
    }

    public static bool IsForcedDarknessConditionInactive(Map m)
    {
        return !m.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse) &&
               !BloodMoonConditionActive(m);
    }


    public static bool BloodMoonConditionActive(Map m)
    {
        if (DefDatabase<GameConditionDef>.GetNamedSilentFail("HPLovecraft_BloodMoon") != null)
            return m.gameConditionManager.ConditionIsActive(GameConditionDef.Named("HPLovecraft_BloodMoon"));
        return false;
    }

    //Checks for sunrise conditions.
    public static bool IsSunRisingOrDaylight(this Pawn p)
    {
        return IsSunRisingOrDaylight(p.MapHeld);
    }

    //private static Dictionary<Map, float> lastSunlightChecks = new Dictionary<Map, float>();

    //Sunrise is very dangerous to be out.
    public static bool IsSunRisingOrDaylight(Map m)
    {
        //If it's daylight, it's not safe.
        if (GenCelestial.IsDaytime(GenCelestial.CurCelestialSunGlow(m))) return true;
        if (GenCelestial.CurCelestialSunGlow(m) > 0.01f) return true;

        //if (curSunlight > 0.01f)
        //{
        //    var lastSunlight = 0f;
        //    if (!lastSunlightChecks.ContainsKey(m))
        //    {
        //        lastSunlightChecks.Add(m, curSunlight);
        //        lastSunlight = curSunlight;
        //    }
        //    else
        //    {
        //        lastSunlight = lastSunlightChecks[m];
        //    }
        //    return curSunlight > lastSunlight;
        //}
        return false;
    }

    public static string MainDesc(Pawn pawn)
    {
        var text = "ROMV_VampireDesc".Translate(new object[]
        {
            VampireStringUtility.AddOrdinal(pawn.VampComp().Generation),
            pawn.VampComp().Bloodline.LabelCap
        });
        return text.CapitalizeFirst();
    }


    public static void SummonEffect(IntVec3 loc, Map map, Thing summoner, float size)
    {
        ExplosionUtility.DoExplosion(loc, map, size, DamageDefOf.EMP, null, -1, DamageDefOf.Stun.soundExplosion);
    }

    public static void GiveVampXP(this Pawn vampire, int amount = 15)
    {
        if (vampire?.VampComp() is { } v && v.IsVampire && vampire.Faction == Faction.OfPlayer)
        {
            MoteMaker.ThrowText(vampire.DrawPos + new Vector3(0, 0, 0.1f), vampire.Map, "XP +" + amount);
            v.XP += amount;
        }
    }
    
    public static IEnumerable<Hediff_Injury> GetAllInjuries(Pawn pawn)
    {
        int num;
        if (pawn?.health?.hediffSet != null)
        {
            for (var i = 0; i < pawn.health.hediffSet.hediffs.Count; i = num)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury { } hediffInjury)
                {
                    yield return hediffInjury;
                }

                num = i + 1;
            }
        }
    }

    /// <summary>
    ///     Heals body part injuries for Vampires.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="maxInjuries"></param>
    /// <param name="maxInjuriesPerBodyPartInit"></param>
    public static void Heal(Pawn target, int maxInjuries = 4, int maxInjuriesPerBodyPartInit = 2,
        bool scarsOnly = false)
    {
        foreach (var rec in target.health.hediffSet.GetInjuredParts())
        {
            if (maxInjuries <= 0) continue;
            var maxInjuriesPerBodyPart = maxInjuriesPerBodyPartInit;
            foreach (var current in from injury in 
                         GetAllInjuries(target)
                     where injury.Part == rec
                     select injury)
            {
                if (maxInjuriesPerBodyPart <= 0) continue;
                if (!scarsOnly)
                {
                    if (!current.CanHealNaturally() || current.IsPermanent()) continue;
                    current.Heal((int)current.Severity + 1);
                    maxInjuries--;
                    maxInjuriesPerBodyPart--;
                }
                else
                {
                    if (current.CanHealNaturally() && !current.IsPermanent()) continue;
                    current.Heal((int)current.Severity + 1);
                    maxInjuries--;
                    maxInjuriesPerBodyPart--;
                }
            }
        }
    }

    /// <summary>
    ///     Regenerates a random part or limb of the character.
    /// </summary>
    /// <param name="target"></param>
    public static void RegenerateRandomPart(Pawn target)
    {
//Null checks
        if (target == null) return;
        var missingParts = new List<Hediff_MissingPart>()
            .Concat(target?.health?.hediffSet?.GetMissingPartsCommonAncestors()).ToList();
        if (missingParts.NullOrEmpty()) return;


        var partToRestore = missingParts.RandomElement();
        var part = partToRestore.Part;
        if (target.health != null)
        {
            target.health.RestorePart(part);


            if (part?.def == DefDatabase<BodyPartDef>.GetNamedSilentFail("Tongue"))
                target.health.AddHediff(target.VampComp().Bloodline.fangsHediff, part);
        }

        Messages.Message("ROMV_LimbRegen".Translate(new object[]
        {
            target.LabelShort,
            partToRestore.Part.def.label
        }), MessageTypeDefOf.PositiveEvent);
    }


    /// <summary>
    ///     Forces vampires to have night-time work assignments.
    /// </summary>
    /// <param name="pawn"></param>
    public static void AdjustTimeTables(Pawn pawn)
    {
        if (pawn.IsVampire(false) && pawn.timetable is { } t)
        {
            t.times = new List<TimeAssignmentDef>(24);
            for (var i = 0; i < 24; i++)
            {
                TimeAssignmentDef item;
                if (i <= 5 || i > 18)
                    item = TimeAssignmentDefOf.Anything;
                else
                    item = TimeAssignmentDefOf.Sleep;
                t.times.Add(item);
            }
        }
    }


    /// <summary>
    ///     Forces a vampire to sleep.
    /// </summary>
    /// <param name="pawn"></param>
    public static void MakeSleepy(Pawn pawn)
    {
        if (VampireSettings.Get.slumberToggle && pawn?.VampComp() is { } v && pawn?.needs?.rest is { } r)
            r.CurLevelPercentage = 0.05f;
    }


    /// <summary>
    ///     Returns grappler modifier for vampires
    /// </summary>
    /// <param name="grappler"></param>
    /// <returns></returns>
    public static int GrapplerModifier(Pawn grappler)
    {
        var result = 0;
        if (grappler.IsVampire(true)) result += 20 - grappler.VampComp().Generation;
        if (grappler.def == VampDefOf.ROMV_BatSpectralRace) result += 15;
        if (grappler.def == VampDefOf.ROMV_BloodMistRace) result += 15;
        return result;
    }

    /// <summary>
    ///     Checks if something is a vampire bed.
    /// </summary>
    /// <param name="bed"></param>
    /// <returns></returns>
    public static bool IsVampireBed(this Thing bed)
    {
        return bed != null &&
               (
                   bed.def == ThingDef.Named("ROMV_SimpleCoffinBed") ||
                   bed.def == ThingDef.Named("ROMV_RoyalCoffinBed") ||
                   bed.def == ThingDef.Named("ROMV_SarcophagusBed")
               );
    }

    /// <summary>
    ///     Finds the generation hediff to give to a vampire.
    /// </summary>
    public static HediffDef GenerationDef(this Pawn pawn)
    {
        switch (pawn?.VampComp()?.Generation ?? 13)
        {
            case 1:
                return VampDefOfTwo.ROM_Generations_Caine;
            case 2:
                return VampDefOfTwo.ROM_Generations_TheThree;
            case 3:
                return VampDefOfTwo.ROM_Generations_Antediluvian;
            case 4:
            case 5:
                return VampDefOfTwo.ROM_Generations_Methuselah;
            case 6:
            case 7:
            case 8:
                return VampDefOfTwo.ROM_Generations_Elder;
            case 9:
            case 10:
                return VampDefOfTwo.ROM_Generations_Ancillae;
            case 11:
            case 12:
            case 13:
                return VampDefOfTwo.ROM_Generations_Neonate;
        }

        return VampDefOfTwo.ROM_Generations_Thinblood;
    }
}