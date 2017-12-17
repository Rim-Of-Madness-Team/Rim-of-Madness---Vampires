using System.Collections.Generic;
using RimWorld;
using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Auspex
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
                                MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, "ROMV_HediffRemoved".Translate(h.def.LabelCap));
                                p.health.hediffSet.hediffs.Remove(h);
                            }
                        }
                    }
                }
                if (t.Cell != default(IntVec3) && t.Cell is IntVec3 c)
                {
                    // Verse.FogGrid
                    if (CasterPawn.Map.fogGrid.IsFogged(c))
                    {
                        MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess);
                        CasterPawn.Map.fogGrid.Notify_FogBlockerRemoved(c);
                        return;
                    }
                    else
                    {
                        MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastFailure);
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
