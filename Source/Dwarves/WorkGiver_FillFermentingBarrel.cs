using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    public class WorkGiver_FillFermentingBarrel : WorkGiver_Scanner
    {
        private static string TemperatureTrans;

        private static string NoWortTrans;

        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDef.Named("LotRD_FermentingBarrel"));

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public static void Reset()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoWortTrans = "LotRD_NoWortMead".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_FermentingMeadBarrel Building_FermentingMeadBarrel) ||
                Building_FermentingMeadBarrel.Fermented || Building_FermentingMeadBarrel.SpaceLeftForWort <= 0)
            {
                return false;
            }

            var ambientTemperature = Building_FermentingMeadBarrel.AmbientTemperature;
            var compProperties =
                Building_FermentingMeadBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f ||
                ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(TemperatureTrans);
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }

            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }

            if (FindWort(pawn) != null)
            {
                return !t.IsBurning();
            }

            JobFailReason.Is(NoWortTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var t2 = FindWort(pawn);
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_FillFermentingBarrel"), t, t2);
        }

        private Thing FindWort(Pawn pawn)
        {
            bool Predicate(Thing x)
            {
                return !x.IsForbidden(pawn) && pawn.CanReserve(x);
            }

            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(ThingDef.Named("LotRD_MeadWort"));
            const PathEndMode peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn);
            Predicate<Thing> validator = Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator);
        }
    }
}