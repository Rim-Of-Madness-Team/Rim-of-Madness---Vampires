using HarmonyLib;
using Verse;

namespace Vampire;

[StaticConstructorOnStartup]
static partial class HarmonyPatches
{
    public static bool VampireGenInProgress = false;

    static HarmonyPatches()
    {
        Harmony.DEBUG = false;

        var harmony = new Harmony("rimworld.jecrell.vampire");
        
        HarmonyPatches_Needs(harmony);

        HarmonyPatches_Ingestion(harmony);

        HarmonyPatches_Pathing(harmony);

        HarmonyPatches_Caravan(harmony);

        HarmonyPatches_Givers(harmony);

        HarmonyPatches_Beds(harmony);

        HarmonyPatches_Lovin(harmony);

        HarmonyPatches_Graphics(harmony);

        HarmonyPatches_UI(harmony);

        HarmonyPatches_Age(harmony);

        HarmonyPatches_AI(harmony);

        HarmonyPatches_Menu(harmony);

        HarmonyPatches_Thoughts(harmony);

        HarmonyPatches_Powers(harmony);

        HarmonyPatches_Graves(harmony);

        HarmonyPatches_Misc(harmony);

        HarmonyPatches_Mods(harmony);

        HarmonyPatches_Royalty(harmony);

        HarmonyPatches_Fixes(harmony);
    }
}