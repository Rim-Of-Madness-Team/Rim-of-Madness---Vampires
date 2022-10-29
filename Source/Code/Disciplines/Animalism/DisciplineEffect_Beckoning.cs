using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire;

public class DisciplineEffect_Beckoning : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);

        var count = new IntRange(15, 25).RandomInRange;
        IntVec3 loc;
        if (RCellFinder.TryFindRandomPawnEntryCell(out loc, target.Map, CellFinder.EdgeRoadChance_Animal))
        {
            VampireUtility.SummonEffect(loc, CasterPawn.Map, CasterPawn, 10f);

            for (var i = 0; i < count; i++)
                target.Map.wildAnimalSpawner.SpawnRandomWildAnimalAt(loc);
        }

        Find.LetterStack.ReceiveLetter("ROMV_AnimalHerd".Translate(), "ROMV_AnimalHerdDesc".Translate(),
            LetterDefOf.PositiveEvent, new GlobalTargetInfo(loc, target.Map));
    }
}