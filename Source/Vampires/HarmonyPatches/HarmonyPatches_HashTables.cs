using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DigHoleAttempt
    {
        private float workLeft = -1000f;
        private int ticksSinceAttempt = Int32.MinValue;

        public float WorkLeft
        {
            get => workLeft;
            set => workLeft = value;
        }

        public int TicksSinceAttempt
        {
            get => ticksSinceAttempt;
            set => ticksSinceAttempt = value;
        }
        
        public bool ShouldUseData()
        {
            if ((ticksSinceAttempt + GenDate.TicksPerHour > Find.TickManager.TicksGame))
                return true;
            return false;
        }
        
        public DigHoleAttempt(int ticksForAttempt, float workLeft)
        {
            this.ticksSinceAttempt = ticksForAttempt;
            this.workLeft = workLeft;
        }
    }

    public partial class HarmonyPatches
    {
        public static Dictionary<Pawn, DigHoleAttempt> DigHoleAttempts = new Dictionary<Pawn, DigHoleAttempt>();
    }
}