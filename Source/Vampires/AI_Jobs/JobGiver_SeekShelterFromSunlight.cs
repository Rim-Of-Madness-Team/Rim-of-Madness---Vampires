using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vampire
{
    public class JobGiver_SeekShelterFromSunlight : ThinkNode_JobGiver
    {

        protected IntRange ticksBetweenWandersRange = new IntRange(20, 100);

        protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

        protected Danger maxDanger = Danger.None;

        protected float wanderRadius = 7.5f;

        protected virtual IntVec3 GetExactWanderDest(Pawn pawn)
        {
            IntVec3 wanderRoot = pawn.PositionHeld;
            return RCellFinder.RandomWanderDestFor(pawn, wanderRoot, wanderRadius, delegate(Pawn p, IntVec3 v) { if (v.Roofed(p.MapHeld)) return true; return false; }, PawnUtility.ResolveMaxDanger(pawn, maxDanger));
        }

//        public override float GetPriority(Pawn pawn)
//        {
//            Log.Message("VampJobPriority");
//
//            if (pawn.VampComp() is CompVampire v && v.IsVampire &&
//            GenLocalDate.HourInteger(pawn) >= 6 && GenLocalDate.HourInteger(pawn) <= 17 &&
//            !pawn.PositionHeld.Roofed(pawn.MapHeld))
//            {
//                return 9.5f;
//            }
//            return 0f;
//        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            try
            {
                if (pawn.MapHeld is Map map && pawn.PositionHeld is IntVec3 pos && pos.IsValid && !pos.Roofed(map))
                {
                    if (VampSunlightPathUtility.GetSunlightPathJob(pawn) is Job j)
                        return j;   
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            return null;
        }

        //private static Region ClosestRegionIndoors(IntVec3 root, Map map, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
        //{
        //    Region region = root.GetRegion(map, traversableRegionTypes);
        //    if (region == null)
        //    {
        //        return null;
        //    }
        //    RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
        //    Region foundReg = null;
        //    RegionProcessor regionProcessor = delegate (Region r)
        //    {
        //        if (r.portal != null)
        //        {
        //            return false;
        //        }
        //        if (!r.Room.PsychologicallyOutdoors)
        //        {
        //            foundReg = r;
        //            return true;
        //        }
        //        return false;
        //    };
        //    RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
        //    return foundReg;
        //}
    }
}
