using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public static class VampireGen
{
    public const int VAMP_MINSPAWNGENERATION = 4;
    public const int VAMP_MAXSPAWNGENERATION = 14;

    public static int RandHigherGenerationWeak
    {
        get
        {
            var minGenWeaker = VampireSettings.Get.lowestActiveVampGen + 3;
            var maxGen = VAMP_MAXSPAWNGENERATION;
            minGenWeaker =
                minGenWeaker > maxGen ? maxGen : minGenWeaker;
            return Rand.Range(minGenWeaker, maxGen);
        }
    }

    public static int RandHigherGenerationTough
    {
        get
        {
            var minGen = VampireSettings.Get.lowestActiveVampGen;
            var maxGenTougher = 11;
            maxGenTougher = maxGenTougher < minGen ? minGen : maxGenTougher;
            var maxGen = Mathf.Clamp(maxGenTougher, minGen, maxGenTougher);
            return Rand.Range(minGen, maxGenTougher);
        }
    }

    public static int RandHigherGeneration =>
        Rand.Range(VampireSettings.Get.lowestActiveVampGen, VAMP_MAXSPAWNGENERATION);

    public static int RandLowerGeneration =>
        Rand.Range(VAMP_MINSPAWNGENERATION, VampireSettings.Get.lowestActiveVampGen); //3-6

    public static bool TryGiveVampirismHediff(Pawn pawn, int generation, BloodlineDef bloodline, Pawn sire,
        bool firstVampire = false)
    {
        try
        {
            var vampHediff = (HediffVampirism)HediffMaker.MakeHediff(VampDefOf.ROM_Vampirism, pawn);
            vampHediff.firstVampire = firstVampire;
            vampHediff.sire = sire?.VampComp() ?? null;
            vampHediff.generation = generation;
            vampHediff.bloodline = bloodline;
            pawn.health.AddHediff(vampHediff);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }

        return false;
    }

    public static bool TryGiveVampirismHediffFromSire(Pawn pawn, Pawn sire, bool firstVampire = false)
    {
        try
        {
            var vampHediff = (HediffVampirism)HediffMaker.MakeHediff(VampDefOf.ROM_Vampirism, pawn);
            vampHediff.firstVampire = firstVampire;
            vampHediff.sire = sire.VampComp();
            vampHediff.generation = sire.VampComp().Generation + 1;
            vampHediff.bloodline = sire.VampComp().Bloodline;
            pawn.health.AddHediff(vampHediff);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }

        return false;
    }

    public static void AddFangsHediff(Pawn pawn)
    {
        if (pawn != null)
        {
            if (pawn.RaceProps?.body?.GetPartsWithDef(DefDatabase<BodyPartDef>.
                    GetNamed("Tongue"))?.FirstOrDefault() is { } tongue)
            {
                pawn.health.RestorePart(tongue);
                pawn.health.AddHediff(pawn.VampComp().Bloodline.fangsHediff, tongue);
            }
            //Elder things have an 'eating tube' instead of a tongue
            else if (pawn.def.defName == "Alien_ElderThing_Race_Standard")
            {
                if (pawn.RaceProps?.body?.GetPartsWithDef(DefDatabase<BodyPartDef>.
                        GetNamed("ElderThing_EatingTube"))?.FirstOrDefault() is { } eatingTube)
                {
                    pawn.health.RestorePart(eatingTube);
                    pawn.health.AddHediff(pawn.VampComp().Bloodline.fangsHediff, eatingTube);
                }
                else Log.Error("ROM Vampires Error - Unable to add fangs to Elder Thing: No '??? body part record for " + pawn.LabelShort);
            }
            else Log.Error("ROM Vampires Error - Unable to add fangs: No 'tongue' body part record for " + pawn.LabelShort);
        }
        else Log.Error("ROM Vampires Error - Unable to add fangs: Attempted to add fangs to non-existent pawn");
    }

    //public static void AddTongueHediff(Pawn pawn)
    //{
    //    BodyPartRecord bpR = pawn.health?.hediffSet?.GetNotMissingParts().FirstOrDefault(x => x.def == DefDatabase<BodyPartDef>.GetNamedSilentFail("Tongue"));
    //    if (bpR == null) return;
    //    pawn.health.RestorePart(bpR);
    //    pawn.health.AddHediff(VampDefOf.ROMV_VampireTongue, bpR, null);

    //}

    public static void AddBloodlineHediff(Pawn pawn)
    {
        if (pawn.VampComp()?.Bloodline?.bloodlineHediff != null)
            pawn.health.AddHediff(pawn.VampComp().Bloodline.bloodlineHediff);
    }

    public static bool TryGiveVampireAdditionalHediffs(Pawn pawn)
    {
        try
        {
            AddFangsHediff(pawn);
            //AddTongueHediff(pawn);
            AddBloodlineHediff(pawn);
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return false;
        }

        return true;
    }

    public static PawnKindDef DetermineKindDef(int generation)
    {
        var result = PawnKindDef.Named("ROMV_VampireKind");
        if (generation == 1)
            result = PawnKindDef.Named("ROMV_FirstVampireKind");
        else if (generation <= 6)
            result = PawnKindDef.Named("ROMV_AncientVampireKind");
        else if (generation <= 9)
            result = PawnKindDef.Named("ROMV_GreaterVampireKind");
        else if (generation <= 12)
            result = PawnKindDef.Named("ROMV_VampireKind");
        else if (generation <= 14)
            result = PawnKindDef.Named("ROMV_LesserVampireKind");
        else
            result = PawnKindDef.Named("ROMV_ThinbloodVampireKind");
        return result;
    }

    public static Pawn GenerateVampire(int generation, BloodlineDef bloodline, Pawn sire, Faction vampFaction = null,
        bool firstVampire = false)
    {
        //Lower generation vampires are impossibly old.
        float? math = sire != null
            ?
            sire.ageTracker.AgeChronologicalYearsFloat + new FloatRange(100, 300).RandomInRange
            :
            generation > 4
                ? Mathf.Clamp(2000 - generation * Rand.Range(20, 200), 16, 2000)
                :
                100000 - generation * Rand.Range(10000, 50000);

        var faction = vampFaction != null ? vampFaction :
            generation < 7 ? Find.FactionManager.FirstFactionOfDef(VampDefOf.ROMV_LegendaryVampires) :
            VampireUtility.RandVampFaction;


        HarmonyPatches.VampireGenInProgress = true;
        var request = new PawnGenerationRequest(
            kind: DetermineKindDef(generation),
            faction: faction,
            context: PawnGenerationContext.NonPlayer,
            tile: -1,
            forceGenerateNewPawn: true,
            allowDead: false,
            allowDowned: false,
            canGeneratePawnRelations: generation > 7,
            mustBeCapableOfViolence: true,
            colonistRelationChanceFactor: generation > 7 ? 20f : 0f,
            forceAddFreeWarmLayerIfNeeded: false,
            allowGay: true,
            allowFood: false, //Why would they have it?
            allowAddictions: false,
            inhabitant: false,
            certainlyBeenInCryptosleep: false,
            forceRedressWorldPawnIfFormerColonist: false,
            worldPawnFactionDoesntMatter: false,
            biocodeWeaponChance: 0,
            biocodeApparelChance: 0,
            extraPawnForExtraRelationChance: null,
            relationWithExtraPawnChanceFactor: 0,
            validatorPreGear: null,
            validatorPostGear: null,
            forcedTraits: null,
            prohibitedTraits: null,
            minChanceToRedressWorldPawn: null,
            fixedBiologicalAge: math,
            fixedChronologicalAge: null,
            fixedGender: null,
            fixedLastName: null,
            fixedIdeo: null,
            forceNoIdeo: false,
            forceNoBackstory: false,
            forbidAnyTitle: false
        );

        var pawn = PawnGenerator.GeneratePawn(request);
        if (DebugSettings.godMode)
            Log.Message(pawn.Name + " Age:" + pawn.ageTracker.AgeNumberString + " Generation:" + generation);
        HarmonyPatches.VampireGenInProgress = false;

        if (firstVampire)
        {
            var caineName = new NameTriple("Caine", "Caine", "Darkfather");
            pawn.Name = caineName;
            pawn.gender = Gender.Male;
        }

        pawn.story.HairColor = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColor, 20);
        if (!bloodline.allowsHair)
            pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");
        pawn.VampComp().InitializeVampirism(sire, bloodline, generation, firstVampire);
        pawn.ageTracker.AgeBiologicalTicks = (long)(math * GenDate.TicksPerYear);
        //TryGiveVampirismHediff(pawn, generation, bloodline, sire, firstVampire);
        return pawn;
    }

    /// <summary>
    ///     It's best to clear all of these when a vampire is initialized.
    /// </summary>
    public static void RemoveMortalHediffs(Pawn AbilityUser)
    {
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x is HediffVampirism_VampGiver);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Malnutrition);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x is Hediff_Addiction);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.BadBack);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Asthma);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Cataract);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Carcinoma);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Flu);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.FoodPoisoning);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Frail);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Heatstroke);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Hypothermia);
        //AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Hangover);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Plague);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Blindness);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Malaria);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.ResurrectionPsychosis);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.ResurrectionSickness);
        AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.WoundInfection);
    }
}