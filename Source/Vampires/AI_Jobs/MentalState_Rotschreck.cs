using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class MentalState_Rotschreck : MentalState
    {
        CompVampire vampComp => this.pawn.GetComp<CompVampire>();
        Need_Blood vampBlood => this.pawn.needs.TryGetNeed<Need_Blood>();

        public override bool ForceHostileTo(Thing t)
        {
            return true;
        }

        protected override bool CanEndBeforeMaxDurationNow => false;
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (pawn.PositionHeld.Roofed(pawn.MapHeld))
                this.RecoverFromState();
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
}
