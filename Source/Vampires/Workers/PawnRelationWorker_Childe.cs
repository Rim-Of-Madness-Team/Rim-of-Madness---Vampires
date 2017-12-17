using RimWorld;
using Vampire.Components;
using Vampire.Utilities;
using Verse;

namespace Vampire.Workers
{
    public class PawnRelationWorker_Childe : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            if (me.IsVampire() && other.IsVampire())
            {
                CompVampire compVamp = other.GetComp<CompVampire>();
                return me != other && (other.GetSire() == me);
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
}
