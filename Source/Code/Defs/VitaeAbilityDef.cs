using System.Collections.Generic;
using AbilityUser;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// Used externally in .xml files.
// Uses RimWorld naming system.

namespace Vampire;

public class VitaeAbilityDef : AbilityDef
{
    public int bloodCost;
    public List<string> tags;
}

public class VitaeAbilityDefLevel
{
    public VitaeAbilityDef def;
    public int level;
}