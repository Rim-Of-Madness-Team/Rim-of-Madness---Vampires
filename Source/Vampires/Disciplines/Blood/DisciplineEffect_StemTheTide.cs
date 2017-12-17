using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_StemTheTide : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>(target.health.hediffSet.GetInjuriesTendable());
            
            if (!injuries.NullOrEmpty())
            {
                foreach (Hediff_Injury injury in injuries)
                {
                    if (injury.Bleeding)
                        injury.Heal(30);
                }
            }
        }
    }
}
