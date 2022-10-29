using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class DisciplineEffect_Mesmerise : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        if (target.Faction == CasterPawn.Faction) //To avoid throwing red errors
            target.ClearMind();

        //See Issue 5: Make sure all pawns know that the target is no longer a threat, to force them to re-evaluate attacking.
        //Useful when clearing beserk status.
        foreach (var p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(p =>
                     p.mindState.meleeThreat == target))
        {
            p.mindState.meleeThreat = null;
            p.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}