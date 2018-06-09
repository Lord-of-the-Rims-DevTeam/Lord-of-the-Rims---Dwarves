using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    public class JobDriver_TakeMeadOutOfFermentingBarrel : JobDriver
    {
        
        private const TargetIndex BarrelInd = TargetIndex.A;
        private const TargetIndex BeerToHaulInd = TargetIndex.B;
        private const TargetIndex StorageCellInd = TargetIndex.C;
        private const int Duration = 200;
        
        protected Building_FermentingMeadBarrel MeadBarrel => (Building_FermentingMeadBarrel) job.GetTarget(BarrelInd).Thing;
        public override bool TryMakePreToilReservations() => pawn.Reserve(MeadBarrel, job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(BarrelInd);
            this.FailOnBurningImmobile(BarrelInd);
            yield return Toils_Goto.GotoThing(BarrelInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(BarrelInd)
                .FailOnCannotTouch(BarrelInd, PathEndMode.Touch).FailOn(() => !MeadBarrel.Fermented)
                .WithProgressBarToilDelay(BarrelInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing = MeadBarrel.TakeOutMead();
                    GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near, null);
                    StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(thing.Position, thing);
                    IntVec3 c;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, currentPriority, pawn.Faction,
                        out c, true))
                    {
                        job.SetTarget(StorageCellInd, c);
                        job.SetTarget(BeerToHaulInd, thing);
                        job.count = thing.stackCount;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(BeerToHaulInd, 1, -1, null);
            yield return Toils_Reserve.Reserve(StorageCellInd, 1, -1, null);
            yield return Toils_Goto.GotoThing(BeerToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(BeerToHaulInd, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
            yield break;
        }

    }
}