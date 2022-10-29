using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobGiver_ShareBlood : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn)
    {
        //Log.Message("0");
        if (pawn.VampComp() == null)
            //Log.Message("0a");
            return 0f;
        if (!pawn.VampComp().IsVampire) return 0f;

        if (pawn.Drafted) return 0f;

        var blood = pawn.needs.TryGetNeed<Need_Blood>();
        if (blood == null) return 0f;
        if (blood.CurLevelPercentage < blood.ShouldFeedPerc) return 0f;
        if (pawn.VampComp().Ghouls is { } ghouls && !ghouls.NullOrEmpty() &&
            ghouls.Any(ghoul => ShouldGiveBloodNow(pawn, ghoul)))
            return 9.5f;
        //Log.Message("0f");
        return 0f;
    }

    private static bool ShouldGiveBloodNow(Pawn vampire, Pawn ghoul)
    {
        if (ghoul?.VampComp()?.ThrallData?.ShouldFeedBlood == true &&
            ghoul.MapHeld != null && ghoul.Spawned && vampire.Spawned &&
            ghoul.MapHeld == vampire.MapHeld)
            return true;
        return false;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        return GiveBloodJob(pawn);
    }

    private static Job GiveBloodJob(Pawn pawn)
    {
        var blood = pawn.needs.TryGetNeed<Need_Blood>();
        if (blood == null)
            return null;
        var ghoulToFeed = pawn.VampComp().Ghouls
            .FirstOrDefault(ghoul => ghoul.VampComp()?.ThrallData?.ShouldFeedBlood ?? false);
        return ghoulToFeed != null ? new Job(VampDefOf.ROMV_GhoulBloodBond, ghoulToFeed) : null;
    }
}