using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Dwarves
{
    // Token: 0x02000A6F RID: 2671
    public class JobDriver_FlyToRearTarget : JobDriver
    {
        // Token: 0x06003B75 RID: 15221 RVA: 0x001BFBA3 File Offset: 0x001BDFA3
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.numMeleeAttacksMade, "numMeleeAttacksMade", 0, false);
        }

        // Token: 0x06003B76 RID: 15222 RVA: 0x001BFBC0 File Offset: 0x001BDFC0
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            IAttackTarget attackTarget = this.job.targetA.Thing as IAttackTarget;
            if (attackTarget != null)
            {
                this.pawn.Map.attackTargetReservationManager.Reserve(this.pawn, this.job, attackTarget);
            }
            return true;
        }

        // Token: 0x06003B77 RID: 15223 RVA: 0x001BFC0C File Offset: 0x001BE00C
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.DoAtomic(delegate
            {
                Pawn pawn = this.job.targetA.Thing as Pawn;
                if (pawn != null && pawn.Downed && this.pawn.mindState.duty != null && this.pawn.mindState.duty.attackDownedIfStarving && this.pawn.Starving())
                {
                    this.job.killIncappedTarget = true;
                }
            });
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
                if (this.pawn.meleeVerbs.TryMeleeAttack(thing, this.job.verbToUse, false))
                {
                    if (this.pawn.CurJob == null || this.pawn.jobs.curDriver != this)
                    {
                        return;
                    }
                    this.numMeleeAttacksMade++;
                    if (this.numMeleeAttacksMade >= this.job.maxNumMeleeAttacks)
                    {
                        base.EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);
            yield break;
        }

        // Token: 0x06003B78 RID: 15224 RVA: 0x001BFC30 File Offset: 0x001BE030
        public override void Notify_PatherFailed()
        {
            if (this.job.attackDoorIfTargetLost)
            {
                Thing thing;
                using (PawnPath pawnPath = base.Map.pathFinder.FindPath(this.pawn.Position, base.TargetA.Cell, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return;
                    }
                    IntVec3 intVec;
                    thing = pawnPath.FirstBlockingBuilding(out intVec, this.pawn);
                }
                if (thing != null)
                {
                    this.job.targetA = thing;
                    this.job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
                    this.job.expiryInterval = Rand.Range(2000, 4000);
                    return;
                }
            }
            base.Notify_PatherFailed();
        }

        // Token: 0x06003B79 RID: 15225 RVA: 0x001BFD18 File Offset: 0x001BE118
        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        // Token: 0x04002635 RID: 9781
        private int numMeleeAttacksMade;
    }
}
