using System.Collections.Generic;
using Verse;

namespace Vampire;

public class DisciplineDef : Def
{
    public List<VitaeAbilityDef> abilities;
    public List<VitaeAbilityDefLevel> extraAbilities;
    public List<string> tags;
}