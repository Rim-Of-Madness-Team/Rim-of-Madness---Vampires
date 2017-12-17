using System.Collections.Generic;
using Verse;

namespace Vampire.Disciplines.Animalism
{
    public class HediffCompProperties_TickGraphic : HediffCompProperties
    {
        public int cycleRate = 30;
        public List<GraphicData> cycleGraphics;

        public HediffCompProperties_TickGraphic()
        {
            compClass = typeof(HediffComp_TickGraphic);
        }
    }
}
