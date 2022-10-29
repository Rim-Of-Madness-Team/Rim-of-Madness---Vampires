using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

public enum BondStage
{
    None = 0,
    FirstTaste = 1,
    SecondTaste = 2,
    Thrall = 3
}

public class ThrallData : IExposable
{
    private BondStage bondStage = BondStage.None;
    private int lastBondAttempt = -1;
    private int lastVitaeDrinkTick = -1;
    private Pawn pawn;
    private Pawn regnant;

    public ThrallData()
    {
    }

    public ThrallData(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public ThrallData(Pawn pawn, Pawn regnant, BondStage bondStage = BondStage.None, bool setVitaeDrinkTick = false)
    {
        this.pawn = pawn;
        this.regnant = regnant;
        this.bondStage = bondStage;
        if (setVitaeDrinkTick)
            lastVitaeDrinkTick = Find.TickManager.TicksGame;
    }

    private bool ShouldFadeBond => lastVitaeDrinkTick != -1 &&
                                   lastVitaeDrinkTick + GenDate.TicksPerSeason < Find.TickManager.TicksGame;

    public bool ShouldFeedBlood => pawn != null && !pawn.Dead && !pawn.Downed && !pawn.IsFighting() &&
                                   !pawn.InMentalState && lastVitaeDrinkTick != -1 &&
                                   lastVitaeDrinkTick + GenDate.TicksPerSeason / 2 < Find.TickManager.TicksGame;

    public Pawn Regnant
    {
        get => regnant;
        set => regnant = value;
    }

    public BondStage BondStage
    {
        get => bondStage;
        set
        {
            var curStage = bondStage;
            if (value > curStage)
                Messages.Message(
                    "ROMV_BloodBondImproved".Translate(new object[] { pawn.LabelShort, regnant.LabelShort }),
                    MessageTypeDefOf.PositiveEvent);
            else if (value < curStage)
                Messages.Message(
                    "ROMV_BloodBondStrained".Translate(new object[] { pawn.LabelShort, regnant.LabelShort }),
                    MessageTypeDefOf.PositiveEvent);
            bondStage = value;
        }
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_References.Look(ref regnant, "regnant");
        Scribe_Values.Look(ref lastVitaeDrinkTick, "lastVitaeDrinkTick", -1);
        Scribe_Values.Look(ref lastBondAttempt, "lastBondAttempt", -1);
        Scribe_Values.Look(ref bondStage, "bondStage");
    }

    public bool TryAdjustBondStage(Pawn bonder, int value, bool showMessage = false)
    {
        if (value > 0) Notify_DrankBlood(bonder);
        if (regnant == null && bonder != null) regnant = bonder;
        if (!ShouldAdjustBondStage(bonder, value, showMessage)) return false;
        lastBondAttempt = Find.TickManager.TicksGame;
        AdjustBondStage(value);
        ThrallInteraction();
        return true;
    }

    public void CheckBondStage()
    {
        if (ShouldFadeBond)
            TryAdjustBondStage(null, -1);
    }

    private void Notify_DrankBlood(Pawn bonder)
    {
        lastVitaeDrinkTick = Find.TickManager.TicksGame;
        if (regnant == null) regnant = bonder;
        if (regnant != null)
            if (pawn != null && !regnant.VampComp().Ghouls.Contains(pawn))
                regnant.VampComp().Ghouls.Add(pawn);
    }

    private bool ShouldAdjustBondStage(Pawn bonder, int value, bool showMessage)
    {
        var result = true;
        Letter letter = null;

        if (value > 0)
        {
            if (lastBondAttempt != -1 && lastBondAttempt + GenDate.TicksPerDay > Find.TickManager.TicksGame)
            {
                if (!showMessage || pawn == null) return false;
/*                    Messages.Message("ROMV_FailedToAdjustBondStage".Translate(pawn), new GlobalTargetInfo(pawn),
                        MessageTypeDefOf.RejectInput);
                    letter = LetterMaker.MakeLetter("ROMV_FailedToAdjustBondStageLabel".Translate(),
                        "ROMV_FailedToAdjustBondStageDesc".Translate(pawn, bonder),
                        LetterDefOf.NeutralEvent, new GlobalTargetInfo(pawn));*/
                result = false;
            }
            else if (showMessage && regnant != null && bonder != regnant)
            {
/*                    Messages.Message("ROMV_FailedToAdjustBondStage".Translate(pawn), new GlobalTargetInfo(pawn),
                        MessageTypeDefOf.RejectInput);
                    letter = LetterMaker.MakeLetter("ROMV_FailedToAdjustBondStageLabel".Translate(),
                        "ROMV_FailedToAdjustBondStageDescTwo".Translate(pawn, bonder, regnant),
                        LetterDefOf.NeutralEvent, new GlobalTargetInfo(pawn));*/
                result = false;
            }
        }

        if (letter != null)
            Find.LetterStack.ReceiveLetter(letter);

        return result;
    }

    private void ThrallInteraction()
    {
        InteractionDef intDef = null;
        var rulePackDefList = new List<RulePackDef>();
        switch (BondStage)
        {
            case BondStage.None:

                return;
            case BondStage.FirstTaste:
            case BondStage.SecondTaste:
                intDef = VampDefOf.ROMV_VampireUnbondedDrink;
                break;
            case BondStage.Thrall:
                intDef = VampDefOf.ROMV_VampireBondedDrink;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (intDef == null) return;
        MoteMaker.MakeInteractionBubble(regnant, pawn, intDef.interactionMote, intDef.GetSymbol());
        if (this?.pawn?.needs?.mood?.thoughts?.memories is { } m)
            m.TryGainMemory(intDef.recipientThought, regnant);
        Find.PlayLog.Add(new PlayLogEntry_Interaction(intDef, regnant, pawn, rulePackDefList));
    }


    private void AdjustBondStage(int value)
    {
        var stageVal = (int)bondStage;
        if (stageVal + value <= 0)
            BondStage = BondStage.None;
        else
            switch (stageVal + value)
            {
                case 1:
                    BondStage = BondStage.FirstTaste;
                    break;
                case 2:
                    BondStage = BondStage.SecondTaste;
                    break;
                default:
                    BondStage = BondStage.Thrall;
                    break;
            }
    }
}