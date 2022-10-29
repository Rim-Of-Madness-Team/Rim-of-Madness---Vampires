using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Vampire;

public class BloodlineDef : Def
{
    public bool allowsHair = true;
    public List<VitaeAbilityDef> bloodlineAbilities = null;
    public HediffDef bloodlineHediff = null;
    public bool canFeedOnVampires = false;
    public BloodPreferabilty desperateBloodPref = BloodPreferabilty.Any;
    public List<DisciplineDef> disciplines = null;
    public Type embraceWorker;

    private EmbraceWorker embraceWorkerInt;
    public HediffDef fangsHediff = VampDefOf.ROMV_Fangs;
    public string headGraphicsPath = "";
    public BloodPreferabilty minBloodPref = BloodPreferabilty.Lowblood;
    public string nakedBodyGraphicsPath = "";
    public bool onlyRestsInCoffins = false;
    public bool scenarioCanAdd = true;
    public List<Color> skinColors = null;

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