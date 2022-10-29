using System.Linq;
using Verse;

namespace Vampire;

public class DisciplineEffect_BloodLust : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        var pawnList = target?.MapHeld?.mapPawns?.AllPawns
            ?.Where(x => x.Faction == CasterPawn.Faction && x?.health?.hediffSet?.hediffs != null)?.ToList();
        if (pawnList == null || pawnList.Count <= 0) return;
        foreach (var p in pawnList) HealthUtility.AdjustSeverity(p, VampDefOf.ROMV_Presence_BloodLustHediff, 1.0f);
    }
}