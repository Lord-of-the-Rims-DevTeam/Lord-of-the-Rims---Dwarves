using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Dwarves
{
    public class IncidentWorker_AncientDwarvenStronghold : IncidentWorker
    {
        private const float RelationsImprovement = 8f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && !AnyExistingStrongholds() &&
                   TryFindNewSiteTile(out _);
        }

        public static bool AnyExistingStrongholds()
        {
            return Find.World?.worldObjects?.Sites?.FirstOrDefault(x =>
                x.parts.FirstOrDefault(y => y.def.defName == "LotRD_AncientDwarvenStronghold") != null) != null;
        }


        //The same as TileFinder.TryFindNewSiteTile EXCEPT
        // this one finds locations with caves
        public static bool TryFindNewSiteTile(out int tile, int minDist = 8, int maxDist = 30,
            bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
        {
            int findTile(int root)
            {
                bool validator(int x)
                {
                    return !Find.WorldObjects.AnyWorldObjectAt(x) && Find.World.HasCaves(x) &&
                           TileFinder.IsValidTileForNewSettlement(x);
                }

                var tileFinderMode = TileFinderMode.Near;
                if (!preferCloserTiles)
                {
                    tileFinderMode = TileFinderMode.Furthest;
                }

                if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out var result,
                    validator,
                    false, tileFinderMode))
                {
                    return result;
                }

                return -1;
            }

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
            if (!TryFindNewSiteTile(out var tile))
            {
                return false;
            }

            //            Faction faction;
            //            Faction faction2;
            //            int num;
            //            this.TryFindFactions(out faction, out faction2) &&
            //                TileFinder.TryFindNewSiteTile(out num, 8, 30, false, true, -1)
            //var pirate = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Pirate"));
            var site = SiteMaker.MakeSite(DwarfDefOf.LotRD_AncientDwarvenStronghold, tile,
                Find.FactionManager.FirstFactionOfDef(DwarfDefOf.LotRD_MonsterFaction));
            site.sitePartsKnown = true;
            site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(Faction.OfPlayer, 8, GenerateRewards());
            Find.WorldObjects.Add(site);
            SendStandardLetter(parms, site);
            return true;
        }

        private List<Thing> GenerateRewards()
        {
            ThingSetMakerParams parms = default;
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
            foreach (var site in sites)
            {
                var component = site.GetComponent<DefeatAllEnemiesQuestComp>();
                if (component is {Active: true} && component.requestingFaction == faction)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
        {
            return CommonHumanlikeEnemyFaction(f1, f2) != null;
        }

        private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
        {
            if ((from x in Find.FactionManager.AllFactions
                where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) &&
                      x.HostileTo(f2)
                select x).TryRandomElement(out var result))
            {
                return result;
            }

            return null;
        }
    }
}