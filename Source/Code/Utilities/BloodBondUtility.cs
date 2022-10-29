using Verse;

namespace Vampire;

public static class BloodBondUtility
{
    public static string GetString(this BondStage bondStage)
    {
        switch (bondStage)
        {
            case BondStage.None:
                return "ROMV_BondStage_None".Translate();
            case BondStage.FirstTaste:
                return "ROMV_BondStage_FirstTaste".Translate();
            case BondStage.SecondTaste:
                return "ROMV_BondStage_SecondTaste".Translate();
            case BondStage.Thrall:
                return "ROMV_BondStage_Thrall".Translate();
        }

        return "ROMV_BondStage_None".Translate();
    }
}