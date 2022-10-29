using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire;

public class WorldComponent_VampireSettings : WorldComponent
{
    public bool aiToggle = true;
    public Dictionary<ThingDef, int> bloodPointRules = new();
    public bool bloodPointRulesFirstCheck = false;
    public bool damageToggle = true;
    public bool eventsEnabled;
    public int lowestActiveVampGen = 7;
    public GameMode mode = GameMode.Disabled;
    public bool rainDamageToggle;
    public bool rainSlumberToggle;
    public bool scenPartLongerNights = false;
    public bool scenPartLongerNightsFirstCheck = false;
    public float scenPartLongerNightsLength = 0;
    public bool settingsWindowSeen;
    public bool slumberToggle = true;
    public float spawnPct;
    public float sunDimming;

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
        Scribe_Values.Look(ref aiToggle, "aiToggle", true);
        Scribe_Values.Look(ref slumberToggle, "slumberToggle", true);
        Scribe_Values.Look(ref damageToggle, "damageToggle", true);
        Scribe_Values.Look(ref rainDamageToggle, "rainWalkingToggle", true);
        Scribe_Values.Look(ref rainSlumberToggle, "rainSlumberToggle", true);
        Scribe_Values.Look(ref spawnPct, "spawnPct");
        Scribe_Values.Look(ref lowestActiveVampGen, "lowestActiveVampGen", 7);
        Scribe_Values.Look(ref eventsEnabled, "eventsEnabled");
        Scribe_Values.Look(ref sunDimming, "sunDimming");
        Scribe_Values.Look(ref settingsWindowSeen, "settingsWindowSeen");
    }
}