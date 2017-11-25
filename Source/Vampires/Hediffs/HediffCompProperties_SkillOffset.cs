using RimWorld;
using System;
using Verse;

namespace Vampire
{
    public class HediffCompProperties_SkillOffset : HediffCompProperties
    {
        public SkillDef skillDef;
        public int offset;
        public HediffCompProperties_SkillOffset()
        {
            this.compClass = typeof(HediffComp_SkillOffset);
        }
    }
}
