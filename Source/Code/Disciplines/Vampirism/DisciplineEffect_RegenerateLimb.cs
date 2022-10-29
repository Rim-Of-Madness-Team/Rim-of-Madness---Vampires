using Verse;

namespace Vampire;

public class DisciplineEffect_RegenerateLimb : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);

        VampireUtility.RegenerateRandomPart(target);
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
//                                if (current.CanHealNaturally() && !current.IsPermanent()) // basically check for scars and old wounds
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