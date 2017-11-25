using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class JobGiver_VampireBeast : ThinkNode_JobGiver
    {
        private const float WaitChance = 0.75f;

        private const int WaitTicks = 90;

        private const int MinMeleeChaseTicks = 420;

        private const int MaxMeleeChaseTicks = 900;

        private const int WanderOutsideDoorRegions = 9;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawn?.CurJob?.def == VampDefOf.ROMV_Feed)
            {
                return null;
            }
            if (pawn?.TryGetAttackVerb(false) == null)
            {
                return null;
            }
            
            Pawn pawn2 = this.FindPawnTarget(pawn);
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                if (pawn2.InAggroMentalState)
                    return this.MeleeAttackJob(pawn2);
                else
                    return this.FeedJob(pawn2);
            }

            Building building = this.FindTurretTarget(pawn);
            if (building != null)
            {
                return this.MeleeAttackJob(building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.MapHeld.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    IntVec3 cellBeforeBlocker;
                    Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                    if (thing != null)
                    {
                        //Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                        //if (job != null)
                        //{
                        return this.MeleeAttackJob(thing);
                        //}
                    }
                    IntVec3 loc = pawnPath.LastCellBeforeBlockerOrFinalCell(pawn.MapHeld);
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.PositionHeld)
                    {
                        return new Job(JobDefOf.Wait, 30, false);
                    }
                    return new Job(JobDefOf.Goto, randomCell);
                }
            }
            Building buildingDoor = this.FindDoorTarget(pawn);
            if (buildingDoor != null)
            {
                return this.MeleeAttackJob(buildingDoor);
            }

            return null;
        }


        private Job FeedJob(Pawn pawn)
        {
            return new Job(VampDefOf.ROMV_Feed, pawn);
        }

        private Job MeleeAttackJob(Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(900, 1800),
                attackDoorIfTargetLost = true,
                killIncappedTarget = true
            };
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing x) => x is Pawn p && p.Spawned && !p.Dead && p?.BloodNeed()?.CurBloodPoints > 0 && !p.IsVampire() && (p.PositionHeld.Roofed(p.Map) || !VampireUtility.IsDaylight(p)), 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
        }

        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building && t.Spawned, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
        }


        private Building_Door FindDoorTarget(Pawn pawn)
        {
            return (Building_Door)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing t) => t is Building_Door && t.Spawned, 0f, 70f, default(IntVec3), 3.40282347E+38f, false);
        }

    }
}
