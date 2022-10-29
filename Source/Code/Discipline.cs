using System.Collections.Generic;
using Verse;

namespace Vampire;

public class Discipline : IExposable
{
    private List<VitaeAbilityDef> availableAbilities;

    private DisciplineDef def;
    private int level;
    private int points;

    public int prevPoints;

    public Discipline()
    {
    }

    public Discipline(DisciplineDef def)
    {
        this.def = def;
    }

    public int Level
    {
        get => level;
        set => level = value;
    }

    public int Points
    {
        get => points;
        set => points = value;
    }

    public DisciplineDef Def => def;

    public List<VitaeAbilityDef> AvailableAbilities
    {
        get
        {
            if (availableAbilities == null)
                availableAbilities = new List<VitaeAbilityDef>();
            for (var i = 0; i < level; i++)
            {
                availableAbilities.Add(def.abilities[i]);
                if (!Def.extraAbilities.NullOrEmpty() &&
                    def.extraAbilities.FindAll(x => x.level == i) is { } vadlList)
                    foreach (var vadl in vadlList)
                        availableAbilities.Add(vadl.def);
            }

            return availableAbilities;
        }
    }

    public bool IsVisible => level > 0;
    public int NextLevelThreshold => prevPoints;

    //public Discipline(int level, List<VitaeAbilityDef> abilities)
    //{
    //    this.level = level;
    //    this.abilities = abilities;
    //}

    public void ExposeData()
    {
        Scribe_Values.Look(ref level, "level");
        Scribe_Values.Look(ref points, "points");
        Scribe_Values.Look(ref prevPoints, "prevPoints");
        Scribe_Defs.Look(ref def, "def");
    }

    public void UpdateAbilities()
    {
        availableAbilities = null;
    }

    public void Notify_Reset(Pawn p)
    {
        p.VampComp().AbilityPoints += points;
        points = 0;
        level = 0;
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
}