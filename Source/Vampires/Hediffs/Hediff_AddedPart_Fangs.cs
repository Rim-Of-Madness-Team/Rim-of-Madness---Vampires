using RimWorld;
using Verse;

namespace Vampire
{
    public class Hediff_AddedPart_Fangs : Hediff_AddedPart
    {
        /// <summary>
        /// Sometimes raiders would arrive with fangs, and yet
        /// have no vampirism.
        /// This will fix that issue entirely, and a bit heavy handedly.
        /// </summary>
        public override bool ShouldRemove
        {
            get
            {
                if (pawn.IsVampire(true))
                    return false;
                return true;
            }
        }
        
        /// <summary>
        /// If raiders show up with fangs, and the fangs are removed by the 'ShouldRemove' check,
        /// so we should also give them back their jaws.
        /// </summary>
        public override void PostRemoved()
        {           
            if (!pawn.IsVampire(true))
                pawn.health.RestorePart(Part, this, false);

            base.PostRemoved();
        }
    }
}
