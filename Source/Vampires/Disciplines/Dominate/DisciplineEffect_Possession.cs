using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class DisciplineEffect_Possession : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            Hediff hediff = HediffMaker.MakeHediff(VampDefOf.ROMV_PossessionHediff, target, null);
            hediff.Severity = 1.0f;
            target.health.AddHediff(hediff, null, null);
            hediff.TryGetComp<HediffComp_Possession>().ActivateEffect(this.CasterPawn);
        }
    }
}
