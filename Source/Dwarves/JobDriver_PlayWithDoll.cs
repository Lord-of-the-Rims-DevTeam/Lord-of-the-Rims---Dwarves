using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    //Original code by Mehni, adopted by Jecrell
    public class JobDriver_PlayWithDoll : JobDriver
    {
        private const TargetIndex ToyInd = TargetIndex.A;
        private const TargetIndex joySpot = TargetIndex.B;

        private Thing PuzzleBox => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool yeaa)
        {
            return pawn.Reserve(PuzzleBox, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(ToyInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(ToyInd);
            yield return Toils_Ingest.PickupIngestible(ToyInd, pawn);
            yield return CarryToyToSpot(pawn, ToyInd);
            yield return Toils_Ingest.FindAdjacentEatSurface(joySpot, ToyInd);
            var playWithToy = new Toil
            {
                tickAction = WaitTickAction()
            };
            playWithToy.AddFinishAction(() =>
            {
                JoyUtility.TryGainRecRoomThought(pawn);
                TalkToDoll();
            });
            playWithToy.defaultCompleteMode = ToilCompleteMode.Delay;
            playWithToy.defaultDuration = job.def.joyDuration;
            playWithToy.handlingFacing = true;
            yield return playWithToy;
        }

        private void TalkToDoll()
        {
            var symbol = InteractionDefOf.Chitchat.GetSymbol();
            if (Rand.Value > 0.5f && pawn?.needs?.joy?.CurCategory <= JoyCategory.Low ||
                (pawn?.story?.traits.HasTrait(TraitDefOf.Abrasive) ?? false))
            {
                symbol = InteractionDefOf.Insult.GetSymbol();
            }

            MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, symbol);
//            float extraLuckFromQuality = base.TargetThingA.GetStatValue(StatDefOf.JoyGainFactor, true);
//            float extraLuckFromSmarts = pawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt;
//
//            float yourLuckyNumber = ((1f + extraLuckFromSmarts) * extraLuckFromQuality) / 100;
//
//            Log.Message("lucky number is: " + yourLuckyNumber.ToString());
//
//            if (Rand.Chance(yourLuckyNumber) || DebugSettings.godMode)
//            {
//                Thing reward = ThingMaker.MakeThing(ThingDefOf.Gold);
//                reward.stackCount = Rand.RangeInclusive(10, 50);
//                GenSpawn.Spawn(reward, pawn.Position, pawn.Map);
//                PuzzleBox.Destroy();
//                Letter letter = LetterMaker.MakeLetter("LotRD_PuzzleSolvedLabel".Translate(), "LotRD_PuzzleSolved".Translate(new object[] {
//                    pawn.Label,
//                    reward.Label,
//                }), LetterDefOf.PositiveEvent);
//                Find.LetterStack.ReceiveLetter(letter);
//            }
        }

        private Action WaitTickAction()
        {
            return delegate
            {
                pawn.rotationTracker.FaceCell(TargetB.Cell);
                pawn.GainComfortFromCellIfPossible();
                var extraJoyGainFactor = TargetThingA.GetStatValue(StatDefOf.JoyGainFactor);
                JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
            };
        }

        //slightly modified version of Toils_Ingest.CarryIngestibleToChewSpot
        private static Toil CarryToyToSpot(Pawn pawn, TargetIndex puzzleInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var intVec = IntVec3.Invalid;
                var thing2 = actor.CurJob.GetTarget(puzzleInd).Thing;

                bool baseChairValidator(Thing t)
                {
                    if (t.def.building == null || !t.def.building.isSittable)
                    {
                        return false;
                    }

                    if (t.IsForbidden(pawn))
                    {
                        return false;
                    }

                    if (!actor.CanReserve(t))
                    {
                        return false;
                    }

                    if (!t.IsSociallyProper(actor))
                    {
                        return false;
                    }

                    if (t.IsBurning())
                    {
                        return false;
                    }

                    if (t.HostileTo(pawn))
                    {
                        return false;
                    }

                    var result = false;
                    for (var i = 0; i < 4; i++)
                    {
                        var c = t.Position + GenAdj.CardinalDirections[i];
                        var edifice = c.GetEdifice(t.Map);
                        if (edifice == null || edifice.def.surfaceType != SurfaceType.Eat)
                        {
                            continue;
                        }

                        result = true;
                        break;
                    }

                    return result;
                }

                //if you can find a table with chair, great. If not, go to your room.

                var thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell,
                    TraverseParms.For(actor),
                    30f, //"chair search radius"
                    t => baseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None);

                if (thing == null)
                {
                    if (pawn.ownership?.OwnedRoom != null)
                    {
                        (from c in pawn.ownership.OwnedRoom.Cells
                            where c.Standable(pawn.Map) && !c.IsForbidden(pawn) &&
                                  pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None)
                            select c).TryRandomElement(out intVec);
                    }
                }

                if (thing != null)
                {
                    intVec = thing.Position;
                    actor.Reserve(thing, actor.CurJob);
                }

                if (intVec == IntVec3.Invalid)
                {
                    intVec = RCellFinder.SpotToChewStandingNear(pawn, thing2);
                }

                actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, intVec);
                actor.pather.StartPath(intVec, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        //public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
        //{
        //    return base.ModifyCarriedThingDrawPos(ref drawPos, ref behind, ref flip);
        //}
    }
}