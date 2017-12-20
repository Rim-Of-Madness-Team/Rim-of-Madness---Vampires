using System;
using System.Collections.Generic;
using System.Linq;
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
        Restricted = 1
    }

    public class CompVampire : CompAbilityUser
    {
        #region Variables
        private Pawn sire = null;
        private List<Pawn> childer = null;
        private List<Pawn> ghouls = null;
        private List<Pawn> souls = new List<Pawn>();
        private SkillSheet sheet = null;
        private BloodlineDef bloodline = null;
        private int generation = -1;
        private int level = 0;
        private int xp = 0;
        private int abilityPoints = 0;
        private PawnKindDef currentForm = null;
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
        public SunlightPolicy CurrentSunlightPolicy { get => curSunlightPolicy; set => curSunlightPolicy = value; }
        public int VampLastHomeCheck { get => vampLastHomeCheck; set => vampLastHomeCheck = value; }
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

        public int Level { get => level;
            set
            {
                if (value > level)
                {
                    abilityPoints++;
                    if (XP < value * 600)
                    {
                        XP = value * 600;
                    }
                }
                level = value;
            }
        }
        public int XP { get => xp;
            set
            {
                xp = value;
                if (xp > XPTillNextLevel)
                   Notify_LevelUp(true);
            }
        }

        public float XPLastLevel
        {
            get
            {
                float result = 0f;
                if (level > 0) result = level * 600;
                return result;
            }
        }
        public float XPTillNextLevelPercent => (float)(xp - XPLastLevel) / (float)(XPTillNextLevel - XPLastLevel);
        public int XPTillNextLevel => (level + 1) * 600;

        public int AbilityPoints { get => abilityPoints; set => abilityPoints = value; }
        public bool Transformed => currentForm != null;
        public PawnKindDef CurrentForm { get => currentForm; set => currentForm = value;
        }
        private Graphic curFormGraphic = null;
        public Graphic CurFormGraphic { get => curFormGraphic; set => curFormGraphic = value; }

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

        public Pawn Sire { get => sire; set => sire = value; }
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
        //public List<Pawn> Ghouls
        //{
        //    get
        //    {
        //        if (ghouls == null)
        //        {
        //            ghouls = new List<Pawn>();
        //            if (this.AbilityUser?.relations?.DirectRelations is List<DirectPawnRelation> rels)
        //            {
        //                foreach (DirectPawnRelation rel in rels)
        //                {
        //                    if (rel.def == VampDefOf.ROMV_Childe)
        //                    {
        //                        ghouls.Add(rel.otherPawn);
        //                    }
        //                }
        //            }

        //        }
        //        return ghouls;
        //    }
        //}
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
        public BloodlineDef Bloodline {
            get => bloodline;
            set => bloodline = value; }
        public int Generation { get => generation; set => generation = value; }
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
                    if (i == 2) { result -= 3000; continue; }
                    if (i < 7)  { result -= 100; continue; }
                    result -= 5;
                }
                return result;
            }
        }
        public bool IsVampire => AbilityUser?.health?.hediffSet?.HasHediff(VampDefOf.ROM_Vampirism) ?? false;
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
            if (sendNotification && IsVampire && AbilityUser != null && AbilityUser.Spawned && AbilityUser.Faction == Faction.OfPlayerSilentFail)
                Messages.Message("ROMV_LevelUp".Translate(AbilityUser), new RimWorld.Planet.GlobalTargetInfo(AbilityUser), DefDatabase<MessageTypeDef>.GetNamed("ROMV_VampireNotifaction"));
        }
        public void Notify_ResetAbilities()
        {
            Sheet.ResetDisciplines();
        }

        public void Notify_UpdateAbilities()
        {
            if (AbilityUser.IsVampire() && this is CompVampire)
            {
                //Disciplines Skill Sheet
                if (Sheet?.Disciplines is List<Discipline> dd && !dd.NullOrEmpty())
                {
                    foreach (Discipline d in dd)
                    {
                        if (d?.AvailableAbilities is List<VitaeAbilityDef> vdd && !vdd.NullOrEmpty())
                        {
                            foreach (VitaeAbilityDef vd in vdd)
                            {
                                if (AbilityData.Powers.FirstOrDefault(x => x.Def.defName == vd.defName) == null)
                                {
                                    AddPawnAbility(vd);
                                }

                            }
                        }
                    }
                }
                //Bloodlines Abilities
                if (this?.Bloodline?.bloodlineAbilities is List<VitaeAbilityDef> bloodVADs && !bloodVADs.NullOrEmpty())
                {
                    foreach (VitaeAbilityDef bloodVAD in bloodVADs)
                    {
                        if (AbilityData.Powers.FirstOrDefault(x => x.Def.defName == bloodVAD.defName) == null)
                        {
                            AddPawnAbility(bloodVAD);
                        }
                    }
                }
                //Regenerate Limb
                if (this?.AbilityData.Powers?.FirstOrDefault(x => x.Def is VitaeAbilityDef vDef && vDef == VampDefOf.ROMV_RegenerateLimb) == null)
                {
                    AddPawnAbility(VampDefOf.ROMV_RegenerateLimb);
                }

                //Vampiric Healing
                if (this?.AbilityData.Powers?.FirstOrDefault(x => x.Def is VitaeAbilityDef vDef && vDef == VampDefOf.ROMV_VampiricHealing) == null)
                {
                    AddPawnAbility(VampDefOf.ROMV_VampiricHealing);
                }
            }
        }

        public void GiveFeedJob(Pawn victim)
        {
            Job feedJob = new Job(VampDefOf.ROMV_Feed, victim);
            AbilityUser.jobs.TryTakeOrderedJob(feedJob, JobTag.SatisfyingNeeds);
        }

        public void GiveEmbraceJob(Pawn newChilde)
        {
            Job embraceJob = new Job(VampDefOf.ROMV_Embrace, newChilde);
            AbilityUser.jobs.TryTakeOrderedJob(embraceJob);
        }

        public void InitializeVampirism(Pawn newSire, BloodlineDef newBloodline = null, int newGeneration = -1, bool firstVampire = false)
        {
            //Log.Message("Init");
            AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x is HediffVampirism_VampGiver);
            AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.Malnutrition);
            AbilityUser.health.hediffSet.hediffs.RemoveAll(x => x is Hediff_Addiction);
            VampireGen.TryGiveVampirismHediff(AbilityUser, newGeneration, newBloodline, newSire, firstVampire);
            if (!firstVampire)
            {
                bloodline = newBloodline;// sireComp.Bloodline;
                generation = newGeneration;// + 1;
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
        }

        public override void CompTick()
        {
            base.CompTick();
            if (IsVampire)
            {
                SunlightWatcherTick();
                if (Transformed && atCurTicks != -1 && Find.TickManager.TicksGame > atCurTicks)
                {
                    atDirty = true;
                }
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
                        ThinkNode_JobGiver thinkNode_JobGiver = (ThinkNode_JobGiver)Activator.CreateInstance(typeof(JobGiver_SeekShelterFromSunlight));
                        thinkNode_JobGiver.ResolveReferences();
                        ThinkResult thinkResult = thinkNode_JobGiver.TryIssueJobPackage(p, default(JobIssueParams));
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
            Messages.Message("ROMV_EmbracedSuccessfully".Translate(new object[]
            {
                AbilityUser.LabelShort,
                sireComp.AbilityUser.LabelShort,
                sireComp.Bloodline.LabelCap
            }), MessageTypeDefOf.PositiveEvent);
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
            VampireThoughtUtility.GiveThoughtsForDiablerie(AbilityUser);
        }

        #endregion Methods

        #region Overrides
        public override bool TryTransformPawn() => Bloodline != null;

        /// <summary>
        /// Prevents natural death in most circumstances.
        /// Checks to see if the Vampire needs to enter a Torpor state or not.
        /// Reacts to stake paralysis
        /// </summary>
        //public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        //{
        //    //if should kill, and not decapitation or massive body squish, start Torpor state
        //    absorbed = true;
        //}
        
        public override bool AllowStackWith(Thing other)
        {
            return base.AllowStackWith(other);

        }

        public override float GrappleModifier => IsVampire ? 20 - generation : 0;



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Find.Selector.NumSelected == 1)
            {
                for (int i = 0; i < AbilityData.AllPowers.Count; i++)
                {
                    if (AbilityData.AllPowers[i] is VampAbility p && p.ShouldShowGizmo() && p.AbilityDef.MainVerb.hasStandardCommand && p.AbilityDef.bloodCost != 0) yield return p.GetGizmo();
                }
                if (AbilityUser.Downed && AbilityUser.IsVampire())
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
                            BloodPool.AdjustBlood(-bloodHeal.bloodCost);
                            VampireUtility.Heal(AbilityUser);
                        },
                        disabled = BloodPool.CurBloodPoints <= 0
                    };
                }
            }

        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref bloodline, "bloodline");
            Scribe_Values.Look(ref generation, "generation");
            Scribe_Values.Look(ref level, "level");
            Scribe_Values.Look(ref xp, "xp");
            Scribe_Values.Look(ref abilityPoints, "abilityPoints");
            Scribe_Values.Look(ref curSunlightPolicy, "curSunlightPolicy", SunlightPolicy.Restricted);
            Scribe_References.Look(ref sire, "sire");
            Scribe_Collections.Look(ref souls, "souls", LookMode.Reference);
            Scribe_Deep.Look(ref sheet, "sheet", new object[] { AbilityUser });
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AbilityData.Powers.Clear();
                if (AbilityUser.IsVampire() && (AbilityData.Powers == null || AbilityData.Powers.NullOrEmpty()))
                {
                    if (Sheet.Disciplines is List<Discipline> dd && !dd.NullOrEmpty())
                    {
                        foreach (Discipline d in dd)
                        {
                            if (d.AvailableAbilities is List<VitaeAbilityDef> vds && !vds.NullOrEmpty())
                            {
                                foreach (VitaeAbilityDef vd in vds)
                                {
                                    AddPawnAbility(vd);
                                }
                            }
                        }
                    }
                    Notify_UpdateAbilities();
                }
            }
        }

        #endregion Overrides
    }
}
