using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class HediffWithComps_BeastHunger : HediffWithComps
{
    public static MentalStateDef MentalState_VampireBeast = DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
    private readonly int checkRate = 240;
    private bool sentLetter;
    public int ticksRemaining = GenDate.TicksPerHour;

    public override void Tick()
    {
        base.Tick();

        if (Find.TickManager.TicksGame % checkRate == 0)
            if (pawn?.BloodNeed() is { } pB)
            {
                if (CurStageIndex == 3 && pawn.MentalStateDef != MentalState_VampireBeast)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true);
                    if (!sentLetter)
                    {
                        sentLetter = true;
                        Find.LetterStack.ReceiveLetter("ROMV_TheBeast".Translate(),
                            "ROMV_TheBeastDesc".Translate(pawn?.Label), VampDefOf.ROMV_FrenzyMessage, pawn);
                    }
                }

                if (pB.CurLevelPercentage < 0.3f)
                    Severity = Mathf.Clamp01(Severity + 0.005f);
                else
                    Severity -= 0.2f;
            }
    }
}