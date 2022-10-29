using RimWorld;
using Verse;

namespace Vampire;

public class PawnRelationWorker_Ghoul : PawnRelationWorker
{
    public override bool InRelation(Pawn me, Pawn other)
    {
        if (me.IsVampire(true) && other.IsVampire(true))
        {
            var compVamp = other.GetComp<CompVampire>();
            return me != other && other.GetSire() == me;
        }

        return false;
    }

    public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
    {
        return 0f;
    }

    public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
    {
        other.SetSire(generated);
    }
}