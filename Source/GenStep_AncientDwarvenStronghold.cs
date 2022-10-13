using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace Dwarves
{
    public class GenStep_AncientDwarvenStronghold : GenStep
    {
        private const int Size = 16;

        private static readonly List<CellRect> possibleRects = new List<CellRect>();

        public override int SeedPart { get; }

        public static CellRect TopHalf(CellRect rect)
        {
            return new CellRect(rect.minX, rect.minZ, rect.Width, (int) (rect.Height / 2f));
        }


        private static CellRect BottomHalf(CellRect rect)
        {
            return new CellRect(rect.minX, rect.minZ + (int) (rect.Height / 2f), rect.Width, (int) (rect.Height / 2f));
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect rectToDefend))
            {
                rectToDefend = CellRect.SingleCell(map.Center);
            }

            var floorDef =
                TerrainDef.Named(
                    "FlagstoneGranite"); // rp.floorDef ?? BaseGenUtility.CorrespondingTerrainDef(thingDef, true);

            var faction = Find.FactionManager.FirstFactionOfDef(DwarfDefOf.LotRD_MonsterFaction);
            ResolveParams rp = default;
            rp.rect = GetOutpostRect(rectToDefend, map);
            rp.rect = rp.rect.ExpandedBy(30);
            Log.Message(rp.rect.minX + " " + rp.rect.minZ);
            rp.faction = faction;
            rp.floorDef = floorDef;
            rp.wallStuff = ThingDefOf.BlocksGranite;
            rp.pathwayFloorDef = TerrainDef.Named("SilverTile");
            rp.edgeDefenseWidth = 2;
            rp.edgeDefenseTurretsCount = 0; // new int?(Rand.RangeInclusive(0, 1));
            rp.edgeDefenseMortarsCount = 0;
            rp.settlementPawnGroupPoints = 0.4f;
            rp.chanceToSkipWallBlock = 0.1f;
            BaseGen.globalSettings.map = map;
//			BaseGen.globalSettings.minBuildings = 1;
//			BaseGen.globalSettings.minBarracks = 1;
//			BaseGen.symbolStack.Push("factionBase", resolveParams);
            var singlePawnLord = rp.singlePawnLord ??
                                 LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(rp.rect.CenterCell), map);


            BaseGen.symbolStack.Push("outdoorLighting", rp);
            var resolveParams3 = rp;
            var pawn = PawnGenerator.GeneratePawn(DwarfDefOf.LotRD_DragonFireDrake, faction);
            resolveParams3.singlePawnToSpawn = pawn;
            resolveParams3.rect = new CellRect(rp.rect.CenterCell.x, rp.rect.CenterCell.z, 5, 5);
            resolveParams3.singlePawnLord = singlePawnLord;
            BaseGen.symbolStack.Push("pawn", resolveParams3);

            var thingDef =
                ThingDefOf.BlocksGranite; //rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, false);

            //Dragon Room
            var dragonRoomParams = default(ResolveParams);
            var dragonRoomRect = rp.rect.ContractedBy(Rand.Range(20, 25));

            //Rubble strewn about
            var rubbleEverywhere = rp;
            rubbleEverywhere.rect = rp.rect.ContractedBy(1);
            rubbleEverywhere.filthDef = ThingDefOf.Filth_RubbleBuilding;
            rubbleEverywhere.chanceToSkipFloor = 0.25f;
            BaseGen.symbolStack.Push("filthMaker", rubbleEverywhere);

            //Corpses strewn about
            var corpsesEverywhere = rp;
            corpsesEverywhere.rect = dragonRoomRect.ContractedBy(1);
            corpsesEverywhere.hivesCount = Rand.Range(12, 18);
            corpsesEverywhere.faction = Find.FactionManager.RandomEnemyFaction();
            BaseGen.symbolStack.Push("corpseMaker", corpsesEverywhere);

            //Corpses strewn about (2)
            var corpsesEverywhereTwo = rp;
            corpsesEverywhereTwo.rect = rp.rect.ContractedBy(1);
            corpsesEverywhereTwo.hivesCount = Rand.Range(12, 18);
            corpsesEverywhereTwo.faction = Find.FactionManager.RandomEnemyFaction();
            BaseGen.symbolStack.Push("corpseMaker", corpsesEverywhereTwo);

            //Just place a dwaven throne.
            var throneArea = rp;
            throneArea.rect = CellRect.SingleCell(BottomHalf(dragonRoomRect).CenterCell);
            throneArea.singleThingDef = ThingDef.Named("LotRD_DwarvenThrone");
            throneArea.singleThingStuff = ThingDefOf.BlocksGranite;
            BaseGen.symbolStack.Push("thing", throneArea);

            //Gold coins strewn about
            var coinsEverywhere = rp;
            coinsEverywhere.rect = BottomHalf(dragonRoomRect).ContractedBy(4);
            coinsEverywhere.filthDef = ThingDef.Named("LotRD_Filth_GoldCoins");
            coinsEverywhere.chanceToSkipFloor = 0f;
            coinsEverywhere.filthDensity = new FloatRange(1, 5);
            coinsEverywhere.streetHorizontal = true;
            BaseGen.symbolStack.Push("filthMaker", coinsEverywhere);


            //Dragon horde
            var dragonHorde = rp;
            dragonHorde.rect =
                BottomHalf(dragonRoomRect)
                    .ContractedBy(
                        5); //new CellRect(dragonRoomRect.minX, (int)(dragonRoomRect.minZ + dragonRoomRect.Height / 2f), dragonRoomRect.Width, (int)(dragonRoomRect.Height / 2f));
            dragonHorde.thingSetMakerDef = DwarfDefOf.LotRD_Treasure;
            var newParamsForItemGen = new ThingSetMakerParams
            {
                countRange = new IntRange(15, 20),
                maxThingMarketValue = Rand.Range(10000, 15000)
            };
            dragonHorde.thingSetMakerParams = newParamsForItemGen;
            dragonHorde.singleThingStackCount = 250;
            BaseGen.symbolStack.Push("stockpile", dragonHorde);
            //


            dragonRoomParams.rect = dragonRoomRect;
            BaseGen.symbolStack.Push("roof", dragonRoomParams);
            var dragonDoorParams = dragonRoomParams;
            dragonDoorParams.rect = dragonRoomRect;
            BaseGen.symbolStack.Push("doors", dragonDoorParams);
            dragonDoorParams.rect = dragonRoomRect;
            var dragonRoomWalls = dragonRoomParams;
            dragonRoomWalls.rect = dragonRoomRect;
            dragonRoomWalls.wallStuff = thingDef;
            dragonRoomWalls.floorDef = floorDef;
            dragonRoomWalls.chanceToSkipWallBlock = 0.25f;
            BaseGen.symbolStack.Push("edgeWalls", dragonRoomWalls);
            BaseGen.symbolStack.Push("clear", dragonRoomParams);

            //
            // Abandoned Dwarven Mountain Fortress
            var num = 0;
            var edgeDefenseWidth = rp.edgeDefenseWidth;
            var num2 = rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = num2 >= 1f ? GenMath.RoundRandom(num2) : 0;
            var traverseParms = TraverseParms.For(TraverseMode.PassDoors);
            var abandonedBase = rp;
            abandonedBase.rect = rp.rect;
            abandonedBase.faction = faction;
            var resolveParams4 = abandonedBase;
            resolveParams4.rect = rp.rect.ContractedBy(num + 10);
            resolveParams4.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);
            var resolveParams5 = abandonedBase;
            resolveParams5.rect = rp.rect.ContractedBy(num + 10);
            resolveParams5.faction = faction;
            resolveParams5.wallStuff = thingDef;
            resolveParams5.floorDef = floorDef;
            BaseGen.symbolStack.Push("basePart_outdoors", resolveParams5);
            // 

            //Four Corners -- Four Guardtowers
            for (var i = 0; i < 3; i++)
            {
                var towerCorner = rp.rect.Corners.ElementAt(i);
                var towerRect = new CellRect(towerCorner.x, towerCorner.z, 10, 10);
                var towerParams = default(ResolveParams);
                towerParams.rect = towerRect;
                BaseGen.symbolStack.Push("roof", towerRect);
                var towerDoorParams = towerParams;
                towerDoorParams.rect = towerRect;
                BaseGen.symbolStack.Push("doors", towerDoorParams);
                towerDoorParams.rect = towerRect;
                var towerWallParams = towerParams;
                towerWallParams.rect = towerRect;
                towerWallParams.wallStuff = thingDef;
                towerWallParams.floorDef = floorDef;
                towerWallParams.chanceToSkipWallBlock = 0.1f;
                BaseGen.symbolStack.Push("edgeWalls", towerWallParams);
                BaseGen.symbolStack.Push("clear", towerParams);
            }


            BaseGen.symbolStack.Push("roof", rp);
            var doorParams = rp;
            BaseGen.symbolStack.Push("doors", doorParams);
            var resolveParams = rp;
            resolveParams.wallStuff = thingDef;
            resolveParams.chanceToSkipWallBlock = 0.1f;
            BaseGen.symbolStack.Push("edgeWalls", resolveParams);
            var thickwallParams = rp;
            thickwallParams.rect = thickwallParams.rect.ContractedBy(1);
            thickwallParams.wallStuff = thingDef;
            thickwallParams.chanceToSkipWallBlock = 0.3f;
            BaseGen.symbolStack.Push("edgeWalls", thickwallParams);
            var dthickwallParams = thickwallParams;
            dthickwallParams.rect = dthickwallParams.rect.ContractedBy(1);
            dthickwallParams.chanceToSkipWallBlock = 0.6f;
            BaseGen.symbolStack.Push("edgeWalls", dthickwallParams);
            var resolveParams2 = rp;
            resolveParams2.floorDef = floorDef;
            BaseGen.symbolStack.Push("floor", resolveParams2);
            BaseGen.symbolStack.Push("clear", rp);
            if (rp.addRoomCenterToRootsToUnfog != null && rp.addRoomCenterToRootsToUnfog.Value &&
                Current.ProgramState == ProgramState.MapInitializing)
            {
                MapGenerator.rootsToUnfog.Add(rp.rect.CenterCell);
            }

            Log.Message("generated dragon lair");
            BaseGen.Generate();
        }

        private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
        {
            possibleRects.Add(new CellRect(rectToDefend.minX - 1 - 16, rectToDefend.CenterCell.z - 8, 16, 16));
            possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - 8, 16, 16));
            possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.minZ - 1 - 16, 16, 16));
            possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.maxZ + 1, 16, 16));
            var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
            possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
            if (possibleRects.Any())
            {
                return possibleRects.RandomElement();
            }

            return rectToDefend;
        }
    }
}