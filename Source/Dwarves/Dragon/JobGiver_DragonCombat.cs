using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Dwarves
{
    // Token: 0x020000BA RID: 186
    public class JobGiver_DragonCombat : ThinkNode_JobGiver
    {
        // Token: 0x0600046C RID: 1132
        protected bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = (verb.verbProps.range > 5f)
            }, out dest);
        }

        // Token: 0x0600046D RID: 1133 RVA: 0x0002C314 File Offset: 0x0002A714
        protected virtual float GetFlagRadius(Pawn pawn)
        {
            return 999999f;
        }

        // Token: 0x0600046E RID: 1134 RVA: 0x0002C31B File Offset: 0x0002A71B
        protected virtual IntVec3 GetFlagPosition(Pawn pawn)
        {
            return IntVec3.Invalid;
        }

        // Token: 0x0600046F RID: 1135 RVA: 0x0002C322 File Offset: 0x0002A722
        protected virtual bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return true;
        }

        // Token: 0x06000470 RID: 1136 RVA: 0x0002C328 File Offset: 0x0002A728
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_DragonCombat JobGiver_DragonCombat = (JobGiver_DragonCombat)base.DeepCopy(resolve);
            JobGiver_DragonCombat.targetAcquireRadius = this.targetAcquireRadius;
            JobGiver_DragonCombat.targetKeepRadius = this.targetKeepRadius;
            JobGiver_DragonCombat.needLOSToAcquireNonPawnTargets = this.needLOSToAcquireNonPawnTargets;
            JobGiver_DragonCombat.chaseTarget = this.chaseTarget;
            return JobGiver_DragonCombat;
        }

        // Token: 0x06000471 RID: 1137 RVA: 0x0002C374 File Offset: 0x0002A774
        protected override Job TryGiveJob(Pawn pawn)
        {
            this.UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
            {
                return null;
            }
            float rand = Rand.Value;
            //if (rand > 0.90) //Wing buffet, 10%
            //{
            //    return this.WingBuffet(enemyTarget);
            //}
            if (rand > 0.60) //Fireblast, 10%
            {
                return this.FireBlast(enemyTarget);
            }
            //else if (rand > 0.70) //Fly to rear, 10%
            //{
            //    return this.FlyToRearTarget(enemyTarget);
            //}
            return this.MeleeAttackJob(enemyTarget);
            
            //bool flag = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
            //bool flag2 = pawn.Position.Standable(pawn.Map);
            //bool flag3 = verb.CanHitTarget(enemyTarget);
            //bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
            //if ((flag && flag2 && flag3) || (flag4 && flag3))
            //{
            //    return new Job(JobDefOf.Wait_Combat, JobGiver_DragonCombat.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            //}
            //IntVec3 intVec;
            //if (!this.TryFindShootingPosition(pawn, out intVec))
            //{
            //    return null;
            //}
            //if (intVec == pawn.Position)
            //{
            //    return new Job(JobDefOf.Wait_Combat, JobGiver_DragonCombat.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            //}
            //return new Job(JobDefOf.Goto, intVec)
            //{
            //    expiryInterval = JobGiver_DragonCombat.ExpiryInterval_ShooterSucceeded.RandomInRange,
            //    checkOverrideOnExpire = true
            //};
        }

        // Token: 0x06000472 RID: 1138 RVA: 0x0002C4DC File Offset: 0x0002A8DC
        protected virtual Job MeleeAttackJob(Thing enemyTarget)
        {
            return new Job(JobDefOf.AttackMelee, enemyTarget)
            {
                expiryInterval = JobGiver_DragonCombat.ExpiryInterval_Melee.RandomInRange,
                checkOverrideOnExpire = true,
                expireRequiresEnemiesNearby = true
            };
        }

        protected virtual Job FireBlast(Thing enemyTarget)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_FireBlast"), enemyTarget)
            {
                expiryInterval = JobGiver_DragonCombat.ExpiryInterval_Melee.RandomInRange,
                checkOverrideOnExpire = true,
                expireRequiresEnemiesNearby = true
            };
        }

        protected virtual Job WingBuffet(Thing enemyTarget)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_WingBuffet"), enemyTarget)
            {
                expiryInterval = JobGiver_DragonCombat.ExpiryInterval_Melee.RandomInRange,
                checkOverrideOnExpire = true,
                expireRequiresEnemiesNearby = true
            };
        }
        
        protected virtual Job FlyToRearTarget(Thing enemyTarget)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("LotRD_FlyToRearTarget"), enemyTarget)
            {
                expiryInterval = JobGiver_DragonCombat.ExpiryInterval_Melee.RandomInRange,
                checkOverrideOnExpire = true,
                expireRequiresEnemiesNearby = true
            };
        }

        // Token: 0x06000473 RID: 1139 RVA: 0x0002C51C File Offset: 0x0002A91C
        protected virtual void UpdateEnemyTarget(Pawn pawn)
        {
            Thing thing = pawn.mindState.enemyTarget;
            if (thing != null && (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) || (float)(pawn.Position - thing.Position).LengthHorizontalSquared > this.targetKeepRadius * this.targetKeepRadius || ((IAttackTarget)thing).ThreatDisabled(pawn)))
            {
                thing = null;
            }
            if (thing == null)
            {
                thing = this.FindAttackTargetIfPossible(pawn);
                if (thing != null)
                {
                    pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
                    Lord lord = pawn.GetLord();
                    if (lord != null)
                    {
                        lord.Notify_PawnAcquiredTarget(pawn, thing);
                    }
                }
            }
            else
            {
                Thing thing2 = this.FindAttackTargetIfPossible(pawn);
                if (thing2 == null && !this.chaseTarget)
                {
                    thing = null;
                }
                else if (thing2 != null && thing2 != thing)
                {
                    pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
                    thing = thing2;
                }
            }
            pawn.mindState.enemyTarget = thing;
            if (thing is Pawn && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
        }

        // Token: 0x06000474 RID: 1140 RVA: 0x0002C682 File Offset: 0x0002AA82
        private Thing FindAttackTargetIfPossible(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
            {
                return null;
            }
            return this.FindAttackTarget(pawn);
        }

        // Token: 0x06000475 RID: 1141 RVA: 0x0002C6A4 File Offset: 0x0002AAA4
        protected virtual Thing FindAttackTarget(Pawn pawn)
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat;
            if (this.needLOSToAcquireNonPawnTargets)
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
            }
            if (this.PrimaryVerbIsIncendiary(pawn))
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => this.ExtraTargetValidator(pawn, x), 0f, this.targetAcquireRadius, this.GetFlagPosition(pawn), this.GetFlagRadius(pawn), false, true);
        }

        // Token: 0x06000476 RID: 1142 RVA: 0x0002C734 File Offset: 0x0002AB34
        private bool PrimaryVerbIsIncendiary(Pawn pawn)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    if (allVerbs[i].verbProps.isPrimary)
                    {
                        return allVerbs[i].IsIncendiary();
                    }
                }
            }
            return false;
        }

        // Token: 0x0400028C RID: 652
        private float targetAcquireRadius = 56f;

        // Token: 0x0400028D RID: 653
        private float targetKeepRadius = 65f;

        // Token: 0x0400028E RID: 654
        private bool needLOSToAcquireNonPawnTargets;

        // Token: 0x0400028F RID: 655
        private bool chaseTarget;

        // Token: 0x04000290 RID: 656
        public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

        // Token: 0x04000291 RID: 657
        private static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

        // Token: 0x04000292 RID: 658
        private const int MinTargetDistanceToMove = 5;

        // Token: 0x04000293 RID: 659
        private const int TicksSinceEngageToLoseTarget = 400;
    }
}
