using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Dwarves
{
    public static partial class HarmonyDwarves
    {
        private const string WaxModPackName = "Call of Cthulhu - Industrial Age";

        public static void HarmonyFactions(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                new HarmonyMethod(typeof(HarmonyDwarves).GetMethod("RandomSettlementTileFor_PreFix")));
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_DyingBecauseExposedToLight"), null,
                new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_DyingBecauseExposedToLight_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_GrowthRate"), null,
                new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_GrowthRate_PostFix)));

            //AdjustFurnitureSettings();
            //AdjustWaxSettings();
        }

        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            if (faction?.def?.defName == "LotRD_HillClans")
            {
                __result = RandomSettlementTileFor_HillDwarves(faction);
                return false;
            }
            if (faction?.def?.defName != "LotRD_MountainKingdom")
            {
                return true;
            }
            __result = RandomSettlementTileFor_MountainDwarves(faction);
            return false;
        }

        private static int RandomSettlementTileFor_MountainDwarves(Faction faction)
        {
            for (var i = 0; i < 500; i++)
            {
                if (!(from _ in Enumerable.Range(0, 100)
                      select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                  {
                      var tile = Find.WorldGrid[x];
                      if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                      {
                          return 0f;
                      }

                      if (tile.hilliness == Hilliness.Mountainous)
                      {
                          return 1000f;
                      }

                      return 0f; //tile.biome.settlementSelectionWeight;
                  }, out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(num))
                {
                    return num;
                }
            }

            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        private static int RandomSettlementTileFor_HillDwarves(Faction faction)
        {
            for (var i = 0; i < 500; i++)
            {
                if (!(from _ in Enumerable.Range(0, 100)
                      select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                  {
                      var tile = Find.WorldGrid[x];
                      if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                      {
                          return 0f;
                      }

                      if (tile.hilliness == Hilliness.LargeHills)
                      {
                          return 1000f;
                      }

                      return 0f; //tile.biome.settlementSelectionWeight;
                  }, out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(num))
                {
                    return num;
                }
            }

            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }


    }
}