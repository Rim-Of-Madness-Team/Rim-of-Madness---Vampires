using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_RevealIllusions : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            if (TargetsAoE[0] is LocalTargetInfo t)
            {
                if (t.Thing is Pawn p)
                {
                    List<HediffDef> defsToCheck = new List<HediffDef>()
                    {
                        VampDefOf.ROMV_CorruptFormHediff_Arms, VampDefOf.ROMV_CorruptFormHediff_Legs,
                        VampDefOf.ROMV_CorruptFormHediff_Sight, VampDefOf.ROMV_PossessionHediff
                    };
                    if (p?.health?.hediffSet?.hediffs is List<Hediff> hList)
                    {
                        foreach (Hediff h in hList)
                        {
                            if (defsToCheck.Contains(h.def))
                            {
                                MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, "ROMV_HediffRemoved".Translate(h.def.LabelCap), -1f);
                                p.health.hediffSet.hediffs.Remove(h);
                            }
                        }
                    }
                }
                if (t.Cell != default(IntVec3) && t.Cell is IntVec3 c)
                {
                    // Verse.FogGrid
                    if (this.CasterPawn.Map.fogGrid.IsFogged(c))
                    {
                        MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
                        this.CasterPawn.Map.fogGrid.Notify_FogBlockerRemoved(c);
                        return;
                    }
                    else
                    {
                        MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastFailure, -1f);
                    }
                }
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult)
            {
                Effect( );
                outResult = true;
            }
            outResult = inResult;
        }
    }
}
