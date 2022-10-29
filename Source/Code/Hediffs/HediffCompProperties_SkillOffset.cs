using RimWorld;
using Verse;

namespace Vampire;

public class HediffCompProperties_SkillOffset : HediffCompProperties
{
    public int offset;
    public SkillDef skillDef;

    public HediffCompProperties_SkillOffset()
    {
        compClass = typeof(HediffComp_SkillOffset);
    }
}