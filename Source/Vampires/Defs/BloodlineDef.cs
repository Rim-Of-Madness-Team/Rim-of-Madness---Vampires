using System;
using System.Collections.Generic;
using UnityEngine;
using Vampire.Utilities;
using Vampire.Workers;
using Verse;

namespace Vampire.Defs
{
    public class BloodlineDef : Def
    {
        public bool onlyRestsInCoffins = false;
        public HediffDef bloodlineHediff = null;
        public HediffDef fangsHediff = VampDefOf.ROMV_Fangs;
        public Type embraceWorker;
        public bool scenarioCanAdd = true;
        public bool canFeedOnVampires = false;
        public BloodPreferabilty minBloodPref = BloodPreferabilty.Lowblood;
        public BloodPreferabilty desperateBloodPref = BloodPreferabilty.Any;
        public List<DisciplineDef> disciplines = null;
        public List<VitaeAbilityDef> bloodlineAbilities = null;
        public string nakedBodyGraphicsPath = "";
        public string headGraphicsPath = "";
        public List<Color> skinColors = null;
        public bool allowsHair = true;

        private EmbraceWorker embraceWorkerInt = null;
        public EmbraceWorker EmbraceWorker
        {
            get
            {
                if (embraceWorkerInt == null)
                {
                    embraceWorkerInt = (EmbraceWorker)Activator.CreateInstance(embraceWorker);
                    embraceWorkerInt.def = this;
                }
                return embraceWorkerInt;
            }
        }

    }
}
