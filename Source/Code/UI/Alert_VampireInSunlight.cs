using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire;

public class Alert_VampireInSunlight : Alert
{
    public Alert_VampireInSunlight()
    {
        defaultLabel = "ROMV_Alert_VampireInTheSun".Translate();
        defaultPriority = AlertPriority.Critical;
    }

    private IEnumerable<Pawn> VampiresInTheSun
    {
        get
        {
            var maps = Find.Maps;
            for (var i = 0; i < maps.Count; i++)
                if (maps[i].IsPlayerHome)
                    foreach (var p in maps[i].mapPawns.FreeColonistsSpawned)
                        if (p.VampComp() is { } v && v.IsVampire)
                            if (v.InSunlight)
                                yield return p;
        }
    }

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        foreach (var current in VampiresInTheSun) stringBuilder.AppendLine("    " + current.Name.ToStringShort);
        return string.Format("ROMV_Alert_VampireInTheSunDesc".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        if (Find.AnyPlayerHomeMap == null) return false;
        var pawn = VampiresInTheSun.FirstOrDefault();
        if (pawn == null) return false;
        return AlertReport.CulpritIs(pawn);
    }
}