using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_FeralClaws : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            IEnumerable<BodyPartRecord> recs = target.health.hediffSet.GetNotMissingParts();
            Dictionary<BodyPartRecord, HediffDef> bodyPartRecords = new Dictionary<BodyPartRecord, HediffDef>();
            if (recs.FirstOrDefault(x => (x.def == BodyPartDefOf.LeftHand)) is BodyPartRecord leftHand)
                bodyPartRecords.Add(leftHand, VampDefOf.ROMV_FeralClaw);
            if (recs.FirstOrDefault(x => (x.def == BodyPartDefOf.RightHand)) is BodyPartRecord rightHand)
                bodyPartRecords.Add(rightHand, VampDefOf.ROMV_FeralClaw);

            if ((bodyPartRecords?.Count() ?? 0) > 0)
            {
                foreach (KeyValuePair<BodyPartRecord, HediffDef> transformableParts in bodyPartRecords)
                {
                    Hediff transformedHediff = HediffMaker.MakeHediff(transformableParts.Value, target, transformableParts.Key);
                    transformedHediff.Severity = 1.0f;
                    target.health.AddHediff(transformedHediff, transformableParts.Key, null);
                }
            }
        }
    }
}
