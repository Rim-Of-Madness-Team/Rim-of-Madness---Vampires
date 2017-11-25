using System;
using System.Collections.Generic;
using Verse;

namespace Vampire
{
    public class HediffCompProperties_TickGraphic : HediffCompProperties
    {
        public int cycleRate = 30;
        public List<GraphicData> cycleGraphics;

        public HediffCompProperties_TickGraphic()
        {
            this.compClass = typeof(HediffComp_TickGraphic);
        }
    }
}
