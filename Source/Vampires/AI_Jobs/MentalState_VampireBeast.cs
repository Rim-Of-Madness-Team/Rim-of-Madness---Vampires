using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class MentalState_VampireBeast : MentalState
    {
        CompVampire vampComp => pawn.GetComp<CompVampire>();
        Need_Blood vampBlood => pawn.needs.TryGetNeed<Need_Blood>();

        public override bool ForceHostileTo(Thing t)
        {
            return true;
        }
        
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (vampComp != null)
            {
                if (!vampComp.IsVampire)
                    RecoverFromState();
                if (vampBlood.IsFull)
                    RecoverFromState();
            }
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
}
