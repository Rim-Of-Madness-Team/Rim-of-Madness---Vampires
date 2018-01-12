using Verse;

namespace Vampire
{
    public class HediffVampirismGenerationBonus : HediffWithComps
    {
 
        public override string LabelBase
        {
            get
            {
                if (pawn.VampComp().Generation != -1)
                {
                    return base.LabelBase + " (" + "ROMV_HI_Generation".Translate(HediffVampirism.AddOrdinal(pawn.VampComp().Generation)) + ")";
                }
                return base.LabelBase;
            }
        }


        public override bool ShouldRemove => this.def != pawn.GenerationDef();

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.def != pawn.GenerationDef())
            {
                HealthUtility.AdjustSeverity(pawn, pawn.GenerationDef(), 1.0f);
            }
        }
    }
}