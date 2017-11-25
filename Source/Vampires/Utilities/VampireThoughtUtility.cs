using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public static class VampireThoughtUtility
    {
        public static void GiveThoughtsForDiablerie(Pawn drainer)
        {
            if (!drainer.RaceProps.Humanlike)
            {
                return;
            }
            drainer.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(VampDefOf.ROMV_IConsumedASoul, 0), null);
        }

        public static void GiveThoughtsForDrinkingBlood(Pawn drainer)
        {
            if (!drainer.RaceProps.Humanlike)
            {
                return;
            }
            drainer.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(VampDefOf.ROMV_IGaveTheKiss, 0), null);
        }



        public static void GiveThoughtsForPawnDiedOfBloodLoss(Pawn victim, Pawn drainer = null)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            int thoughtIndex = 1;
            if (victim.guilt.IsGuilty || drainer == null)
            {
                thoughtIndex = 0;
            }
            ThoughtDef def;
            if (victim.IsColonist)
            {
                def = VampDefOf.ROMV_KnowColonistDiedOfBloodLoss;
            }
            else
            {
                def = VampDefOf.ROMV_KnowGuestDiedOfBloodLoss;
            }
            foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                     where x.IsColonist || x.IsPrisonerOfColony
                                     select x)
            {
                current.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, thoughtIndex), null);
            }
        }

        public static void GiveThoughtsForPawnBloodHarvested(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            ThoughtDef thoughtDef = null;
            if (victim.HostFaction == Faction.OfPlayer)
            {
                thoughtDef = VampDefOf.ROMV_KnowGuestBloodHarvested;
            }
            foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                     where x.IsColonist || x.IsPrisonerOfColony
                                     select x)
            {
                if (current == victim)
                {
                    current.needs.mood.thoughts.memories.TryGainMemory(VampDefOf.ROMV_MyBloodHarvested, null);
                }
                else if (thoughtDef != null)
                {
                    current.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, null);
                }
            }
        }



    }
}
