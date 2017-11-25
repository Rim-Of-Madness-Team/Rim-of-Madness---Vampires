using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public static class BloodItemUtility
    {
        public static readonly int AMT_BLOODPACK = 3;
        public static readonly int AMT_BLOODVIAL = 1;

        public static bool ExtractionWillKill(Pawn extractee, bool isBloodPack = false)
        {
            int amt = isBloodPack ? AMT_BLOODPACK : AMT_BLOODVIAL;
            if (extractee?.BloodNeed()?.CurBloodPoints <= amt)
                return true;
            return false;
        }

        public static Thing SpawnBloodFromExtraction(Pawn extractee, bool isBloodPack = false)
        {
            BloodType type = BloodTypeUtility.BloodType(extractee);
            Thing result = null;
            switch (type)
            {
                case BloodType.Animal:
                    result = (Thing)ThingMaker.MakeThing(isBloodPack ? VampDefOf.BloodPack_Animal : null);
                    break;
                case BloodType.LowBlood:
                    result = (Thing)ThingMaker.MakeThing(isBloodPack ? VampDefOf.BloodPack_LowBlood : VampDefOf.BloodVial_LowBlood);
                    break;
                case BloodType.AverageBlood:
                    result = (Thing)ThingMaker.MakeThing(isBloodPack ? VampDefOf.BloodPack_AverageBlood : VampDefOf.BloodPack_AverageBlood);
                    break;
                case BloodType.HighBlood:
                    result = (Thing)ThingMaker.MakeThing(isBloodPack ? VampDefOf.BloodPack_HighBlood : VampDefOf.BloodVial_HighBlood);
                    break;
                case BloodType.Special:
                    result = (Thing)ThingMaker.MakeThing(isBloodPack ? null : VampDefOf.BloodVial_Special);
                    break;
            }
            if (result != null)
            {
                result.stackCount = 1;
                GenPlace.TryPlaceThing(result, extractee.PositionHeld, extractee.Map, ThingPlaceMode.Near);
                extractee.BloodNeed().AdjustBlood(isBloodPack ? -AMT_BLOODPACK : -AMT_BLOODVIAL);
            }
            return result;
        }

    }
}
