using RimWorld;
using Vampire.Utilities;
using Verse;

namespace Vampire.Hediffs
{
    public class HediffWithComps_BeastHunger : HediffWithComps
    {
        private int checkRate = 240;
        public int ticksRemaining = GenDate.TicksPerHour;
        public static MentalStateDef MentalState_VampireBeast = DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");

        public override void Tick()
        {
            base.Tick();
            
            if (Find.TickManager.TicksGame % checkRate == 0)
            {
                if (pawn?.BloodNeed() is Need_Blood pB)
                {
                    if (CurStageIndex == 3 && pawn.MentalStateDef != MentalState_VampireBeast)
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true);
                    }

                    if (pB.CurLevelPercentage < 0.3f)
                    {
                        Severity += 0.005f;
                    }
                    else
                    {
                        Severity -= 0.2f;
                    }
                    
                }
            }
        }
        
    }
}
