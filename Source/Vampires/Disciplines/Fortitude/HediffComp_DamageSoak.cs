using System;
using RimWorld;
using Verse;
using System.Text;

namespace Vampire
{
    public class HediffComp_DamageSoak : HediffComp_Disappears
    {

        public new HediffCompProperties_DamageSoak Props
        {
            get
            {
                return (HediffCompProperties_DamageSoak)this.props;
            }
        }
        
        public override string CompTipStringExtra
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.Append(base.CompTipStringExtra);
                s.AppendLine("ROMV_HI_DamageSoaked".Translate(Props.damageToSoak));
                return s.ToString();
            }
        }
    }
}
