using RimWorld;
using Verse;

namespace Vampire;

public class PawnRelationWorker_Sire : PawnRelationWorker
{
    public override bool InRelation(Pawn me, Pawn other)
    {
        if (me.IsVampire(true) && other.IsVampire(true))
        {
            var compVamp = other.GetComp<CompVampire>();
            return me.IsChildeOf(other);
        }

        return false;
    }

    public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
    {
        return 0f;
    }

    public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
    {
        generated.SetSire(other);
        //generated.relations.AddDirectRelation(VampDefOf.ROMV_Sire, other);
    }
}