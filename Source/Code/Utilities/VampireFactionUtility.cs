using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

public class BloodlineChance
{
    public BloodlineChance(BloodlineDef bloodline, float chance)
    {
        Bloodline = bloodline;
        Chance = chance;
    }

    public BloodlineDef Bloodline { get; set; }

    public float Chance { get; set; }
}

public static class VampireFactionUtility
{
    public static readonly List<string> VampireFactions = new()
    {
        "ROMV_Sabbat",
        "ROMV_Anarch",
        "ROMV_Camarilla"
    };

    public static readonly List<BloodlineChance> BloodlinesForSabbat = new()
    {
        new BloodlineChance(VampDefOf.ROMV_ClanLasombra, 10.0f),
        new BloodlineChance(VampDefOf.ROMV_ClanTzimize, 8.0f),
        new BloodlineChance(VampDefOf.ROMV_ClanPijavica, 1.0f)
    };

    public static readonly List<BloodlineChance> BloodlinesForCamarilla = new()
    {
        new BloodlineChance(VampDefOf.ROMV_ClanVentrue, 1.0f),
        new BloodlineChance(VampDefOf.ROMV_ClanGargoyle, 1.0f),
        new BloodlineChance(VampDefOf.ROMV_ClanNosferatu, 1.0f),
        new BloodlineChance(VampDefOf.ROMV_ClanTremere, 1.0f)
    };

    public static readonly List<BloodlineChance> BloodlinesForAnarch = new()
    {
        new BloodlineChance(VampDefOf.ROMV_ClanGangrel, 1.0f)
    };

    public static bool IsVampireFaction(this Faction fac)
    {
        if (fac != null && VampireFactions.Any(x => x == fac.def.defName))
            return true;
        return false;
    }


    /// <summary>
    ///     Returns number of vampires in faction.
    /// </summary>
    public static int GetVampiresInFactionCount(Faction faction)
    {
        if (faction == null)
            return 0;

        var num = 0;
        using (var enumerator = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).GetEnumerator())
        {
            while (enumerator.MoveNext())
                if (enumerator.Current.IsVampire(true))
                    num++;
        }

        return num;
    }
}