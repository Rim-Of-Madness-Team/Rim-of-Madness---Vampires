using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_BatForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(this.CasterPawn, VampDefOf.ROMV_BatFormHediff, 1.0f);
        }
    }
}
