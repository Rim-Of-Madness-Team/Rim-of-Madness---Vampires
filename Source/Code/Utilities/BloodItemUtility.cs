using Verse;

namespace Vampire;

public static class BloodItemUtility
{
    public static readonly int AMT_BLOODPACK = 3;
    public static readonly int AMT_BLOODWINE = 1;
    public static readonly int AMT_BLOODVIAL = 1;

    public static bool ExtractionWillKill(Pawn extractee, BloodExtractType bloodExtractType)
    {
        var amt = 0;
        switch (bloodExtractType)
        {
            case BloodExtractType.Vial:
                amt = AMT_BLOODVIAL;
                break;
            case BloodExtractType.Wine:
                amt = AMT_BLOODWINE;
                break;
            case BloodExtractType.Pack:
                amt = AMT_BLOODPACK;
                break;
        }

        if (extractee?.BloodNeed() is { } bn && (bn.CurBloodPoints <= amt || bn.DrainingIsDeadly))
            return true;
        return false;
    }

    public static Thing SpawnBloodFromExtraction(Pawn extractee, BloodExtractType bloodExtractType)
    {
        var type = BloodTypeUtility.BloodType(extractee);
        Thing result = null;
        switch (type)
        {
            case BloodType.Animal:
                switch (bloodExtractType)
                {
                    case BloodExtractType.Vial:
                    case BloodExtractType.Pack:
                    case BloodExtractType.Wine:
                        result = ThingMaker.MakeThing(VampDefOf.BloodPack_Animal);
                        break;
                }

                break;
            case BloodType.LowBlood:
                switch (bloodExtractType)
                {
                    case BloodExtractType.Vial:
                        result = ThingMaker.MakeThing(VampDefOf.BloodVial_LowBlood);
                        break;
                    case BloodExtractType.Pack:
                        result = ThingMaker.MakeThing(VampDefOf.BloodPack_LowBlood);
                        break;
                    case BloodExtractType.Wine:
                        result = ThingMaker.MakeThing(VampDefOf.BloodWine_LowBlood);
                        break;
                }

                break;
            case BloodType.AverageBlood:
                switch (bloodExtractType)
                {
                    case BloodExtractType.Vial:
                        result = ThingMaker.MakeThing(VampDefOf.BloodVial_AverageBlood);
                        break;
                    case BloodExtractType.Pack:
                        result = ThingMaker.MakeThing(VampDefOf.BloodPack_AverageBlood);
                        break;
                    case BloodExtractType.Wine:
                        result = ThingMaker.MakeThing(VampDefOf.BloodWine_AverageBlood);
                        break;
                }

                break;
            case BloodType.HighBlood:
                switch (bloodExtractType)
                {
                    case BloodExtractType.Vial:
                        result = ThingMaker.MakeThing(VampDefOf.BloodVial_HighBlood);
                        break;
                    case BloodExtractType.Pack:
                        result = ThingMaker.MakeThing(VampDefOf.BloodPack_HighBlood);
                        break;
                    case BloodExtractType.Wine:
                        result = ThingMaker.MakeThing(VampDefOf.BloodWine_HighBlood);
                        break;
                }

                break;
            case BloodType.Special:
                switch (bloodExtractType)
                {
                    case BloodExtractType.Vial:
                    case BloodExtractType.Pack:
                        result = ThingMaker.MakeThing(VampDefOf.BloodVial_Special);
                        break;
                    case BloodExtractType.Wine:
                        result = ThingMaker.MakeThing(VampDefOf.BloodWine_Special);
                        break;
                }

                break;
        }

        if (result != null)
        {
            result.stackCount = 1;
            GenPlace.TryPlaceThing(result, extractee.PositionHeld, extractee.Map, ThingPlaceMode.Near);

            var bloodAdjustAmount = 0;
            switch (bloodExtractType)
            {
                case BloodExtractType.Vial:
                    bloodAdjustAmount = -AMT_BLOODVIAL;
                    break;
                case BloodExtractType.Wine:
                    bloodAdjustAmount = -AMT_BLOODWINE;
                    break;
                case BloodExtractType.Pack:
                    bloodAdjustAmount = -AMT_BLOODPACK;
                    break;
            }

            extractee.BloodNeed().AdjustBlood(bloodAdjustAmount);
        }

        return result;
    }
}