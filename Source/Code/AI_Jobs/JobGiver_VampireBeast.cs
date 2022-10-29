using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

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
        if (pawn?.CurJob?.def == VampDefOf.ROMV_Feed) return null;
        if (pawn?.TryGetAttackVerb(null) == null) return null;

        var pawn2 = FindPawnTarget(pawn);
        if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
        {
            if (pawn2.InAggroMentalState)
                return MeleeAttackJob(pawn2);
            return FeedJob(pawn2);
        }

        var building = FindTurretTarget(pawn);
        if (building != null) return MeleeAttackJob(building);
        if (pawn2 != null)
            using (var pawnPath = pawn.MapHeld.pathFinder.FindPath(pawn.Position, pawn2.Position,
                       TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
            {
                if (!pawnPath.Found) return null;
                IntVec3 cellBeforeBlocker;
                var thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null)
                    //Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                    //if (job != null)
                    //{
                    return MeleeAttackJob(thing);
                //}
                IntVec3 loc;
                pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc);
                var randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map), 9, TraverseParms.For(pawn))
                    .RandomCell;
                if (randomCell == pawn.PositionHeld) return new Job(JobDefOf.Wait, 30);
                return new Job(JobDefOf.Goto, randomCell);
            }

        Building buildingDoor = FindDoorTarget(pawn);
        if (buildingDoor != null) return MeleeAttackJob(buildingDoor);

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
        return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable,
            x => x is Pawn p && p.Spawned && !p.Dead && p?.BloodNeed()?.CurBloodPoints > 0 && !p.IsVampire(true) &&
                 (p.PositionHeld.Roofed(p.Map) || !VampireUtility.IsDaylight(p)), 0f, 9999f, default, 3.40282347E+38f,
            true);
    }

    private Building FindTurretTarget(Pawn pawn)
    {
        return (Building)AttackTargetFinder.BestAttackTarget(pawn,
            TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable |
            TargetScanFlags.NeedThreat, t => t is Building && t.Spawned, 0f, 70f);
    }


    private Building_Door FindDoorTarget(Pawn pawn)
    {
        return (Building_Door)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable,
            t => t is Building_Door && t.Spawned, 0f, 70f);
    }
}