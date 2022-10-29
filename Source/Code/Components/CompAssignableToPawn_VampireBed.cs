using RimWorld;
using Verse;

namespace Vampire;

public class CompAssignableToPawn_VampireBed : CompAssignableToPawn_Bed
{
    public Building_Coffin coffin => ((Building_Bed)parent).PositionHeld.GetFirstThing<Building_Coffin>(parent.Map);

    public override AcceptanceReport CanAssignTo(Pawn pawn)
    {
        if (coffin != null && !coffin.HasAnyContents)
            if (coffin.TryGetComp<CompVampBed>() is { } vbed)
            {
                if (!pawn.IsVampire(true) && vbed.VampiresOnly)
                    return AcceptanceReport.WasRejected;
                return AcceptanceReport.WasAccepted;
            }

        return AcceptanceReport.WasRejected;
    }

    public override void TryAssignPawn(Pawn pawn)
    {
        pawn.ownership.ClaimBedIfNonMedical((Building_Bed)parent);
    }

    public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
    {
        pawn.ownership.UnclaimBed();
    }
}