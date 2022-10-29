using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire;

public class IncidentWorker_AncientVampireCrypt : IncidentWorker
{
    private const float RelationsImprovement = 8f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        Faction faction;
        Faction faction2;
        int num;
        return base.CanFireNowSub(parms) &&
               TryFindNewSiteTile(out num);
    }


    //The same as TileFinder.TryFindNewSiteTile EXCEPT
    // this one finds locations with caves
    public static bool TryFindNewSiteTile(out int tile, int minDist = 8, int maxDist = 30,
        bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
    {
        var findTile = delegate(int root)
        {
            var minDist2 = minDist;
            var maxDist2 = maxDist;
            Predicate<int> validator = x =>
                !Find.WorldObjects.AnyWorldObjectAt(x) &&
                Find.World.HasCaves(x) && TileFinder.IsValidTileForNewSettlement(x);
            var preferCloserTiles2 = preferCloserTiles;
            int result;
            if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator,
                    false, TileFinderMode.Near))
                return result;
            return -1;
        };
        int arg;
        if (nearThisTile != -1)
        {
            arg = nearThisTile;
        }
        else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, x => findTile(x) != -1))
        {
            tile = -1;
            return false;
        }

        tile = findTile(arg);
        return tile != -1;
    }


    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!VampireSettings.Get.eventsEnabled) return false;
        int tile;
        if (!TryFindNewSiteTile(out tile)) return false;
        var site = SiteMaker.MakeSite(DefDatabase<SitePartDef>.GetNamed("ROMV_AncientVampireCrypt"), tile,
            Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROMV_LegendaryVampires")));
        site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(Faction.OfPlayer, 8, GenerateRewards());
        Find.WorldObjects.Add(site);
        SendStandardLetter(parms, null);
        return true;
    }

    private List<Thing> GenerateRewards(Faction alliedFaction = null)
    {
        var parms = default(ThingSetMakerParams);
        parms.techLevel = TechLevel.Medieval; //new TechLevel?(alliedFaction.def.techLevel);
        return ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
    }

//        private bool TryFindFactions(out Faction alliedFaction, out Faction enemyFaction)
//        {
//            if ((from x in Find.FactionManager.AllFactions
//                where !x.def.hidden && !x.defeated && !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) &&
//                      this.CommonHumanlikeEnemyFactionExists(Faction.OfPlayer, x) && !this.AnyQuestExistsFrom(x)
//                select x).TryRandomElement(out alliedFaction))
//            {
//                enemyFaction = this.CommonHumanlikeEnemyFaction(Faction.OfPlayer, alliedFaction);
//                return true;
//            }
//            alliedFaction = null;
//            enemyFaction = null;
//            return false;
//        }

    private bool AnyQuestExistsFrom(Faction faction)
    {
        var sites = Find.WorldObjects.Sites;
        for (var i = 0; i < sites.Count; i++)
        {
            var component = sites[i].GetComponent<DefeatAllEnemiesQuestComp>();
            if (component != null && component.Active && component.requestingFaction == faction) return true;
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