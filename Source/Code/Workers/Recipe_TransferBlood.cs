using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace Vampire;

public class Recipe_TransferBlood : Recipe_Surgery
{
    [DebuggerHidden]
    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        if (pawn?.BloodNeed() is { } n && (n.BloodWanted > 0 || GetBloodLoss(pawn) != null))
            if (pawn.RaceProps.body.corePart is { } r)
                yield return r;
    }


    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            //if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part))
            //{
            //    return;
            //}
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == BloodUtility.GetBloodLossDef(pawn));
        }
    }

    private Hediff GetBloodLoss(Pawn pawn)
    {
        return pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == BloodUtility.GetBloodLossDef(pawn));
    }
}