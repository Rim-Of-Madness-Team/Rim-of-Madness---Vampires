using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class MentalState_Rotschreck : MentalState
{
    private CompVampire vampComp => pawn.GetComp<CompVampire>();
    private Need_Blood vampBlood => pawn.needs.TryGetNeed<Need_Blood>();

    protected override bool CanEndBeforeMaxDurationNow => false;

    public override bool ForceHostileTo(Thing t)
    {
        return true;
    }

    public override void MentalStateTick()
    {
        base.MentalStateTick();
        if (pawn.PositionHeld.Roofed(pawn.MapHeld))
            RecoverFromState();
        //Room room = pawn.GetRoom(RegionType.Set_All);
        //if (room != null)
        //{
        //    if (!room.PsychologicallyOutdoors)
        //        this.RecoverFromState();
        //}
    }

    public override bool ForceHostileTo(Faction f)
    {
        return true;
    }

    public override RandomSocialMode SocialModeMax()
    {
        return RandomSocialMode.Off;
    }
}