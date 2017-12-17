using Verse;

namespace Vampire.Hediffs
{
    public class HediffComp_SkillOffset : HediffComp
    {
        public HediffCompProperties_SkillOffset Props
        {
            get
            {
                return (HediffCompProperties_SkillOffset)props;
            }

        }
        public override string CompTipStringExtra
        {
            get
            {
                return Props.skillDef.LabelCap + ": " + ((Props.offset >= 0) ? "+" : "") + Props.offset.ToString();
            }
        }

    }
}
