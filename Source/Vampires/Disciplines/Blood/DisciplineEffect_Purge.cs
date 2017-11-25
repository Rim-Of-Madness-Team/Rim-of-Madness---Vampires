using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_Purge : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            if (target.RaceProps.IsMechanoid)
            {
                Messages.Message("ROMV_CannotPurgeMechanoids".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            target.ClearMind();
            target.jobs.TryTakeOrderedJob(new Verse.AI.Job(VampDefOf.ROMV_BloodVomit, target.PositionHeld));
        }
    }
}
