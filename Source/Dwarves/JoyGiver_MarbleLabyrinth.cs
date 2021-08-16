using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    public class JoyGiver_MarbleLabyrinth : JoyGiver_Ingest
    {
        /// <summary>
        ///     Similar to how a pawn can be a social drinker or solitary relaxer in their room, hobbits can try and solve a puzzle
        ///     box.
        /// </summary>
        public override float GetChance(Pawn pawn)
        {
            //hobbits have twice the chance of opening the puzzle box
            if (pawn.def.defName == "LotRH_HobbitStandardRace")
            {
                return def.baseChance;
            }

            return def.baseChance * 0.5f;
        }

        //folks can puzzle during a party and when they're by themselves: hence the dual-job
        public override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobInternal(pawn, null);
        }


        public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot, float maxRadius = -1)
        {
            return TryGiveJobInternal(pawn,
                x => !x.Spawned || GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map));
        }

        private Job TryGiveJobInternal(Pawn pawn, Predicate<Thing> extraValidator)
        {
            var thing = BestIngestItem(pawn, extraValidator);
            if (thing != null)
            {
                return CreateIngestJob(thing, pawn);
            }

            return null;
        }

        protected override Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
        {
            //Find the puzzle box.
            bool predicate(Thing t)
            {
                return t.def == DefDatabase<ThingDef>.GetNamed("LotRD_DwarfMarbleLabyrinth") && pawn.CanReserve(t) &&
                       (extraValidator == null || extraValidator(t));
            }

            var searchSet = new List<Thing>();
            GetSearchSet(pawn, searchSet);
            var traverseParams = TraverseParms.For(pawn);

            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, PathEndMode.OnCell,
                traverseParams, 9999f, predicate);
        }

        protected override Job CreateIngestJob(Thing thing, Pawn pawn)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_SolvePuzzleBox"), thing)
            {
                count = 1
            };
        }
    }
}