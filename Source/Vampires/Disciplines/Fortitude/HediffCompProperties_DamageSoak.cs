using System;
using Verse;

namespace Vampire
{
    public class HediffCompProperties_DamageSoak : HediffCompProperties_Disappears
    {
        public int damageToSoak = 1;

        public HediffCompProperties_DamageSoak()
        {
            this.compClass = typeof(HediffComp_DamageSoak);
        }
    }
}
