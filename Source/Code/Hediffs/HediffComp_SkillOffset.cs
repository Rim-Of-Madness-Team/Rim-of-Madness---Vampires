using Verse;

namespace Vampire;

public class HediffComp_SkillOffset : HediffComp
{
    public HediffCompProperties_SkillOffset Props => (HediffCompProperties_SkillOffset)props;

    public override string CompTipStringExtra =>
        Props.skillDef.LabelCap + ": " + (Props.offset >= 0 ? "+" : "") + Props.offset.ToString();
}