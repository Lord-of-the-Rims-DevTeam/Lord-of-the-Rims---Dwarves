using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    public class WorkGiver_TakeMeadOutOfFermentingBarrel : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDef.Named("LotRD_FermentingBarrel"));

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_FermentingMeadBarrel {Fermented: true}))
            {
                return false;
            }

            if (t.IsBurning())
            {
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }

            LocalTargetInfo target = t;
            return pawn.CanReserve(target, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_TakeMeadOutOfFermentingBarrel"), t);
        }
    }
}