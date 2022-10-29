using System.Text;
using Verse;

namespace Vampire;

public class HediffComp_ReadMind : HediffComp
{
    public Pawn MindBeingRead { get; set; }

    public override bool CompShouldRemove
    {
        get
        {
            if (MindBeingRead == null || MindBeingRead.Dead) return true;
            return base.CompShouldRemove;
        }
    }

    public override string CompTipStringExtra
    {
        get
        {
            var s = new StringBuilder();
            s.Append(base.CompTipStringExtra);
            var curMind = MindBeingRead == null ? "" : MindBeingRead.Label;
            s.AppendLine("ROMV_MindBeingRead".Translate(curMind));
            return s.ToString();
        }
    }
}