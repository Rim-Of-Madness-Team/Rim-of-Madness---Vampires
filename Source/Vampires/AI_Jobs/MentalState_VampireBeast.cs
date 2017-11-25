using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class MentalState_VampireBeast : MentalState
    {
        CompVampire vampComp => this.pawn.GetComp<CompVampire>();
        Need_Blood vampBlood => this.pawn.needs.TryGetNeed<Need_Blood>();

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
                    this.RecoverFromState();
                if (vampBlood.IsFull)
                    this.RecoverFromState();
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
