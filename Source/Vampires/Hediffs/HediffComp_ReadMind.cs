using System;
using RimWorld;
using Verse;
using System.Text;

namespace Vampire
{
    public class HediffComp_ReadMind : HediffComp
    {

        private Pawn mindBeingRead;
        public Pawn MindBeingRead { get => mindBeingRead; set => mindBeingRead = value; }
        
        public override bool CompShouldRemove
        {
            get
            {
                if (MindBeingRead == null || MindBeingRead.Dead)
                {
                    return true;
                }
                return base.CompShouldRemove;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.Append(base.CompTipStringExtra);
                string curMind = (MindBeingRead == null) ? "" : MindBeingRead.Label;
                s.AppendLine("ROMV_MindBeingRead".Translate(curMind));
                return s.ToString();
            }
        }
    }
}
