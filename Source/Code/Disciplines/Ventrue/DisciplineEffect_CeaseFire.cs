using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_CeaseFire : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        var pawnList = target?.MapHeld?.mapPawns?.AllPawns?.Where(x =>
            (x?.Faction?.HostileTo(CasterPawn.Faction) ?? false) && x?.mindState?.mentalStateHandler != null)?.ToList();
        if (pawnList == null || pawnList.Count <= 0) return;
        foreach (var p in pawnList)
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
    }
}