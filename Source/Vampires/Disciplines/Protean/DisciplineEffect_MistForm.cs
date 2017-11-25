using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_MistForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(this.CasterPawn, VampDefOf.ROMV_MistFormHediff, 1.0f);
        }
    }
}
