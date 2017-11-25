using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_Beckoning : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);

            int count = new IntRange(15, 25).RandomInRange;
            IntVec3 loc;
            if (RCellFinder.TryFindRandomPawnEntryCell(out loc, target.Map, CellFinder.EdgeRoadChance_Animal, null))
            {
                for (int i = 0; i < count; i++)
                    target.Map.wildSpawner.SpawnRandomWildAnimalAt(loc);
            }
            Find.LetterStack.ReceiveLetter("ROMV_AnimalHerd".Translate(), "ROMV_AnimalHerdDesc".Translate(), LetterDefOf.PositiveEvent, new RimWorld.Planet.GlobalTargetInfo(loc, target.Map), null);
        }
    }
}
