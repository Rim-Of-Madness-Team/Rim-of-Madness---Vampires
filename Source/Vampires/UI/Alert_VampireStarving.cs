using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Vampire
{
    public class Alert_VampireStarving : Alert
    {
        private IEnumerable<Pawn> VampiresStarving
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (maps[i].IsPlayerHome)
                    {
                        foreach (Pawn p in maps[i].mapPawns.FreeColonistsSpawned)
                        {
                            if (p.VampComp() is CompVampire v && v.IsVampire)
                            {
                                if (p?.BloodNeed().CurLevelPercentage < 0.3f)
                                    yield return p;
                            }
                        }
                    }
                }
            }
        }

        public Alert_VampireStarving()
        {
            this.defaultLabel = "ROMV_Alert_StarvingVampire".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.VampiresStarving)
            {
                stringBuilder.AppendLine("    " + current.NameStringShort);
            }
            return string.Format("ROMV_Alert_StarvingVampireDesc".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            if (Find.AnyPlayerHomeMap == null)
            {
                return false;
            }
            Pawn pawn = this.VampiresStarving.FirstOrDefault<Pawn>();
            if (pawn == null)
            {
                return false;
            }
            return AlertReport.CulpritIs(pawn);
        }
    }
}
