using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_FeralClaws : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        var recs = target.health.hediffSet.GetNotMissingParts();
        var bodyPartRecords = new Dictionary<BodyPartRecord, HediffDef>();
        var hands = recs.Where(x => x.def == BodyPartDefOf.Hand).ToList();
        if (hands?.Count() > 0)
            foreach (var hand in hands)
                bodyPartRecords.Add(hand, VampDefOf.ROMV_FeralClaw);

        if ((bodyPartRecords?.Count ?? 0) > 0)
            foreach (var transformableParts in bodyPartRecords)
            {
                var transformedHediff =
                    HediffMaker.MakeHediff(transformableParts.Value, target, transformableParts.Key);
                transformedHediff.Severity = 1.0f;
                target.health.AddHediff(transformedHediff, transformableParts.Key);
            }
    }
}