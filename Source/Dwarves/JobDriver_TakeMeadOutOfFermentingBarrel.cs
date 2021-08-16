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

        private Building_FermentingMeadBarrel MeadBarrel =>
            (Building_FermentingMeadBarrel) job.GetTarget(BarrelInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa)
        {
            return pawn.Reserve(MeadBarrel, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(BarrelInd);
            this.FailOnBurningImmobile(BarrelInd);
            yield return Toils_Goto.GotoThing(BarrelInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(BarrelInd)
                .FailOnCannotTouch(BarrelInd, PathEndMode.Touch).FailOn(() => !MeadBarrel.Fermented)
                .WithProgressBarToilDelay(BarrelInd);
            yield return new Toil
            {
                initAction = delegate
                {
                    var thing = MeadBarrel.TakeOutMead();
                    GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near);
                    var currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing,
                        pawn, Map, currentPriority, pawn.Faction, out var c))
                    {
                        job.SetTarget(TargetIndex.C, c);
                        job.SetTarget(TargetIndex.B, thing);
                        job.count = thing.stackCount;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(BeerToHaulInd);
            yield return Toils_Reserve.Reserve(StorageCellInd);
            yield return Toils_Goto.GotoThing(BeerToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(BeerToHaulInd);
            var carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
        }
    }
}