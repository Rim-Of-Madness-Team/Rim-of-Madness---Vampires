using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire
{
    public class IncidentWorker_AncientVampireCrypt : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction faction;
            Faction faction2;
            int num;
            return base.CanFireNowSub(parms) &&
                   TryFindNewSiteTile(out num, 8, 30, false, true, -1);
        }


        //The same as TileFinder.TryFindNewSiteTile EXCEPT
        // this one finds locations with caves
        public static bool TryFindNewSiteTile(out int tile, int minDist = 8, int maxDist = 30,
            bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
        {
            Func<int, int> findTile = delegate(int root)
            {
                int minDist2 = minDist;
                int maxDist2 = maxDist;
                Predicate<int> validator = (int x) =>
                    !Find.WorldObjects.AnyWorldObjectAt(x) &&
                    Find.World.HasCaves(x) && TileFinder.IsValidTileForNewSettlement(x, null);
                bool preferCloserTiles2 = preferCloserTiles;
                int result;
                if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator,
                    false, preferCloserTiles2))
                {
                    return result;
                }
                return -1;
            };
            int arg;
            if (nearThisTile != -1)
            {
                arg = nearThisTile;
            }
            else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, (int x) => findTile(x) != -1))
            {
                tile = -1;
                return false;
            }
            tile = findTile(arg);
            return tile != -1;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int tile;
            if (!TryFindNewSiteTile(out tile, 8, 30, false, true, -1))
            {
                return false;
            }
            Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, DefDatabase<SitePartDef>.GetNamed("ROMV_AncientVampireCrypt"), tile,
                Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROMV_LegendaryVampires")));
            site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(Faction.OfPlayer, 8, GenerateRewards());
            Find.WorldObjects.Add(site);
            base.SendStandardLetter(site);
            return true;
        }

        private List<Thing> GenerateRewards(Faction alliedFaction = null)
        {
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.techLevel = TechLevel.Medieval; //new TechLevel?(alliedFaction.def.techLevel);
            return ThingSetMakerDefOf.Reward_StandardByDropPod.root.Generate(parms);
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
            List<Site> sites = Find.WorldObjects.Sites;
            for (int i = 0; i < sites.Count; i++)
            {
                DefeatAllEnemiesQuestComp component = sites[i].GetComponent<DefeatAllEnemiesQuestComp>();
                if (component != null && component.Active && component.requestingFaction == faction)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
        {
            return this.CommonHumanlikeEnemyFaction(f1, f2) != null;
        }

        private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
        {
            Faction result;
            if ((from x in Find.FactionManager.AllFactions
                where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) &&
                      x.HostileTo(f2)
                select x).TryRandomElement(out result))
            {
                return result;
            }
            return null;
        }

        private const float RelationsImprovement = 8f;
    }
}