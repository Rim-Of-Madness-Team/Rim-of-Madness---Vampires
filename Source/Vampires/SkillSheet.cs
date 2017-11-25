using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    
    public class SkillSheet : IExposable
    {
        private Pawn pawn;
        private List<Discipline> disciplines;

        public Pawn Pawn { get { return pawn; } set { pawn = value; } }
        public List<Discipline> Disciplines => disciplines;

        public void InitializeDisciplines()
        {
            disciplines = new List<Discipline>();
            if (pawn.VampComp().Bloodline.disciplines is List<DisciplineDef> defs && !defs.NullOrEmpty())
            {
                foreach (DisciplineDef dd in defs)
                {
                    disciplines.Add(new Discipline(dd));
                    //Log.Message(dd.LabelCap);
                }
            }
        }

        public void ResetDisciplines()
        {
            if (!disciplines.NullOrEmpty())
            {
                foreach (Discipline d in disciplines)
                {
                    d.Notify_Reset(pawn);
                }
            }
        }

        public SkillSheet() { }

        public SkillSheet(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn");
            Scribe_Collections.Look<Discipline>(ref this.disciplines, true, "disciplines", LookMode.Deep, new object[0]);
        }
    }
}
