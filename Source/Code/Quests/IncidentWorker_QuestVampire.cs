using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire;

public class IncidentWorker_QuestVampire : IncidentWorker
{
    private const float RelationsImprovement = 8f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        int tile;
        if (!TileFinder.TryFindNewSiteTile(out tile)) return false;
        var site = SiteMaker.MakeSite(SitePartDefOf.Outpost, tile,
            Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROMV_Sabbat")));
        site.Tile = tile;
        site.GetComponent<HandleVampireQuestComp>().StartQuest();
        Find.WorldObjects.Add(site);
        SendStandardLetter(parms, null);
        return true;
    }

    private bool AnyQuestExistsFrom(Faction faction)
    {
        var sites = Find.WorldObjects.Sites;
        for (var i = 0; i < sites.Count; i++)
        {
            var component = sites[i].GetComponent<HandleVampireQuestComp>();
            if (component != null && component.Active) return true;
        }

        return false;
    }

    private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
    {
        return CommonHumanlikeEnemyFaction(f1, f2) != null;
    }

    private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
    {
        Faction result;
        if ((from x in Find.FactionManager.AllFactions
                where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) &&
                      x.HostileTo(f2)
                select x).TryRandomElement(out result))
            return result;
        return null;
    }
}