using System.Collections.Generic;
using Verse;

namespace Vampire;

public class DisciplineEffect_StemTheTide : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        var injuries = new List<Hediff_Injury>(target.health.hediffSet.GetInjuriesTendable());

        if (!injuries.NullOrEmpty())
            foreach (var injury in injuries)
                if (injury.Bleeding)
                    injury.Heal(30);
    }
}