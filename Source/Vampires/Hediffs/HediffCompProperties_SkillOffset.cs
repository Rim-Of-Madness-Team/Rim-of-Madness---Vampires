using RimWorld;
using Verse;

namespace Vampire.Hediffs
{
    public class HediffCompProperties_SkillOffset : HediffCompProperties
    {
        public SkillDef skillDef;
        public int offset;
        public HediffCompProperties_SkillOffset()
        {
            compClass = typeof(HediffComp_SkillOffset);
        }
    }
}
