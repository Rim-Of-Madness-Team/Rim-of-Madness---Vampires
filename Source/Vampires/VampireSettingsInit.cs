using System;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class VampireSettingsInit
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

    }

    public enum GameMode
    {
        Disabled = 0,
        EventsOnly = 1,
        Standard = 2,
        Custom = 3
    }
}