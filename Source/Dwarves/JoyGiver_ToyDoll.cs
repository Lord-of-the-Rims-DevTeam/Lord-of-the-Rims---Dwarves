using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    
    //Original code by Mehni, adapted by Jecrell
    public class JoyGiver_ToyDoll : JoyGiver_Ingest
    {
        /// <summary>
        /// Similar to how a pawn can be a social drinker or solitary relaxer in their room, some will prefer toys.
        /// </summary>

        public override float GetChance(Pawn pawn)
        {
            //younger colonists have a better chance
            if (pawn.ageTracker.AgeBiologicalYears < 13) return def.baseChance * 3;
            if (pawn.ageTracker.AgeBiologicalYears < 19) return def.baseChance;
            if (pawn.ageTracker.AgeBiologicalYears < 20) return def.baseChance * 0.5f;
            if (pawn.ageTracker.AgeBiologicalYears < 40) return def.baseChance * 0.25f;
            return def.baseChance * 0.15f;
        }

        //folks can play with dolls during a party and when they're by themselves: hence the dual-job
        public override Job TryGiveJob(Pawn pawn)
        {
            return this.TryGiveJobInternal(pawn, null);
        }

        public override Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
        {
            return this.TryGiveJobInternal(pawn, (Thing x) => !x.Spawned || PartyUtility.InPartyArea(x.Position, partySpot, pawn.Map));
        }

        private Job TryGiveJobInternal(Pawn pawn, Predicate<Thing> extraValidator)
        {
            Thing thing = this.BestIngestItem(pawn, extraValidator);
            if (thing != null)
            {
                return this.CreateIngestJob(thing, pawn);
            }
            return null;
        }

        protected override Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
        {
            //Find a doll
            Predicate<Thing> predicate = (Thing t) => (t.def == DefDatabase<ThingDef>.GetNamed("LotRD_DwarfElfToy")) && pawn.CanReserve(t) && (extraValidator == null || extraValidator(t));
            List<Thing> searchSet = this.GetSearchSet(pawn);
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);

            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, PathEndMode.OnCell, traverseParams, 9999f, predicate, null);
        }

        protected override Job CreateIngestJob(Thing thing, Pawn pawn)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_PlayWithDoll"), thing)
            {
                count = 1
            };
        }
    }
}
