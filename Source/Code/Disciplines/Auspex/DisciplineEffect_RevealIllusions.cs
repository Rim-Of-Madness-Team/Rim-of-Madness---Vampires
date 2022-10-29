using System.Collections.Generic;
using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_RevealIllusions : Verb_UseAbility
{
    public virtual void Effect()
    {
        CasterPawn.Drawer.Notify_DebugAffected();
        if (TargetsAoE[0] is { } t)
        {
            if (t.Thing is Pawn p)
            {
                var defsToCheck = new List<HediffDef>
                {
                    VampDefOf.ROMV_CorruptFormHediff_Arms, VampDefOf.ROMV_CorruptFormHediff_Legs,
                    VampDefOf.ROMV_CorruptFormHediff_Sight, VampDefOf.ROMV_PossessionHediff,
                    VampDefOf.ROMV_HideHediff, HediffDef.Named("ROMV_InvisibilityHediff"),
                    HediffDef.Named("ROMV_HiddenForceHediff")
                };
                if (ModLister.RoyaltyInstalled) defsToCheck.Add(HediffDef.Named("PsychicInvisibility"));
                if (p?.health?.hediffSet?.hediffs is { } hList)
                    foreach (var h in hList)
                        if (defsToCheck.Contains(h.def))
                        {
                            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map,
                                "ROMV_HediffRemoved".Translate(h.def.LabelCap));
                            p.health.hediffSet.hediffs.Remove(h);
                        }
            }

            if (t.Cell != default && t.Cell is { } c)
            {
                // Verse.FogGrid
                if (CasterPawn.Map.fogGrid.IsFogged(c))
                {
                    MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastSuccess);
                    CasterPawn.Map.fogGrid.Notify_FogBlockerRemoved(c);
                    return;
                }

                MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastFailure);
            }
        }
    }

    public override void PostCastShot(bool inResult, out bool outResult)
    {
        if (inResult)
        {
            Effect();
            outResult = true;
        }

        outResult = inResult;
    }
}