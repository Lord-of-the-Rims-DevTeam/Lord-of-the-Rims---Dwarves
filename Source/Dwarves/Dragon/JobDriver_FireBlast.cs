using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Dwarves
{
    // Token: 0x02000A6F RID: 2671
    public class JobDriver_FireBlast : JobDriver
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
            //
            yield return new Toil()
            {
                initAction = () =>
                {
                    Pawn actor = this.GetActor();
                    Vector3 drawPos = actor.DrawPos;
                    var shootLine = new ShootLine(actor.PositionHeld, pawn.PositionHeld);
                    Projectile projectile2 = (Projectile)GenSpawn.Spawn(ThingDef.Named("LotRD_Dragonfireblast"), shootLine.Source, actor.Map, WipeMode.Vanish);
                    if (Rand.Value > 0.9f)
                    {
                        //Log.Message("Miss");
                        int num = Rand.Range(2, 3);
                        int max = GenRadial.NumCellsInRadius(num);
                        int num2 = Rand.Range(0, max);
                        if (num2 > 0)
                        {
                            IntVec3 c = pawn.PositionHeld + GenRadial.RadialPattern[num2];
                            ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                            if (Rand.Chance(0.5f))
                            {
                                projectileHitFlags = ProjectileHitFlags.All;
                            }
                            projectile2.Launch(actor, drawPos, c, pawn, projectileHitFlags, null, null);
                            return;
                        }
                    }
                    var covers = CoverUtility.CalculateCoverGiverSet(job.targetA, actor.Position, actor.Map);
                    covers.TryRandomElementByWeight((CoverInfo c) => c.BlockChance, out CoverInfo coverInfo);
                    ThingDef targetCoverDef = (coverInfo.Thing == null) ? null : coverInfo.Thing.def;

                    if (pawn != null)
                    {
                        projectile2.Launch(actor, drawPos + new Vector3(0, 0, 3), pawn, pawn, ProjectileHitFlags.All, null, targetCoverDef);
                    }
                    else
                    {
                        projectile2.Launch(actor, drawPos + new Vector3(0, 0, 3), shootLine.Dest, pawn, ProjectileHitFlags.All, null, targetCoverDef);
                    }

                    // Sends a wave outwards of flames
                    Vector2 behindVector = pawn.Rotation.AsVector2;
                    var behind = new IntVec3((int)behindVector.x, 0, (int)behindVector.y);
                    int rand = Rand.Range(6, 12);
                    //Log.Message("Pawn position: " + pawn.PositionHeld.x.ToString() + " " + pawn.PositionHeld.z.ToString());
                    for (int i = 0; i < rand; i++)
                    {
                        IntVec3 adjustment = new IntVec3((int)pawn.PositionHeld.x + (int)behind.x + i, pawn.PositionHeld.y, (int)pawn.PositionHeld.z + (int)behind.z + i);
                        //Log.Message("Fire position: " + adjustment.x.ToString() + " " + adjustment.z.ToString());
                        if (!adjustment.IsValid) continue;
                        Projectile projectileNew = (Projectile)GenSpawn.Spawn(ThingDef.Named("LotRD_Dragonfireblast"), shootLine.Source, actor.Map, WipeMode.Vanish);
                        var headoffset = (actor.Rotation == Rot4.West) ? -3.75f : 3.75f;
                        projectileNew.Launch(actor, drawPos + new Vector3(0, 0, 1.5f) + new Vector3(actor.Rotation.AsVector2.x + headoffset, 0, actor.Rotation.AsVector2.y), adjustment, pawn, ProjectileHitFlags.All, null, targetCoverDef);
                    }
                }
            };
            //
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
