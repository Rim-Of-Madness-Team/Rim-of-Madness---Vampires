using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public static class VampireThoughtUtility
{
    public static void GiveThoughtsForDiablerie(Pawn drainer)
    {
        if (!drainer.RaceProps.Humanlike) return;
        drainer.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(VampDefOf.ROMV_IConsumedASoul, 0));
    }

    public static void GiveThoughtsForDrinkingBlood(Pawn drainer)
    {
        if (!drainer.RaceProps.Humanlike) return;
        drainer.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(VampDefOf.ROMV_IGaveTheKiss, 0));
    }


    public static void GiveThoughtsForPawnDiedOfBloodLoss(Pawn victim, Pawn drainer = null)
    {
        if (!victim.RaceProps.Humanlike) return;
        var thoughtIndex = 1;
        if (victim.guilt.IsGuilty || drainer == null) thoughtIndex = 0;
        ThoughtDef def;
        if (victim.IsColonist)
            def = VampDefOf.ROMV_KnowColonistDiedOfBloodLoss;
        else
            def = VampDefOf.ROMV_KnowGuestDiedOfBloodLoss;
        foreach (var current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive
                 where x.IsColonist || x.IsPrisonerOfColony
                 select x)
            current.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, thoughtIndex));
    }

    public static void GiveThoughtsForPawnBloodHarvested(Pawn victim)
    {
        if (!victim.RaceProps.Humanlike) return;
        ThoughtDef thoughtDef = null;
        if (victim.HostFaction == Faction.OfPlayer) thoughtDef = VampDefOf.ROMV_KnowGuestBloodHarvested;
        foreach (var current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive
                 where x.IsColonist || x.IsPrisonerOfColony
                 select x)
            if (current == victim)
                current.needs.mood.thoughts.memories.TryGainMemory(VampDefOf.ROMV_MyBloodHarvested);
            else if (thoughtDef != null) current.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
    }
}