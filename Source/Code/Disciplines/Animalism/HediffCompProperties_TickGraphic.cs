using System.Collections.Generic;
using Verse;

namespace Vampire;

public class HediffCompProperties_TickGraphic : HediffCompProperties
{
    public List<GraphicData> cycleGraphics;
    public int cycleRate = 30;

    public HediffCompProperties_TickGraphic()
    {
        compClass = typeof(HediffComp_TickGraphic);
    }
}