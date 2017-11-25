using System;
using Verse;

namespace Vampire
{
    public class HediffCompProperties_Possession : HediffCompProperties_Disappears
    {
        public HediffCompProperties_Possession()
        {
            this.compClass = typeof(HediffComp_Possession);
        }
    }
}
