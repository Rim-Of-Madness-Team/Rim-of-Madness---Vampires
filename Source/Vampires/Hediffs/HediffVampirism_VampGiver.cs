using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public class HediffVampirism_VampGiver : HediffWithComps
    {
        public virtual BloodlineDef Bloodline => VampireUtility.RandBloodline;
        private int generation = -1;
        public virtual int Generation
        {
            get
            {
                if (generation == -1)
                {
                    switch (this.CurStageIndex)
                    {
                        case 0: generation = 14; break;
                        case 1: generation = Rand.Range(10, 13); break;
                        case 2: generation = Rand.Range(7, 9); break;
                        case 3: generation = Rand.Range(5, 6); break;
                    }
                }
                return generation;
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                return "ROMV_HI_Generation".Translate(HediffVampirism.AddOrdinal(Generation));
            }
        }

        public override string TipStringExtra
        {
            get
            {
                int gen = this.generation;
                int math = (gen > 7) ? 10 + (Math.Abs(gen - 13)) : 10 * (Math.Abs(gen - 9));

                StringBuilder s = new StringBuilder();
                s.AppendLine(Bloodline.LabelCap);
                s.AppendLine(Bloodline.description);
                s.AppendLine("---");
                s.AppendLine("ROMV_Disciplines".Translate());
                if (Bloodline.disciplines is List<DisciplineDef> dDefs && !dDefs.NullOrEmpty())
                {
                    foreach (DisciplineDef dDef in dDefs)
                    {
                        s.AppendLine(" -" + dDef.LabelCap);
                    }
                }
                s.AppendLine("---");
                s.AppendLine("ROMV_HI_VitaeAvailable".Translate(math));
                return s.ToString();
            }
        }

        public bool setup = false;

        public override void PostTick()
        {
            if (!setup)
            {
                bool setup = true;
                if (this.pawn.VampComp() is CompVampire v)
                {
                    int generatonToSpawn = this.Generation;
                    //Pawn sire = VampireRelationUtility.FindSireFor(this.pawn, this.Bloodline, generatonToSpawn);
                    v.InitializeVampirism(null, this.Bloodline, generatonToSpawn);
                }
            }
            base.PostTick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.setup, "setup", false);
            Scribe_Values.Look<int>(ref this.generation, "generation", -1);
        }
    }
}
