using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Dwarves
{
    public class SymbolResolver_CorpseMaker : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {

            var count = rp.hivesCount ?? 1;
            
            for (int i = 0; i < count; i++)
            {
                PawnKindDef kind = (Rand.Value > 0.3f) ? DwarfDefOf.LotRD_DwarfVillager : DwarfDefOf.LotRD_DwarfGuardMountain;
                Faction faction = rp.faction;
                PawnGenerationRequest request = new PawnGenerationRequest(kind, faction,
                    PawnGenerationContext.NonPlayer, BaseGen.globalSettings?.map?.Tile ?? Find.CurrentMap.Tile, false, false, false, false, true, true, 1f, false, true, false,
                    false, false, false, false, null, null, null, null, null, null, null);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                IntVec3 spawnLoc;
                
                //CellFinder.TryFindBestPawnStandCell(pawn, out spawnLoc);
                var map = BaseGen.globalSettings?.map ?? Find.CurrentMap;
                CellFinderLoose.TryGetRandomCellWith((x => x.IsValid && rp.rect.Contains(x) && x.GetEdifice(map) == null && x.GetFirstItem(map) == null), map, 250, out spawnLoc);
                GenSpawn.Spawn(pawn, spawnLoc, map);
                pawn.Kill(null);
                if (pawn?.Corpse is Corpse c && c.TryGetComp<CompRottable>() is CompRottable comp)
                {
                    c.Age += GenDate.TicksPerSeason * Rand.Range(8, 100);
                    Log.Message("Rotted corpse");
                    comp.RotProgress += 9999999;
                }
            }
        }


        private bool IsWallOrRock(Building b)
        {
            return b != null && (b.def == ThingDefOf.Wall || b.def.building.isNaturalRock);
        }

        private static HashSet<Room> visited = new HashSet<Room>();

        private static List<IntVec3> path = new List<IntVec3>();

        private static List<IntVec3> cellsInRandomOrder = new List<IntVec3>();
    }
}