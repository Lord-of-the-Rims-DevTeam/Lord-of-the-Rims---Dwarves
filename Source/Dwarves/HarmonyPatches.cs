using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Dwarves
{
    [StaticConstructorOnStartup]
    public static class HarmonyFactions
    {
        private const string WaxModPackName = "Call of Cthulhu - Industrial Age";


        static HarmonyFactions()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.lotr.dwarves");

            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                new HarmonyMethod(typeof(HarmonyFactions).GetMethod("RandomSettlementTileFor_PreFix")), null);
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_DyingBecauseExposedToLight"), null,
                new HarmonyMethod(typeof(HarmonyFactions), nameof(get_DyingBecauseExposedToLight_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_GrowthRate"), null,
                new HarmonyMethod(typeof(HarmonyFactions), nameof(get_GrowthRate_PostFix)), null);


            //AdjustFurnitureSettings();
            //AdjustWaxSettings();
        }

        private static void AdjustWaxSettings()
        {
            var candelabra = ThingDef.Named("LotRD_DwarvenCandelabra");
            
            if (LoadedModManager.RunningMods.FirstOrDefault(x => x.Name == WaxModPackName) != null)
            {
                var WaxDef = ThingDef.Named("Jecrell_Wax");
                if (candelabra.GetCompProperties<CompProperties_Refuelable>() is CompProperties_Refuelable rf)
                {
                    //Log.Message("Set wax for Dwarven Candelabra");
                    rf.fuelFilter.SetAllow(WaxDef, true);
                    rf.fuelFilter.SetAllow(ThingDefOf.WoodLog, false);
                }
            }
        }

        private static void AdjustFurnitureSettings()
        {
            var endTable = ThingDef.Named("LotRD_EndTable");
            
            HashSet<ThingDef> bedDefs;
            bedDefs = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsBed));
            if (bedDefs?.Count > 0)
            {
                foreach (var def in bedDefs)
                {
                    if (def.GetCompProperties<CompProperties_AffectedByFacilities>() is
                        CompProperties_AffectedByFacilities cp)
                    {
                        if (!cp?.linkableFacilities?.Contains(endTable) ?? false)
                        {
                            //Log.Message("Added end table to " + def.label);
                            cp.linkableFacilities.Add(endTable);
                        }
                    }
                }
            }
        }

        //Reduces growth rate to one fourth
        public static void get_GrowthRate_PostFix(Plant __instance, ref float __result)
        {
            if (__instance.def.defName == "LotRD_PlantEarthBreadRoot" && __instance.Map.glowGrid.GameGlowAt(__instance.Position, true) > 0f)
            {
                __result *= 0.5f;
            }
        }
        
        //While Earthbread prefers caves, it does not die when exposed to sunlight
        public static void get_DyingBecauseExposedToLight_PostFix(Plant __instance, ref bool __result)
        {
            if (__instance.def.defName == "LotRD_PlantEarthBreadRoot") __result = false;
        }

        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            //if (faction.def.defName == "TheAgency")
            //{
            //    __result = RandomSettlementTileFor_TheAgency(faction);
            //    return false;
            //}
            if (faction?.def?.defName == "LotRD_HillClans")
            {
                __result = RandomSettlementTileFor_HillDwarves(faction);
                return false;
            }
            if (faction?.def?.defName == "LotRD_MountainKingdom")
            {
                __result = RandomSettlementTileFor_MountainDwarves(faction);
                return false;
            }
            return true;
        }


        public static int RandomSettlementTileFor_HighElves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    if (tile.Rivers != null && tile.Rivers.Count > 0)
                        return 1000f;
                    return 0f; //tile.biome.settlementSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }


        public static int RandomSettlementTileFor_MountainDwarves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    if (tile.hilliness == Hilliness.Mountainous)
                        return 1000f;
                    return 0f; //tile.biome.settlementSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        public static int RandomSettlementTileFor_HillDwarves(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    List<int> neighbors = new List<int>();
                    if (tile.hilliness == Hilliness.LargeHills)
                        return 1000f;
                    return 0f; //tile.biome.settlementSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }
    }
}