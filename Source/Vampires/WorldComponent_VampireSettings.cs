using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Vampire
{
    public class WorldComponent_VampireSettings : WorldComponent
    {
        public GameMode mode = GameMode.Disabled;
        public bool aiToggle = true;
        public float spawnPct = 0.0f;
        public int lowestActiveVampGen = 7;
        public bool eventsEnabled = false;
        public float sunDimming = 0.0f;
        public bool settingsWindowSeen = false;
        public bool scenPartLongerNights = false;
        public bool scenPartLongerNightsFirstCheck = false;
        public bool bloodPointRulesFirstCheck = false;
        public float scenPartLongerNightsLength = 0;
        public Dictionary<ThingDef, int> bloodPointRules = new Dictionary<ThingDef, int>();

        public WorldComponent_VampireSettings(World world) : base(world)
        {
        }

        public void ToggleAI()
        {
            switch (aiToggle)
            {
                case true:
                    aiToggle = false;
                    Log.Message("Vampire AI disabled.");
                    Messages.Message("ROMV_VampireAIDisabled".Translate(), null, MessageTypeDefOf.TaskCompletion);
                    break;
                case false:
                    aiToggle = true;
                    Log.Message("Vampire AI enabled.");
                    Messages.Message("ROMV_VampireAIEnabled".Translate(), null, MessageTypeDefOf.TaskCompletion);
                    break;
            }

        }

        public void ApplySettings()
        {
            switch (mode)
            {
                case GameMode.Disabled:
                    spawnPct = 0.0f;
                    lowestActiveVampGen = 7;
                    eventsEnabled = false;
                    sunDimming = 0f;
                    break;
                case GameMode.EventsOnly:
                    spawnPct = 0.0f;
                    lowestActiveVampGen = 7;
                    eventsEnabled = true;
                    sunDimming = 0f;
                    break;
                case GameMode.Standard:
                    spawnPct = 0.05f;
                    lowestActiveVampGen = 7;
                    eventsEnabled = true;
                    sunDimming = 0.1f;
                    break;
                case GameMode.Custom:
                    break;
            }
            Log.Message("Vampire settings applied.");
            Messages.Message("ROMV_VampireSettingsApplied".Translate(), null, MessageTypeDefOf.TaskCompletion);
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mode, "mode");
            Scribe_Values.Look(ref aiToggle, "aiToggle");
            Scribe_Values.Look(ref spawnPct, "spawnPct", 0f);
            Scribe_Values.Look(ref lowestActiveVampGen, "lowestActiveVampGen", 7);
            Scribe_Values.Look(ref eventsEnabled, "eventsEnabled");
            Scribe_Values.Look(ref sunDimming, "sunDimming");
            Scribe_Values.Look(ref settingsWindowSeen, "settingsWindowSeen");
        }
    }
}