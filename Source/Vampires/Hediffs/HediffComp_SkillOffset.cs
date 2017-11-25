using System;
using RimWorld;
using Verse;

namespace Vampire
{
    public class HediffComp_SkillOffset : HediffComp
    {
        public HediffCompProperties_SkillOffset Props
        {
            get
            {
                return (HediffCompProperties_SkillOffset)this.props;
            }

        }
        public override string CompTipStringExtra
        {
            get
            {
                return this.Props.skillDef.LabelCap + ": " + ((this.Props.offset >= 0) ? "+" : "") + this.Props.offset.ToString();
            }
        }

    }
}
