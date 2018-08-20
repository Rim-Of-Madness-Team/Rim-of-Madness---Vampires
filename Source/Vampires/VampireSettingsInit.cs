using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class VampireSettingsInit
    {

        public static GameMode mode = GameMode.Standard;
        public static float spawnPct = 0.05f;
        public static int lowestActiveVampGen = 7;

    }
    
    public enum GameMode
    {
        Disabled = 0,
        EventsOnly = 1,
        Standard = 2,
        Custom = 3
    }

}