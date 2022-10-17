using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Dwarves
{
    public class SymbolResolver_CorpseMaker : SymbolResolver
    {
        private static readonly HashSet<Room> visited = new HashSet<Room>();

        private static readonly List<IntVec3> path = new List<IntVec3>();

        private static readonly List<IntVec3> cellsInRandomOrder = new List<IntVec3>();

        public override void Resolve(ResolveParams rp)
        {
            var count = rp.hivesCount ?? 1;

            for (var i = 0; i < count; i++)
            {
                var kind = Rand.Value > 0.3f ? DwarfDefOf.LotRD_DwarfVillager : DwarfDefOf.LotRD_DwarfGuardMountain;
                var faction = rp.faction;
                var request = new PawnGenerationRequest(kind, faction,
                    PawnGenerationContext.NonPlayer, BaseGen.globalSettings?.map?.Tile ?? Find.CurrentMap.Tile, false,
                    false, false, false, true, 0f, false, false, true, false,
                    false);
                var pawn = PawnGenerator.GeneratePawn(request);

                //CellFinder.TryFindBestPawnStandCell(pawn, out spawnLoc);
                var map = BaseGen.globalSettings?.map ?? Find.CurrentMap;
                CellFinderLoose.TryGetRandomCellWith(
                    x => x.IsValid && rp.rect.Contains(x) && x.GetEdifice(map) == null && x.GetFirstItem(map) == null,
                    map, 250, out var spawnLoc);
                GenSpawn.Spawn(pawn, spawnLoc, map);
                pawn.Kill(null);
                if (pawn.Corpse is not { } c || c.TryGetComp<CompRottable>() is not { } comp)
                {
                    continue;
                }

                c.Age += GenDate.TicksPerSeason * Rand.Range(8, 100);
                Log.Message("Rotted corpse");
                comp.RotProgress += 9999999;
            }
        }


        private bool IsWallOrRock(Building b)
        {
            return b != null && (b.def == ThingDefOf.Wall || b.def.building.isNaturalRock);
        }
    }
}