using RimWorld;
using Verse;

namespace Vampire;

public class InteractionWorker_Bonding : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }
}