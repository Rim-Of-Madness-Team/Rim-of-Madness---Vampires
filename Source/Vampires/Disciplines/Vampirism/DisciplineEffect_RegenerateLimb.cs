using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_RegenerateLimb : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);

            /// Restores all missing parts when transforming   
            List<Hediff_MissingPart> missingParts = new List<Hediff_MissingPart>().Concat(target?.health?.hediffSet?.GetMissingPartsCommonAncestors()).ToList();
            if (!missingParts.NullOrEmpty())
            {
                Hediff_MissingPart partToRestore = missingParts.RandomElement();
                target.health.RestorePart(partToRestore.Part);
                
                Messages.Message("ROM_WerewolfLimbRegen".Translate(new object[] {
                target.LabelShort,
                partToRestore.Label
            }), MessageTypeDefOf.PositiveEvent);
            }
        }

//        int maxInjuries = 6;
//        int maxInjuriesPerBodypart;

//                foreach (BodyPartRecord rec in pawn.health.hediffSet.GetInjuredParts())
//                {
//                    if (maxInjuries > 0)
//                    {
//                        maxInjuriesPerBodypart = 2;
//                        foreach (Hediff_Injury current in from injury in pawn.health.hediffSet.GetHediffs<Hediff_Injury>() where injury.Part == rec select injury)
//                        {
//                            if (maxInjuriesPerBodypart > 0)
//                            {
//                                if (current.CanHealNaturally() && !current.IsOld()) // basically check for scars and old wounds
//                                {
//                                    current.Heal((int) current.Severity + 1);
//                                    maxInjuries--;
//                                    maxInjuriesPerBodypart--;
//                                }
//}
//                        }
//                    }
//                }
    }
}
