using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using RimWorld;
using System.Linq;

namespace Vampire
{
    public class Recipe_TransferBlood : Recipe_Surgery
    {
        [DebuggerHidden]
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (pawn?.BloodNeed() is Need_Blood n && (n.BloodWanted > 0 || GetBloodLoss(pawn) != null))
            {
                if (pawn.RaceProps.body.corePart is BodyPartRecord r)
                    yield return r;
            }
        }


        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                //if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part))
                //{
                //    return;
                //}
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.BloodLoss);
            }
        }

        private Hediff GetBloodLoss(Pawn pawn)
        {
            return pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == HediffDefOf.BloodLoss);
        }
    }
}
