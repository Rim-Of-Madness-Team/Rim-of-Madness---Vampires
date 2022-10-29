using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_CorruptForm : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        if (JecsTools.GrappleUtility.TryGrapple(CasterPawn, target))
        {
            base.Effect(target);
            var boolSel = Rand.Range(0, 2);
            BodyPartTagDef tagOne = null;
            BodyPartTagDef tagTwo = null;
            HediffDef hediffDefOne = null;
            HediffDef hediffDefTwo = null;
            switch (boolSel)
            {
                case 0:
                    tagOne = BodyPartTagDefOf.MovingLimbCore;
                    tagTwo = BodyPartTagDefOf.SightSource;
                    hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Legs;
                    hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Sight;

                    break;
                case 1:
                    tagOne = BodyPartTagDefOf.ManipulationLimbCore;
                    tagTwo = BodyPartTagDefOf.SightSource;
                    hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Arms;
                    hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Sight;
                    break;
                case 2:
                    tagOne = BodyPartTagDefOf.ManipulationLimbCore;
                    tagTwo = BodyPartTagDefOf.MovingLimbCore;
                    hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Arms;
                    hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Legs;
                    break;
            }

            var recs = target.health.hediffSet.GetNotMissingParts();
            if (recs.FirstOrDefault(x => x.def.tags.Contains(tagOne)) is { } bp)
                HediffGiverUtility.TryApply(target, hediffDefOne, new List<BodyPartDef> { bp.def });
            if (recs.FirstOrDefault(x => x.def.tags.Contains(tagTwo)) is { } bpII)
                HediffGiverUtility.TryApply(target, hediffDefTwo, new List<BodyPartDef> { bpII.def });
        }
    }
}