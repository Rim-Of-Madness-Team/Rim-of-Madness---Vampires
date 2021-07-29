// ReSharper disable All
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RimWorld;
using Verse;
using AbilityUser;
using Verse.AI;

namespace Vampire
{
    public enum VampState : int
    {
        None = 0,
        Forming = 1,
        Active = 2,
        Torpor = 3,
        FinalDeath = 4
    }

    public enum SunlightPolicy : int
    {
        Relaxed = 0,
        Restricted = 1,
        NoAI = 2
    }

    public class CompVampire : CompAbilityUser
    {
        #region Variables

        private Pawn sire = null;
        private List<Pawn> childer = null;
        private List<Pawn> ghouls = null;
        private List<Pawn> souls = new List<Pawn>();
        private List<Pawn> inferiorBonded = new List<Pawn>();
        private SkillSheet sheet = null;
        private BloodlineDef bloodline = null;
        private ThrallData thrallData = null;
        private int generation = -1;
        private int level = 0;
        private int xp = 0;
        private int abilityPoints = 0;
        private TransformationDef currentForm = null;
        private SunlightPolicy curSunlightPolicy = SunlightPolicy.Restricted;

        public int ticksToLearnXP = -1;
        private int vampLastHomeCheck = -1;
        private IntVec3? vampLastHomePoint = null;

        /// Storing variables for Animal Transformations
        public int atCurIndex = 0;

        public int atCurTicks = -1;
        public bool atDirty = false;

        #endregion Variables

        #region Access Properties

        public List<Pawn> Ghouls
        {
            get => ghouls ?? (ghouls = new List<Pawn>());
            set => ghouls = value;
        }

        public SunlightPolicy CurrentSunlightPolicy
        {
            get => curSunlightPolicy;
            set => curSunlightPolicy = value;
        }

        public int VampLastHomeCheck
        {
            get => vampLastHomeCheck;
            set => vampLastHomeCheck = value;
        }

        public IntVec3 VampLastHomePoint
        {
            get
            {
                if (vampLastHomePoint == null || vampLastHomeCheck < Find.TickManager.TicksGame)
                {
                    vampLastHomeCheck = Find.TickManager.TicksGame + 500;
                    vampLastHomePoint = VampSunlightPathUtility.DetermineHomePoint(Pawn);
                }
                return vampLastHomePoint.Value;
            }
        }

        public int Level
        {
            get => level;
            set
            {
                if (value > level && value != 0)
                {
                    abilityPoints++;
                    if (XP < ((value * 600) * ((IsGhoul) ? 2 : 1)))
                    {
                        XP = ((value * 600) * ((IsGhoul) ? 2 : 1));
                    }
                }
                else if (value == 0)
                    XP = 0;
                level = value;
            }
        }

        public int XP
        {
            get => xp;
            set
            {
                xp = value;
                if (xp > 0 && xp > XPTillNextLevel)
                    Notify_LevelUp(true);
            }
        }

        public float XPLastLevel
        {
            get
            {
                float result = 0f;
                if (level > 0) result = (level * 600) * ((IsGhoul) ? 2 : 1);
                return result;
            }
        }

        public float XPTillNextLevelPercent => (float)(xp - XPLastLevel) / (float)(XPTillNextLevel - XPLastLevel);
        public int XPTillNextLevel => ((level + 1) * 600) * ((IsGhoul) ? 2 : 1);

        public int AbilityPoints
        {
            get => abilityPoints;
            set => abilityPoints = value;
        }

        public bool Transformed => currentForm != null;

        public TransformationDef CurrentForm
        {
            get => currentForm;
            set => currentForm = value;
        }

        private Graphic curFormGraphic = null;
        private bool beenGhoulBefore = false;

        public bool BeenGhoulBefore
        {
            get => beenGhoulBefore;
            set => beenGhoulBefore = value;
        }

        public Graphic CurFormGraphic
        {
            get => curFormGraphic;
            set => curFormGraphic = value;
        }

        public SkillSheet Sheet
        {
            get
            {
                if (sheet == null)
                {
                    sheet = new SkillSheet(Pawn);
                }
                return sheet;
            }
        }

        public Pawn Sire
        {
            get => sire;
            set => sire = value;
        }

        public ThrallData ThrallData
        {
            get => thrallData;
            set => thrallData = value;
        }

        public List<Pawn> Childer
        {
            get
            {
                if (childer == null)
                {
                    childer = new List<Pawn>();
                    if (AbilityUser?.relations?.DirectRelations is List<DirectPawnRelation> rels)
                    {
                        foreach (DirectPawnRelation rel in rels)
                        {
                            if (rel.def == VampDefOf.ROMV_Childe)
                            {
                                childer.Add(rel.otherPawn);
                            }
                        }
                    }
                }
                return childer;
            }
        }
        public List<Pawn> Souls
        {
            get
            {
                if (souls == null)
                {
                    souls = new List<Pawn>();
                }
                return souls;
            }
        }

        public BloodlineDef Bloodline
        {
            get => (IsGhoul) ? GhoulHediff.bloodline : bloodline;
            set => bloodline = value;
        }

        public int Generation
        {
            get => generation;
            set => generation = value;
        }

        public bool Thinblooded => generation > 13;
        public Need_Blood BloodPool => AbilityUser?.needs?.TryGetNeed<Need_Blood>() ?? null;

        public float TrueCombatPower
        {
            get
            {
                float result = 0;
                result += AbilityUser.kindDef.combatPower;
                result += 4000;
                for (int i = 1; i <= generation; i++)
                {
                    if (i == 2)
                    {
                        result -= 3000;
                        continue;
                    }
                    if (i < 7)
                    {
                        result -= 100;
                        continue;
                    }
                    result -= 5;
                }
                return result;
            }
        }

        public bool IsVampire => AbilityUser?.health?.hediffSet?.HasHediff(VampDefOf.ROM_Vampirism) ?? false;

        public bool IsGhoul =>
            !IsVampire && (AbilityUser?.health?.hediffSet?.HasHediff(VampDefOf.ROM_GhoulHediff) ?? false);

        public bool InSunlight
        {
            get
            {
                if (AbilityUser.Spawned)
                {
                    Map curMap = AbilityUser.Map;
                    if (curMap != null)
                    {
                        if (VampireUtility.IsDaylight(AbilityUser)
                            && !AbilityUser.PositionHeld.Roofed(curMap))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsRevenant =>
            IsGhoul && (AbilityUser?.health?.hediffSet?.GetFirstHediffOfDef(VampDefOf.ROM_GhoulHediff) is HediffGhoul hediffGhoul && hediffGhoul.ghoulType == GhoulType.Revenant);

        #endregion Access Properties

        #region Utility Props

        public Need_Blood Blood => this?.Pawn?.needs?.TryGetNeed<Need_Blood>();
        public Need_Blood BloodOther(Pawn other) => other?.needs?.TryGetNeed<Need_Blood>();

        #endregion Utility Props

        #region Methods

        public void Notify_LevelUp(bool sendNotification)
        {
            if (XP <= 0) XP = 1;
            Level++;
            if (sendNotification && (IsVampire || IsGhoul) && AbilityUser != null && AbilityUser.Spawned &&
                AbilityUser.Faction == Faction.OfPlayerSilentFail)
                Messages.Message((IsVampire) ? "ROMV_LevelUp".Translate(AbilityUser) : "ROMV_LevelUpGhoul".Translate(AbilityUser),
                    new RimWorld.Planet.GlobalTargetInfo(AbilityUser),
                    DefDatabase<MessageTypeDef>.GetNamed("ROMV_VampireNotifaction"));
        }

        public void Notify_ResetAbilities()
        {
            Sheet.ResetDisciplines();
        }

        public void Notify_UpdateAbilities()
        {
            if (!AbilityUser.IsVampire() && !AbilityUser.IsGhoul())
                return;

            //Disciplines Skill Sheet
            if (Sheet?.Disciplines is List<Discipline> dd && !dd.NullOrEmpty())
            {
                foreach (var discipline in dd)
                {
                    if (discipline?.AvailableAbilities is List<VitaeAbilityDef> vdd && !vdd.NullOrEmpty())
                    {
                        for (var i = 0; i < vdd.Count; i++)
                        {
                            var VitaeAbilityDef = vdd[i];
                            if (AbilityData.Powers.FirstOrDefault(x => x.Def.defName == VitaeAbilityDef.defName) ==
                                null)
                            {
                                AddPawnAbility(VitaeAbilityDef);
                            }
                        }
                    }
                }
            }
            //Bloodlines Abilities
            if (this?.Bloodline?.bloodlineAbilities is List<VitaeAbilityDef> bloodVads && !bloodVads.NullOrEmpty())
            {
                foreach (var bloodVad in bloodVads)
                {
                    if (AbilityData.Powers.FirstOrDefault(x => x.Def.defName == bloodVad.defName) == null)
                    {
                        AddPawnAbility(bloodVad);
                    }
                }
            }
            //Regenerate Limb
            if (this?.AbilityData.Powers?.FirstOrDefault(x =>
                    x.Def is VitaeAbilityDef vDef && vDef == VampDefOf.ROMV_RegenerateLimb) == null)
            {
                AddPawnAbility(VampDefOf.ROMV_RegenerateLimb);
            }

            //Vampiric Healing
            if (this?.AbilityData.Powers?.FirstOrDefault(x =>
                    x.Def is VitaeAbilityDef vDef && vDef == VampDefOf.ROMV_VampiricHealing) == null)
            {
                AddPawnAbility(VampDefOf.ROMV_VampiricHealing);
            }

            //Vampiric Scar Healing
            if (this?.AbilityData.Powers?.FirstOrDefault(x =>
                    x.Def is VitaeAbilityDef vDef && vDef == VampDefOf.ROMV_VampiricHealingScars) == null)
            {
                AddPawnAbility(VampDefOf.ROMV_VampiricHealingScars);
            }
        }

        private void GiveFeedJob(Pawn victim)
        {
            Job feedJob = new Job(VampDefOf.ROMV_Feed, victim);
            AbilityUser.jobs.TryTakeOrderedJob(feedJob, JobTag.SatisfyingNeeds);
        }

        private void GiveEmbraceJob(Pawn newChilde)
        {
            Job embraceJob = new Job(VampDefOf.ROMV_Embrace, newChilde);
            AbilityUser.jobs.TryTakeOrderedJob(embraceJob);
        }

        /// <summary>
        /// Takes a normal character and transforms them into a Ghoul.
        /// </summary>
        /// <param name="newDomitor"></param>
        /// <param name="isRevenant"></param>
        public void InitializeGhoul(Pawn newDomitor, bool isRevenant = false)
        {
            if (!beenGhoulBefore)
            {
                this.Level = 0;
                beenGhoulBefore = true;
            }

            //If no domitor exists, generate a random lower level vampire.
            if (newDomitor == null)
                newDomitor = VampireGen.GenerateVampire(Rand.Range(7, 13), VampireUtility.RandBloodline, null, null);

            //The domitor must be a vampire. The ghoul must not be a vampire.
            if (!newDomitor.IsVampire() || this.Pawn.IsVampire())
                return;

            //The ghoul cannot already be a ghoul.
            var hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROM_GhoulHediff, false);
            if (hediff != null)
                return;

            //Create the ghoul hediff.
            var ghoulHediff = (HediffGhoul)HediffMaker.MakeHediff(VampDefOf.ROM_GhoulHediff, this.Pawn);
            ghoulHediff.domitor = newDomitor;
            ghoulHediff.bloodline = newDomitor.VampComp().Bloodline;
            ghoulHediff.ghoulPower = GhoulUtility.GetGhoulPower(this.Pawn, newDomitor);
            ghoulHediff.ghoulType = (isRevenant) ? GhoulType.Revenant : GhoulType.Standard;
            this.Pawn.health.AddHediff(ghoulHediff, null, null);

            //Adjust the bond.
            AdjustBondWithRegnant(newDomitor, 1, true);

            //Remove memories of harm. We're friends now! It's alright ghoul-guy.
            VampireBiteUtility.TryRemoveHarmedMemory(newDomitor, this.Pawn);

        }

        /// <summary>
        /// Tries to adjust the bond stage of the character.
        /// </summary>
        /// <param name="regnant"></param>
        /// <param name="value"></param>
        /// <param name="showMessages"></param>
        public void AdjustBondWithRegnant(Pawn regnant, int value, bool showMessages = true)
        {
            if (!regnant.IsVampire())
                return;
            if (this.thrallData == null && value < 0)
                return;
            if (value > 0)
            {
                if (this.thrallData == null)
                    this.thrallData = new ThrallData(this.Pawn);
                if (this.thrallData.TryAdjustBondStage(regnant, value, showMessages))
                {
                }
            }

        }

        /// <summary>
        /// Takes a normal character and transforms them into a fully-functioning vampire.
        /// </summary>
        /// <param name="newSire"></param>
        /// <param name="newBloodline"></param>
        /// <param name="newGeneration"></param>
        /// <param name="firstVampire"></param>
        public void InitializeVampirism(Pawn newSire, BloodlineDef newBloodline = null, int newGeneration = -1,
            bool firstVampire = false)
        {
            //Log.Message("Init");
            //Log.Message($"Initialised vampirism ({Pawn?.Name} gen {newGeneration} blood {newBloodline})", true);
            VampireGen.RemoveMortalHediffs(AbilityUser);
            VampireGen.TryGiveVampirismHediff(AbilityUser, newGeneration, newBloodline, newSire, firstVampire);
            if (!firstVampire)
            {
                bloodline = newBloodline; // sireComp.Bloodline;
                generation = newGeneration; // + 1;
                sire = newSire;
                VampireRelationUtility.SetSireChildeRelations(AbilityUser, newSire?.VampComp() ?? null, newGeneration);
            }
            else
            {
                generation = 1;
                bloodline = VampDefOf.ROMV_Caine;
                sire = null;
            }
            VampireGen.TryGiveVampireAdditionalHediffs(AbilityUser);
            if (VampireUtility.IsDaylight(AbilityUser)) VampireUtility.MakeSleepy(AbilityUser);
            VampireUtility.AdjustTimeTables(AbilityUser);

            sheet = new SkillSheet(AbilityUser);
            sheet.InitializeDisciplines();
            Notify_LevelUp(false);
            AbilityUser.needs.AddOrRemoveNeedsAsAppropriate(); //This removes "food" and adds "blood"
            if (!bloodline.allowsHair)
                AbilityUser.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");
            if (this?.AbilityUser?.playerSettings != null)
                AbilityUser.playerSettings.hostilityResponse = HostilityResponseMode.Attack;

            //Prevents enemy vampires from spawning in with low vitae and hunting the players' characters.
            //Legendary vampires, however, will spawn hungry.
            if (this.Blood != null &&
                this.AbilityUser.Faction != Faction.OfPlayerSilentFail &&
                this.AbilityUser.Faction != Find.FactionManager.FirstFactionOfDef(VampDefOf.ROMV_LegendaryVampires))
                this.Blood.CurBloodPoints = this.Blood.MaxBloodPoints;


            Find.World.GetComponent<WorldComponent_VampireTracker>().AddVampire(AbilityUser, newSire, bloodline, generation, AbilityUser.ageTracker.AgeBiologicalYearsFloat);
        }



        private bool vampirismTriedToBeInialized = false;
        public override void CompTick()
        {
            if (!vampirismTriedToBeInialized)
            {
                //Log.Message($"Name={Pawn?.Name} Pawn.kindDef.defName={Pawn?.kindDef?.defName} pawnKindDefIsVampire == {pawnKindDefIsVampire}", true);
                vampirismTriedToBeInialized = true;
                if (!IsVampire)
                {
                    if (Pawn.kindDef.defName == "ROMV_ThinbloodVampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(14, 15));
                    else if (Pawn.kindDef.defName == "ROMV_LesserVampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(12, 13));
                    else if (Pawn.kindDef.defName == "ROMV_VampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(9, 11));
                    else if (Pawn.kindDef.defName == "ROMV_GreaterVampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(7, 8));
                    else if (Pawn.kindDef.defName == "ROMV_AncientVampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(3, 6));
                    else if (Pawn.kindDef.defName == "ROMV_VampireKind")
                        InitializeVampirism(null, VampireUtility.RandBloodline, Rand.Range(3, 6));
                }
            }

            base.CompTick();

            if (!IsVampire)
            {
                return;
            }

            SunlightWatcherTick();
            if (Transformed && atCurTicks != -1 && Find.TickManager.TicksGame > atCurTicks)
            {
                atDirty = true;
            }
        }


        private bool factionResolved = false;

        /// Give AI Werewolves levels in different forms.
        public void ResolveAIFactionSpawns()
        {
            if (!factionResolved && VampireFactionUtility.IsVampireFaction(Pawn?.Faction))
            {
                factionResolved = true;
            }
        }


        public void SunlightWatcherTick()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                try
                {
                    //Log.Message("SunlightWatcher");
                    Pawn p = Pawn;
                    Map m = p.MapHeld;
                    IntVec3 i = p.PositionHeld;
                    if (p.ParentHolder.IsEnclosingContainer())
                        return;
                    if (p.IsAlreadyDoingSunlightPathJob())
                        return;
                    if (p.Spawned && VampireUtility.IsDaylight(m) && !i.Roofed(m))
                    {
                        ThinkNode_JobGiver thinkNodeJobGiver =
                            (ThinkNode_JobGiver)Activator.CreateInstance(typeof(JobGiver_SeekShelterFromSunlight));
                        thinkNodeJobGiver.ResolveReferences();
                        ThinkResult thinkResult = thinkNodeJobGiver.TryIssueJobPackage(p, default(JobIssueParams));
                        if (thinkResult.Job is Job j && j.IsSunlightSafeFor(p))
                        {
                            p.jobs.StartJob(j, JobCondition.Incompletable, null, false, true, null, null);
                        }
                        else
                        {
                            //Messages.Message("Failed to give seek shelter from sunlight job", MessageTypeDefOf.RejectInput);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public void Notify_Starving(int lastNonStarvingTick)
        {
        }

        public void Notify_Embraced(CompVampire sireComp)
        {
            InitializeVampirism(sireComp.AbilityUser, sireComp.Bloodline, sireComp.Generation + 1);
            Messages.Message("ROMV_EmbracedSuccessfully".Translate(
                AbilityUser.Named("PAWN"),
                sireComp.AbilityUser.Named("SIRE"),
                sireComp.Bloodline.Named("BLOODLINE")
            ), MessageTypeDefOf.PositiveEvent);
        }

        public void Notify_Diablerie(CompVampire victim)
        {
            Messages.Message("ROMV_DiablerieSuccessfully".Translate(new object[]
            {
                AbilityUser.LabelShort,
                victim.AbilityUser.LabelShort
            }), MessageTypeDefOf.PositiveEvent);
            Generation = Math.Min(Generation, victim.Generation);
            Souls.Add(victim.AbilityUser);

            //If Royalty is installed, take over their titles
            if (victim?.Pawn?.royalty is Pawn_RoyaltyTracker pRoyalty)
            {
                if (pRoyalty?.AllTitlesForReading?.FirstOrDefault() != null)
                {
                    foreach (var title in pRoyalty.AllTitlesForReading)
                    {
                        AbilityUser.royalty.SetTitle(title.faction, title.def, false, false, true);
                    }
                }
            }

            VampireThoughtUtility.GiveThoughtsForDiablerie(AbilityUser);
        }

        #endregion Methods

        #region Overrides

        public override bool TryTransformPawn() => Bloodline != null;

        public override float GrappleModifier => IsVampire ? 20 - generation : 0;
        public HediffGhoul GhoulHediff => this.Pawn.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROM_GhoulHediff) as HediffGhoul;


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Find.Selector.NumSelected != 1)
            {
                yield break;
            }

            for (int i = 0; i < AbilityData.AllPowers.Count; i++)
            {
                if (AbilityData.AllPowers[i] is VampAbility p && p.ShouldShowGizmo() &&
                    p.AbilityDef.MainVerb.hasStandardCommand && p.AbilityDef.bloodCost != 0) yield return p.GetGizmo();
            }
            if (AbilityUser.Downed && (AbilityUser.IsVampire() || AbilityUser.IsGhoul()))
            {
                if (!(this.AbilityUser?.health?.summaryHealth?.SummaryHealthPercent > 0.99f))
                {

                    VitaeAbilityDef bloodHeal = DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricHealing");

                    yield return new Command_Action()
                    {
                        defaultLabel = bloodHeal.label,
                        defaultDesc = bloodHeal.GetDescription(),
                        icon = bloodHeal.uiIcon,
                        action = delegate
                        {
                            AbilityUser.Drawer.Notify_DebugAffected();
                            MoteMaker.ThrowText(AbilityUser.DrawPos, AbilityUser.Map, StringsToTranslate.AU_CastSuccess);
                            if (AbilityUser.IsGhoul())
                                BloodPool.CurGhoulVitaePoints -= bloodHeal.bloodCost;
                            else
                                BloodPool.AdjustBlood(-bloodHeal.bloodCost);
                            VampireUtility.Heal(AbilityUser);
                        },
                        disabled = (AbilityUser.IsGhoul()) ? BloodPool.CurGhoulVitaePoints <= 0 : BloodPool.CurBloodPoints <= 0
                    };
                }

                if (this.AbilityUser?.health?.hediffSet?.hediffs.Any(x => x is Hediff_MissingPart) ??
                    false)
                {
                    VitaeAbilityDef bloodRegen = DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_RegenerateLimb");
                    yield return new Command_Action()
                    {
                        defaultLabel = bloodRegen.label,
                        defaultDesc = bloodRegen.GetDescription(),
                        icon = bloodRegen.uiIcon,
                        action = delegate
                        {
                            AbilityUser.Drawer.Notify_DebugAffected();
                            MoteMaker.ThrowText(AbilityUser.DrawPos, AbilityUser.Map, StringsToTranslate.AU_CastSuccess);
                            if (AbilityUser.IsGhoul())
                                BloodPool.CurGhoulVitaePoints -= bloodRegen.bloodCost;
                            else
                                BloodPool.AdjustBlood(-bloodRegen.bloodCost);

                            VampireUtility.RegenerateRandomPart(AbilityUser);
                        },
                        disabled = (AbilityUser.IsGhoul()) ? BloodPool.CurGhoulVitaePoints <= 0 : BloodPool.CurBloodPoints <= 0
                    };
                }

            }
        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref bloodline, "bloodline");
            Scribe_Values.Look(ref beenGhoulBefore, "beenGhoulBefore", false);
            Scribe_Values.Look(ref generation, "generation");
            Scribe_Values.Look(ref level, "level");
            Scribe_Values.Look(ref xp, "xp");
            Scribe_Values.Look(ref abilityPoints, "abilityPoints");
            Scribe_Values.Look(ref curSunlightPolicy, "curSunlightPolicy", SunlightPolicy.Restricted);
            Scribe_References.Look(ref sire, "sire");
            Scribe_Collections.Look(ref souls, "souls", LookMode.Reference);
            Scribe_Deep.Look(ref sheet, "sheet", new object[] { AbilityUser });
            Scribe_Deep.Look(ref thrallData, "thrallData", new object[] { AbilityUser });
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AbilityData.Powers.Clear();
                if ((AbilityUser.IsVampire() || AbilityUser.IsGhoul()) && (AbilityData.Powers == null || AbilityData.Powers.NullOrEmpty()))
                {
                    Notify_UpdateAbilities();
                }
            }
        }

        #endregion Overrides

        public void Notify_DeGhouled()
        {
            Messages.Message("ROMV_LostGhoulPowers".Translate(this.AbilityUser).AdjustedFor(this.AbilityUser), MessageTypeDefOf.NegativeEvent);
            this.ThrallData = null;
        }
    }
}