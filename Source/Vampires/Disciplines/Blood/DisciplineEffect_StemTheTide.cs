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
            
            foreach (var hediffInjury in target.health.hediffSet.GetInjuriesTendable())
            {
                Log.Message(hediffInjury.ToString());
            }
            
            if (target.health.hediffSet.GetInjuriesTendable() is List<Hediff_Injury> injuries && !injuries.NullOrEmpty())
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
