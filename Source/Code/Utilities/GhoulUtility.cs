using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public static partial class GhoulUtility
{
    /// <summary>
    ///     Vampire the Masquerade rules give extra discipline levels for
    ///     Ghouls that have vampiric masters of higher generations.
    /// </summary>
    /// <param name="ghoulPawn"></param>
    /// <param name="regnant">ghoulPawn's blood source character</param>
    /// <returns></returns>
    public static GhoulPower GetGhoulPower(Pawn ghoulPawn, Pawn regnant = null)
    {
        var regnantPawn = regnant ?? ghoulPawn.VampComp().ThrallData.Regnant;
        if (regnantPawn == null) return GhoulPower.Modern;
        switch (regnantPawn.VampComp().Generation)
        {
            case 13:
            case 12:
            case 11:
            case 10:
            case 9:
            case 8:
                return GhoulPower.Modern;
            case 7:
                return GhoulPower.Old;
            case 6:
                return GhoulPower.Ancient;
            case 5:
            case 4:
            case 3:
            case 2:
            case 1:
                return GhoulPower.Primeval;
        }

        return GhoulPower.Modern;
    }


    /// <summary>
    ///     Similar method as GenerateVampire, except it initializes ghouls.
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="bloodline"></param>
    /// <param name="domitor"></param>
    /// <param name="vampFaction"></param>
    /// <param name="isRevenant"></param>
    /// <returns></returns>
    public static Pawn GenerateGhoul(int generation, BloodlineDef bloodline, Pawn domitor,
        Faction vampFaction = null, bool isRevenant = false)
    {
        //Lower generation ghouls are impossibly old.
        float? math = domitor != null
            ?
            domitor.ageTracker.AgeChronologicalYearsFloat + new FloatRange(100, 300).RandomInRange
            :
            generation > 4
                ? Mathf.Clamp(2000 - generation * Rand.Range(20, 200), 16, 2000)
                :
                100000 - generation * Rand.Range(10000, 50000);

        var faction = vampFaction != null ? vampFaction :
            generation < 7 ? Find.FactionManager.FirstFactionOfDef(VampDefOf.ROMV_LegendaryVampires) :
            VampireUtility.RandVampFaction;
        var request = new PawnGenerationRequest(
            PawnKindDefOf.SpaceRefugee, Faction.OfAncients, PawnGenerationContext.NonPlayer,
            -1, false, false, false, false, true, 0f, true, false, true,
            true, false, false, false, false, false, 0, 0, null, 0, null, null, null, null, null);
        var pawn = PawnGenerator.GeneratePawn(request);
        pawn.story.HairColor = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColor, 20);
        if (!bloodline.allowsHair)
            pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");
        pawn.VampComp().InitializeGhoul(domitor, isRevenant);
        return pawn;
    }

    /// <summary>
    ///     Checks the vampirism component to see if the character is a ghoul.
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns></returns>
    public static bool IsGhoul(this Pawn pawn)
    {
        return pawn != null && pawn.GetComp<CompVampire>() is { } v && v.IsGhoul;
    }

    public static void GiveVitaeEffects(Pawn receiver, Pawn donor)
    {
        var pawn = receiver;

        //Give Vitae High Effect
        var vitaeHighHediff = HediffMaker.MakeHediff(VampDefOf.ROMV_VitaeHigh, pawn);
        var numHigh = 0.75f;
        AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, VampDefOf.ROMV_VitaeChemical, ref numHigh);
        vitaeHighHediff.Severity = numHigh;
        pawn.health.AddHediff(vitaeHighHediff);

        //Give Vitae Tolerance Effect
        var vitaeToleranceHediff = HediffMaker.MakeHediff(VampDefOf.ROMV_VitaeTolerance, pawn);
        var numTol = 0.035f;
        numTol /= receiver.BodySize;
        AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, VampDefOf.ROMV_VitaeChemical, ref numTol);
        vitaeToleranceHediff.Severity = numTol;
        pawn.health.AddHediff(vitaeToleranceHediff);

        const float addictiveness = 1.0f;
        const float minToleranceToAddict = 0.01f;
        const float existingAddictionSeverityOffset = 0.2f;

        var needLevelOffset = 1f;
        var overdoseSeverityOffset = new FloatRange(0.18f, 0.35f);
        var chemical = VampDefOf.ROMV_VitaeChemical;

        var addictionHediffDef = VampDefOf.ROMV_VitaeAddiction;
        var lookTarget = receiver;
        var hediff = AddictionUtility.FindToleranceHediff(lookTarget, VampDefOf.ROMV_VitaeChemical);
        var num = hediff?.Severity ?? 0f;
        var hediffAddiction = AddictionUtility.FindAddictionHediff(lookTarget, VampDefOf.ROMV_VitaeChemical);

        if (hediffAddiction != null)
        {
            hediffAddiction.Severity += existingAddictionSeverityOffset;
        }
        else if (Rand.Value < addictiveness && num >= minToleranceToAddict)
        {
            lookTarget.health.AddHediff(addictionHediffDef);
            if (PawnUtility.ShouldSendNotificationAbout(lookTarget))
                Find.LetterStack.ReceiveLetter("LetterLabelNewlyAddicted".Translate(chemical.label).CapitalizeFirst(),
                    "LetterNewlyAddicted".Translate(lookTarget.LabelShort, chemical.label, lookTarget.Named("PAWN"))
                        .AdjustedFor(lookTarget).CapitalizeFirst(), LetterDefOf.NegativeEvent, lookTarget);
            AddictionUtility.CheckDrugAddictionTeachOpportunity(lookTarget);
        }

        if (addictionHediffDef.causesNeed != null)
        {
            var need = lookTarget.needs.AllNeeds.Find(x => x.def == addictionHediffDef.causesNeed);
            if (need != null)
            {
                AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(lookTarget, chemical,
                    ref needLevelOffset);
                need.CurLevel += needLevelOffset;
            }
        }
//            var firstHediffOfDef = lookTarget.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose, false);
//            var num2 = firstHediffOfDef?.Severity ?? 0f;
//            if (num2 < 0.9f && Rand.Value < largeOverdoseChance)
//            {
//                var num3 = Rand.Range(0.85f, 0.99f);
//                HealthUtility.AdjustSeverity(lookTarget, HediffDefOf.DrugOverdose, num3 - num2);
//                if (lookTarget.Faction == Faction.OfPlayer)
//                {
//                    Messages.Message("MessageAccidentalOverdose".Translate(new object[]
//                    {
//                        lookTarget.LabelIndefinite(),
//                        chemical.LabelCap
//                    }).CapitalizeFirst(), MessageTypeDefOf.NegativeHealthEvent);
//                }
//            }
//            else
//            {
//                var num4 = overdoseSeverityOffset.RandomInRange / lookTarget.BodySize;
//                if (num4 > 0f)
//                {
//                    HealthUtility.AdjustSeverity(lookTarget, HediffDefOf.DrugOverdose, num4);
//                }
//            }
    }

    public static void MakeGrayscale(this Texture2D tex)
    {
        var texColors = tex.GetPixels();
        for (var i = 0; i < texColors.Length; i++)
        {
            var grayValue = texColors[i].grayscale;
            texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
        }

        tex.SetPixels(texColors);
        tex.Apply();
    }
}