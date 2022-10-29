using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

public class DigHoleAttempt
{
    public DigHoleAttempt(int ticksForAttempt, float workLeft)
    {
        TicksSinceAttempt = ticksForAttempt;
        this.WorkLeft = workLeft;
    }

    public float WorkLeft { get; set; } = -1000f;

    public int TicksSinceAttempt { get; set; } = int.MinValue;

    public bool ShouldUseData()
    {
        if (TicksSinceAttempt + GenDate.TicksPerHour > Find.TickManager.TicksGame)
            return true;
        return false;
    }
}

public partial class HarmonyPatches
{
    public static Dictionary<Pawn, DigHoleAttempt> DigHoleAttempts = new();
}