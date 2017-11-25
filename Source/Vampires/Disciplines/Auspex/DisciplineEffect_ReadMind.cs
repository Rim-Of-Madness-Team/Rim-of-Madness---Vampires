using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_ReadMind : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
            HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(VampDefOf.ROMV_MindReadingHediff, this.CasterPawn, null);
            if (hediff.TryGetComp<HediffComp_ReadMind>() is HediffComp_ReadMind rm)
            {
                rm.MindBeingRead = target;
            }
            hediff.Severity = 1.0f;
            this.CasterPawn.health.AddHediff(hediff, null, null);
        }
    }
}
