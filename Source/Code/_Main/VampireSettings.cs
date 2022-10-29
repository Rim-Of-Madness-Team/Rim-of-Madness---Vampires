using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Vampire;

[StaticConstructorOnStartup]
public static class VampireSettings
{
//        public static GameMode mode = GameMode.Standard;
//        public static float spawnPct = 0.05f;
//        public static int lowestActiveVampGen = 7;
//        public static bool eventsEnabled = true;
//        public static float sunDimming = 0.1f;

    public static bool ShouldUseSettings => Find.Scenario.AllParts.FirstOrDefault(x =>
        x is ScenPart_ForcedHediff y && Traverse.Create(y)
            .Field("hediff").GetValue<HediffDef>() ==
        VampDefOf.ROM_Vampirism) == null;

    public static WorldComponent_VampireSettings Get => Find.World.GetComponent<WorldComponent_VampireSettings>();

    public static bool IsScenPartLongerNightsLoaded()
    {
        if (Get.scenPartLongerNightsFirstCheck == false)
        {
            Get.scenPartLongerNightsFirstCheck = true;
            if (Find.Scenario?.AllParts?.FirstOrDefault(x => x.def.scenPartClass == typeof(ScenPart_LongerNights)) is
                ScenPart_LongerNights p)
            {
                Get.scenPartLongerNightsLength = p.nightsLength;
                Get.scenPartLongerNights = true;
            }
        }

        return Get.scenPartLongerNights;
    }

    public static int GetBloodPointOverride(ThingDef def)
    {
        //Initial GetBloodPoints check
        if (Get.bloodPointRulesFirstCheck == false)
        {
            Get.bloodPointRulesFirstCheck = true;
            Get.bloodPointRules = new Dictionary<ThingDef, int>();
            if (DefDatabase<BloodPointCountRules>.GetNamedSilentFail("BloodPointCountRules") is { } r)
                foreach (var rule in r.rules)
                    if (ThingDef.Named(rule.def) is { } t)
                        Get.bloodPointRules.Add(t, rule.blood);
        }

        //Use the BloodPointCountRules
        if (Get.bloodPointRules.ContainsKey(def))
            return Get.bloodPointRules[def];

        return -1;
    }
}

public enum GameMode
{
    Disabled = 0,
    EventsOnly = 1,
    Standard = 2,
    Custom = 3
}

public enum AIMode
{
    Enabled = 1,
    Disabled = 0
}