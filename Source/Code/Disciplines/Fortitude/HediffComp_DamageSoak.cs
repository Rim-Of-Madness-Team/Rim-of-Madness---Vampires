using System.Text;
using Verse;

namespace Vampire;

public class HediffComp_DamageSoak : HediffComp_Disappears
{
    public new HediffCompProperties_DamageSoak Props => (HediffCompProperties_DamageSoak)props;

    public override string CompTipStringExtra
    {
        get
        {
            var s = new StringBuilder();
            s.Append(base.CompTipStringExtra);
            s.AppendLine("ROMV_HI_DamageSoaked".Translate(Props.damageToSoak));
            return s.ToString();
        }
    }
}