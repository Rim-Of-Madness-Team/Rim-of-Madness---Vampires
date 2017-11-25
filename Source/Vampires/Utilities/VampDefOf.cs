using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    [DefOf]
    public static class VampDefOf
    {
        public static HediffDef ROM_Vampirism; //Vampires have no inner organs.
        public static HediffDef ROMV_SunExposure;
        public static HediffDef ROMV_Fangs;
        public static HediffDef ROMV_TheBeast;

        public static VitaeAbilityDef ROMV_RegenerateLimb;
        public static VitaeAbilityDef ROMV_VampiricHealing;

        public static DamageDef ROMV_Drain;

        public static HediffDef ROMV_SleepHediff;
        public static HediffDef ROMV_PossessionHediff;
        public static HediffDef ROMV_BatFormHediff;
        public static HediffDef ROMV_FeralClaw;
        public static HediffDef ROMV_MistFormHediff;

        public static HediffDef ROMV_NightwispRavens;

        public static HediffDef ROMV_PerfectFormHediff;
        public static HediffDef ROMV_ZuloFormHediff;
        public static HediffDef ROMV_WarFormHediff;
        public static HediffDef ROM_VampirismRandom;
        public static HediffDef ROM_VampirismGargoyle;
        public static HediffDef ROM_VampirismLasombra;
        public static HediffDef ROM_VampirismTzimisce;
        public static HediffDef ROM_VampirismTremere;
        public static HediffDef ROM_VampirismPijavica;

        public static HediffDef ROMV_CorruptFormHediff_Sight;
        public static HediffDef ROMV_CorruptFormHediff_Legs;
        public static HediffDef ROMV_CorruptFormHediff_Arms;

        public static HediffDef ROMV_MindReadingHediff;
        public static HediffDef ROMV_HeightenedSensesHediff;
        public static HediffDef ROMV_CrocodileTongueHediff;
        public static HediffDef ROMV_BlackMetamorphosisHediff;
        public static HediffDef ROMV_TenebrousFormHediff;
        public static HediffDef ROMV_BloodShieldHediff;


        public static RecipeDef ROMV_ExtractBloodVial;
        public static RecipeDef ROMV_ExtractBloodPack;

        public static ThingDef ROMV_HideyHole;
        public static ThingDef ROMV_RoyalCoffin;

        public static ThingDef BloodVial_LowBlood;
        public static ThingDef BloodVial_AverageBlood;
        public static ThingDef BloodVial_HighBlood;
        public static ThingDef BloodVial_Special;

        public static ThingDef BloodPack_Animal;
        public static ThingDef BloodPack_LowBlood;
        public static ThingDef BloodPack_AverageBlood;
        public static ThingDef BloodPack_HighBlood;

        public static ThoughtDef ROMV_MyBloodHarvested;
        public static ThoughtDef ROMV_KnowGuestBloodHarvested;
        public static ThoughtDef ROMV_KnowGuestDiedOfBloodLoss;
        public static ThoughtDef ROMV_KnowColonistDiedOfBloodLoss;
        public static ThoughtDef ROMV_IWasBittenByAVampire;
        public static ThoughtDef ROMV_IGaveTheKiss;
        public static ThoughtDef ROMV_IConsumedASoul;
        public static ThoughtDef ROMV_ITastedASoul;

        public static ThoughtDef ROMV_WitnessedVampireFeeding;
        public static ThoughtDef ROMV_WitnessedVampireFeedingVisitor;
        public static ThoughtDef ROMV_WitnessedVampireEmbrace;
        public static ThoughtDef ROMV_WitnessedVampireEmbraceVisitor;
        public static ThoughtDef ROMV_WitnessedVampireDiablerie;
        public static ThoughtDef ROMV_WitnessedVampireDiablerieVisitor;

        public static MentalStateDef ROMV_Rotschreck;

        public static ThingDef ROMV_MonstrosityRace;
        public static ThingDef ROMV_BatSpectralRace;
        public static ThingDef ROMV_BloodMistRace;
        public static PawnKindDef ROMV_BloodMistKind;
        public static PawnKindDef ROMV_MonstrosityA;
        public static PawnKindDef ROMV_WolfSpectral;
        public static PawnKindDef ROMV_BatSpectralKind;
        public static PawnKindDef ROMV_AbyssalArmKind;

        public static PawnRelationDef ROMV_Sire;
        public static PawnRelationDef ROMV_Childe;
        //public static PawnRelationDef ROMV_BloodBond_Regnant;
        //public static PawnRelationDef ROMV_BloodBond_ThrallUnknowing;
        //public static PawnRelationDef ROMV_BloodBond_ThrallFaithful;
        //public static PawnRelationDef ROMV_BloodBond_ThrallFaithful;

        public static FactionDef ROMV_LegendaryVampires;
        public static FactionDef ROMV_Camarilla;
        public static FactionDef ROMV_Sabbat;
        public static FactionDef ROMV_Anarch;

        public static BloodlineDef ROMV_Caine;
        public static BloodlineDef ROMV_TheThree;
        public static BloodlineDef ROMV_ClanTzimize;
        public static BloodlineDef ROMV_ClanTremere;
        public static BloodlineDef ROMV_ClanLasombra;
        public static BloodlineDef ROMV_ClanPijavica;
        public static BloodlineDef ROMV_ClanGargoyle;

        public static JobDef ROMV_Feed;
        public static JobDef ROMV_Sip;
        public static JobDef ROMV_FeedAndReturn;
        public static JobDef ROMV_FeedAndDestroy;
        public static JobDef ROMV_BloodVomit;
        public static JobDef ROMV_FeedVampire;
        public static JobDef ROMV_ConsumeBlood;
        public static JobDef ROMV_Embrace;
        public static JobDef ROMV_EnterTorpor;
        public static JobDef ROMV_DigAndHide;
        public static JobDef ROMV_Diablerie;
    }
}
