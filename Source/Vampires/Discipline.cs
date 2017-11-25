using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using AbilityUser;

namespace Vampire
{
    public class Discipline : IExposable
    {
        private int level = 0;
        private int points = 0;
        public int Level { get => level; set => level = value; }
        public int Points { get => points; set => points = value; }

        private DisciplineDef def;
        public DisciplineDef Def => def;
        
        private List<VitaeAbilityDef> availableAbilities = null;
        public List<VitaeAbilityDef> AvailableAbilities
        {
            get
            {
                if (availableAbilities == null)
                {
                    availableAbilities = new List<VitaeAbilityDef>();
                    for (int i = 0; i < level; i++)
                    {
                        availableAbilities.Add(def.abilities[i]);
                    }
                }
                return availableAbilities;
            }
        }

        public bool IsVisible => level > 0;

        public int prevPoints = 0;
        public int NextLevelThreshold => prevPoints;

        public void UpdateAbilities()
        {
            availableAbilities = null;
        }

        public void Notify_Reset(Pawn p)
        {
            p.VampComp().AbilityPoints += this.points;
            this.points = 0;
            this.level = 0;
            UpdateAbilities();
        }

        public void Notify_PointsInvested(int amt)
        {
            ++points;
            if (points > NextLevelThreshold)
            {
                ++level;
                prevPoints = level + points;
                //Log.Message("Level up : " + level.ToString());
            }
            UpdateAbilities();
        }

        public Discipline()
        {

        }

        public Discipline(DisciplineDef def)
        {
            this.def = def;
        }

        //public Discipline(int level, List<VitaeAbilityDef> abilities)
        //{
        //    this.level = level;
        //    this.abilities = abilities;
        //}

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.level, "level");
            Scribe_Values.Look<int>(ref this.points, "points");
            Scribe_Values.Look<int>(ref this.prevPoints, "prevPoints");
            Scribe_Defs.Look<DisciplineDef>(ref this.def, "def");
        }
    }
}
