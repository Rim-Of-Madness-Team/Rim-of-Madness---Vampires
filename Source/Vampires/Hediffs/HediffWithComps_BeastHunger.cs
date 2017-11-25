using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Vampire
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
                if (this.pawn?.BloodNeed() is Need_Blood pB)
                {
                    if (this.CurStageIndex == 3 && this.pawn.MentalStateDef != MentalState_VampireBeast)
                    {
                        this.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true, false, null);
                    }

                    if (pB.CurLevelPercentage < 0.3f)
                    {
                        this.Severity += 0.005f;
                    }
                    else
                    {
                        this.Severity -= 0.2f;
                    }
                    
                }
            }
        }
        
    }
}
