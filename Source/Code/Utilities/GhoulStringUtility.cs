using Verse;

namespace Vampire;

public static partial class GhoulUtility
{
    /// <summary>
    ///     Returns the appropriate translated string for each ghoul power level.
    /// </summary>
    /// <param name="ghoulPower"></param>
    /// <returns></returns>
    public static string GetString(this GhoulPower ghoulPower)
    {
        switch (ghoulPower)
        {
            case GhoulPower.Modern:
                return "ROMV_GhoulPower_Modern".Translate();

            case GhoulPower.Old:
                return "ROMV_GhoulPower_Old".Translate();

            case GhoulPower.Ancient:
                return "ROMV_GhoulPower_Ancient".Translate();

            case GhoulPower.Primeval:
                return "ROMV_GhoulPower_Primeval".Translate();
        }

        return "ROMV_GhoulPower_Modern".Translate();
    }

    /// <summary>
    ///     Returns the description for the Ghoul character card.
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns></returns>
    public static string MainDesc(Pawn pawn)
    {
        var text = "ROMV_GhoulDesc".Translate(new object[]
        {
            pawn.VampComp()?.GhoulHediff.ghoulPower.GetString(),
            pawn.VampComp()?.ThrallData?.Regnant?.VampComp()?.Bloodline?.LabelCap ?? ""
        });
        return text.CapitalizeFirst();
    }
}