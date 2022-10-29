using System;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobGiver_SeekShelterFromSunlight : ThinkNode_JobGiver
{
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
    public override float GetPriority(Pawn pawn)
    {
        return 1;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        try
        {
            if (pawn?.CurJobDef?.defName != "VampDefOf.ROMV_GotoSafety")
            {
                if (pawn.MapHeld is { } map && pawn.PositionHeld is { } pos && pos.IsValid &&
                    !pos.Roofed(map) && VampireUtility.IsForcedDarknessConditionInactive(map))
                    if (VampSunlightPathUtility.GetSunlightPathJob(pawn) is { } j)
                        return j;
                if (pawn?.MentalStateDef?.defName == "ROMV_Rotschreck")
                    if (VampSunlightPathUtility.GetSunlightPathJob(pawn, true) is { } j)
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