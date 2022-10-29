using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire;

public class Alert_VampireStarving : Alert
{
    public Alert_VampireStarving()
    {
        defaultLabel = "ROMV_Alert_StarvingVampire".Translate();
        defaultPriority = AlertPriority.Critical;
    }

    private IEnumerable<Pawn> VampiresStarving
    {
        get
        {
            var maps = Find.Maps;
            for (var i = 0; i < maps.Count; i++)
                if (maps[i].IsPlayerHome)
                    foreach (var p in maps[i].mapPawns.FreeColonistsSpawned)
                        if (p.VampComp() is { } v && v.IsVampire)
                            if (p?.BloodNeed().CurLevelPercentage < 0.3f)
                                yield return p;
        }
    }

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        foreach (var current in VampiresStarving) stringBuilder.AppendLine("    " + current.Name.ToStringShort);
        return string.Format("ROMV_Alert_StarvingVampireDesc".Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        if (Find.AnyPlayerHomeMap == null) return false;
        var pawn = VampiresStarving.FirstOrDefault();
        if (pawn == null) return false;
        return AlertReport.CulpritIs(pawn);
    }
}