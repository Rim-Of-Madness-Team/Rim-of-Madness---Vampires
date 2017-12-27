using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire
{
    
    public enum BondStage : int
    {
        None = 0,
        FirstTaste = 1,
        SecondTaste = 2,
        Thrall = 3
    }
    
    public class ThrallData : IExposable
    {
        private int lastVitaeDrinkTick = -1;
        private Pawn regnant = null;
        private Pawn pawn = null;
        private BondStage bondStage = BondStage.None;

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
                BondStage curStage = bondStage;
                if (value > curStage)
                {
                    Messages.Message("ROMV_BloodBondImproved".Translate(new object[]{this.pawn.LabelShort, this.regnant.LabelShort}), MessageTypeDefOf.PositiveEvent);
                }
                else if (value < curStage)
                {
                    Messages.Message("ROMV_BloodBondImproved".Translate(new object[]{this.pawn.LabelShort, this.regnant.LabelShort}), MessageTypeDefOf.PositiveEvent);
                    
                }
                bondStage = value;
            }
        }

        private bool ShouldFadeBond =>  lastVitaeDrinkTick != -1 && lastVitaeDrinkTick + GenDate.TicksPerSeason < Find.TickManager.TicksGame;
        public void CheckBondStage()
        {
            if (ShouldFadeBond)
                TryAdjustBondStage(null, -1);
        }

        public bool ShouldFeedBlood => lastVitaeDrinkTick != -1 &&
                                         lastVitaeDrinkTick + (GenDate.TicksPerSeason / 2) < Find.TickManager.TicksGame;

        public bool TryAdjustBondStage(Pawn bonder, int value, bool showMessage = false)
        {
            if (value > 0)
            {
                if (lastVitaeDrinkTick != -1 && lastVitaeDrinkTick + GenDate.TicksPerDay > Find.TickManager.TicksGame)
                {
                    if (showMessage && pawn != null)
                    {
                        Messages.Message("ROMV_FailedToAdjustBondStage".Translate(pawn), new GlobalTargetInfo(pawn), MessageTypeDefOf.RejectInput);
                        LetterMaker.MakeLetter("ROMV_FailedToAdjustBondStageLabel".Translate(),
                            "ROMV_FailedToAdjustBondStageDesc".Translate(new object[] {pawn, bonder}),
                            LetterDefOf.NeutralEvent, new GlobalTargetInfo(pawn));
                    }
                    return false;                
                }
                if (showMessage && regnant != null && bonder != regnant)
                {
                    Messages.Message("ROMV_FailedToAdjustBondStage".Translate(pawn), new GlobalTargetInfo(pawn), MessageTypeDefOf.RejectInput);
                    LetterMaker.MakeLetter("ROMV_FailedToAdjustBondStageLabel".Translate(),
                        "ROMV_FailedToAdjustBondStageDescTwo".Translate(new object[] {pawn, bonder, regnant}),
                        LetterDefOf.NeutralEvent, new GlobalTargetInfo(pawn));
                    return false;
                }
                lastVitaeDrinkTick = Find.TickManager.TicksGame;
                if (this.regnant == null) this.regnant = bonder;
                if (this.pawn != null && !regnant.VampComp().Ghouls.Contains(this.pawn))
                    regnant.VampComp().Ghouls.Add(this.pawn);
            }
            if (this.regnant == null && bonder != null) this.regnant = bonder;
            AdjustBondStage(value);
            if (this.BondStage == BondStage.None)
            {
                this.pawn.VampComp().Notify_DeGhouled();
            }
            return true;
        }

        public void AdjustBondStage(int value)
        {
            int stageVal = (int)bondStage;
            if (stageVal + value <= 0)
            {
                BondStage = BondStage.None;
            }
            else if (stageVal + value == 1)
            {
                BondStage = BondStage.FirstTaste;
            }
            else if (stageVal + value == 2)
            {
                BondStage = BondStage.SecondTaste;
            }
            else
            {
                BondStage = BondStage.Thrall;
            }
        }

        public ThrallData()
        {
        }

        public ThrallData(Pawn pawn, bool setVitaeDrinkTick = false)
        {
            this.pawn = pawn;
            if (setVitaeDrinkTick)
                lastVitaeDrinkTick = Find.TickManager.TicksGame;
        }
        
        public ThrallData(Pawn pawn, Pawn regnant, BondStage bondStage = BondStage.None, bool setVitaeDrinkTick = false)
        {
            this.pawn = pawn;
            this.regnant = regnant;
            this.bondStage = bondStage;
            if (setVitaeDrinkTick)
                lastVitaeDrinkTick = Find.TickManager.TicksGame;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn");
            Scribe_References.Look<Pawn>(ref this.regnant, "regnant");
            Scribe_Values.Look<int>(ref this.lastVitaeDrinkTick, "lastVitaeDrinkTick", -1);
            Scribe_Values.Look<BondStage>(ref this.bondStage, "bondStage", BondStage.None);
        }
    }
}