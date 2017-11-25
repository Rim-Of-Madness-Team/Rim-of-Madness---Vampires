using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_CorruptForm : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            if (!JecsTools.GrappleUtility.CanGrapple(this.CasterPawn, target))
            {
                base.Effect(target);
                int boolSel = Rand.Range(0, 2);
                string tagOne = "";
                string tagTwo = "";
                HediffDef hediffDefOne = null;
                HediffDef hediffDefTwo = null;
                switch (boolSel)
                {
                    case 0:
                        tagOne = "MovingLimbCore";
                        tagTwo = "SightSource";
                        hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Legs;
                        hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Sight;

                        break;
                    case 1:
                        tagOne = "ManipulationLimbCore";
                        tagTwo = "SightSource";
                        hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Arms;
                        hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Sight;
                        break;
                    case 2:
                        tagOne = "ManipulationLimbCore";
                        tagTwo = "MovingLimbCore";
                        hediffDefOne = VampDefOf.ROMV_CorruptFormHediff_Arms;
                        hediffDefTwo = VampDefOf.ROMV_CorruptFormHediff_Legs;
                        break;
                }

                IEnumerable<BodyPartRecord> recs = target.health.hediffSet.GetNotMissingParts();
                if (recs.FirstOrDefault(x => (x.def.tags.Contains(tagOne))) is BodyPartRecord bp)
                {
                    HediffGiveUtility.TryApply(target, hediffDefOne, new List<BodyPartDef> { bp.def });
                }
                if (recs.FirstOrDefault(x => (x.def.tags.Contains(tagTwo))) is BodyPartRecord bpII)
                {
                    HediffGiveUtility.TryApply(target, hediffDefTwo, new List<BodyPartDef> { bpII.def });
                }
            }

        }
    }
}
