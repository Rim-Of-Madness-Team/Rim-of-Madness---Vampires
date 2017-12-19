using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_StemTheTide : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            Log.Message("1");
            
            List<Hediff_Injury> injuries = new List<Hediff_Injury>(target.health.hediffSet.GetInjuriesTendable());
            
            if (!injuries.NullOrEmpty())
            {
                Log.Message("2");

                foreach (Hediff_Injury injury in injuries)
                {
                    Log.Message("3");
                    if (injury.Bleeding)
                        injury.Heal(30);
                }
            }
        }
    }
}
