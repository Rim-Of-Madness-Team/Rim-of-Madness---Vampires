using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using Vampire.Components;
using Vampire.Utilities;

namespace Vampire
{

    public enum PreferredFeedMode
    {
        None,
        AnimalNonLethal,
        AnimalLethal,
        HumanoidNonLethal,
        HumanoidLethal
    }

    public enum PreferredHumanoidFeedType
    {
        All,
        PrisonersOnly
    }

    
    /// <summary>
    /// Duplicate of Need_Food
    /// </summary>
    public class Need_Blood : Need
    {
        #region Variables
        private int curBloodPoints = Int32.MinValue;
        private int nextBloodChangeTick = -1;
        private int lastNonStarvingTick = -1;
        private bool bloodFixer = false;
        public PreferredFeedMode preferredFeedMode = PreferredFeedMode.HumanoidNonLethal;
        public PreferredHumanoidFeedType preferredHumanoidFeedType = PreferredHumanoidFeedType.All;
        #endregion Variables

        #region Properties
        public CompVampire CompVampire => pawn?.TryGetComp<CompVampire>();
        public bool IsAnimal => pawn?.RaceProps?.Animal ?? false;
        public bool IsFull => CurBloodPoints == MaxBloodPoints;
        public bool Starving => CompVampire != null && CompVampire.IsVampire && CurCategory == HungerCategory.Starving;
        public bool ShouldDie => CurBloodPoints == 0;
        public float PercPerPoint => 1f / MaxBloodPoints;
        public bool DrainingIsDeadly => CurBloodPoints <= 2 || (pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == HediffDefOf.BloodLoss) is Hediff bloodLoss && bloodLoss.CurStageIndex > 2);
        //public PreferredFeedMode PreferredFeedMode { get => preferredFeedMode; set => preferredFeedMode = value; }




        public int NextBloodChangeTick
        {
            get
            {
                if (nextBloodChangeTick == Int32.MaxValue)
                    nextBloodChangeTick = Find.TickManager.TicksGame + GenDate.TicksPerDay;
                return nextBloodChangeTick;
            }
        }

        public int CurBloodPoints
        {
            get
            {
                if (curBloodPoints == Int32.MinValue)
                    curBloodPoints = MaxBloodPoints;
                return curBloodPoints;
            }
            set => curBloodPoints = value;
        }
        public int MaxBloodPointsForAnimal(Pawn p)
        {
            PawnKindDef def = p.kindDef;
            int result = (def.RaceProps.baseBodySize < 1f) ? 1 : 2;
            if (def == PawnKindDef.Named("Rat")) return 1;
            if (def == PawnKindDefOf.Thrumbo) return 10;
            return result;
        }

        public int MaxBloodPoints
        {
            get
            {
                int result = 7;
                if (IsAnimal)
                {
                    result = MaxBloodPointsForAnimal(pawn);
                }
                if (CompVampire != null && CompVampire.IsVampire)
                {
                    int gen = CompVampire.Generation;
                    result = (gen > 7) ? 10 + (Math.Abs(gen - 13)) : 10 * (Math.Abs(gen - 9));
                }
                return result;
            }
        }
        public int BloodChangePerDay
        {
            get
            {
                if (CompVampire != null && CompVampire.IsVampire)
                {
                    if (CompVampire.Generation < 7)
                    {
                        return -3;
                    }
                    return -1;
                }
                return +1;
            }
        }
        public HungerCategory CurCategory
        {
            get
            {
                if (CompVampire != null && CompVampire.IsVampire)
                {
                    if (CurLevelPercentage <= 0f)
                        return HungerCategory.Starving;

                    if (CurLevelPercentage < PercPerPoint * 2)
                        return HungerCategory.UrgentlyHungry;

                    if (CurLevelPercentage < MaxLevel)
                        return HungerCategory.Hungry;
                }
                return HungerCategory.Fed;
            }
        }
        #endregion  Properties
        
        public override int GUIChangeArrow 
            => (CompVampire != null && CompVampire.IsVampire)? -1 : (CurLevel == MaxLevel) ? 0 : 1;
        public override float CurInstantLevel => base.CurInstantLevel;
        public override float CurLevel
            {
            get => base.CurLevel;
            set => base.CurLevel = value;
        }

        public float ShouldFeedPerc => 0.7f;

        public override float MaxLevel
            => MaxBloodPoints;
        public float BloodWanted 
            => MaxBloodPoints - CurBloodPoints;
        
        public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - lastNonStarvingTick);

        public Need_Blood(Pawn pawn) : base(pawn) { }

        public override void ExposeData()
        {
            //private int curBloodPoints = Int32.MinValue;
            //private int nextBloodChangeTick = Int32.MaxValue;
            base.ExposeData();
            Scribe_Values.Look<int>(ref lastNonStarvingTick, "lastNonStarvingTick", -1);
            Scribe_Values.Look<int>(ref nextBloodChangeTick, "nextBloodChangeTick", -1);
            Scribe_Values.Look<int>(ref curBloodPoints, "curBloodPoints", -1);
            Scribe_Values.Look<bool>(ref bloodFixer, "bloodFixer");
            Scribe_Values.Look<PreferredFeedMode>(ref preferredFeedMode, "preferredFeedMode", PreferredFeedMode.HumanoidNonLethal);
            Scribe_Values.Look<PreferredHumanoidFeedType>(ref preferredHumanoidFeedType, "preferredHumanoidFeedType", PreferredHumanoidFeedType.PrisonersOnly);
        }
        
        public int AdjustBlood(int amt, bool alert = true)
        {
            int prevBloodPoints = CurBloodPoints;

            CurBloodPoints = Mathf.Clamp(CurBloodPoints + amt, 0, MaxBloodPoints);
            CurLevelPercentage = CurBloodPoints * PercPerPoint;

            if (!pawn.IsVampire() && CurBloodPoints < prevBloodPoints)
            {
                int diff = prevBloodPoints - CurBloodPoints;
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, diff * PercPerPoint);
            }

            if (CurBloodPoints == 0)
                Notify_NoBloodLeft(alert);

            return prevBloodPoints - CurBloodPoints;
        }

        public void Notify_NoBloodLeft(bool alert = true)
        {
            if (pawn.Faction == Faction.OfPlayer)
            {
                if (alert)
                {
                    if (pawn.IsVampire())
                        Messages.Message("ROMV_BloodDepletedVamp".Translate(pawn.LabelCap), MessageTypeDefOf.NeutralEvent);
                    else
                        Messages.Message("ROMV_BloodDepleted".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent);
                }

            }

            if (!pawn.IsVampire())
            {
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, 999f);
                if (!pawn.Dead) pawn.Kill(null);
            }
        }

        public void TransferBloodTo(int amt, Need_Blood otherPool, bool alert = true)
        {
            int removedAmt = AdjustBlood(-amt);
            if (removedAmt > 0) otherPool.AdjustBlood(removedAmt);
        }
        
        public override void NeedInterval()
        {
            if (pawn == null || !pawn.Spawned)
                return;

            if (!bloodFixer)
            {
                bloodFixer = true;
                nextBloodChangeTick = -1;
            }
            
            //if ((pawn?.IsVampire() ?? false) && pawn.RaceProps != null && pawn.RaceProps.Humanlike && pawn.Faction != null && pawn.Faction == Faction.OfPlayer &&
            //    !pawn.Downed && !pawn.Dead &&
            //    !pawn.Drafted && (CurLevelPercentage < ShouldFeedPerc) &&
            //    !pawn.CurJob.playerForced &&
            //    (pawn.CurJob.def != VampDefOf.ROMV_ConsumeBlood && 
            //    pawn.CurJob.def != VampDefOf.ROMV_Feed && 
            //    pawn.CurJob.def != VampDefOf.ROMV_Sip &&
            //    pawn.CurJob.def != JobDefOf.AttackMelee &&
            //    pawn.CurJob.def != JobDefOf.Arrest &&
            //    pawn.CurJob.def != JobDefOf.BeatFire &&
            //    pawn.CurJob.def != JobDefOf.Rescue &&
            //    pawn.CurJob.def != JobDefOf.TendPatient &&
            //    pawn.CurJob.def != JobDefOf.Flee &&
            //    pawn.CurJob.def != JobDefOf.FleeAndCower))
            //{
            //    if (JobGiver_GetBlood.FeedJob(pawn) is Job j)
            //    {
            //        this.pawn.jobs.StartJob(j, JobCondition.InterruptForced, null, false, true, null, new JobTag?(JobTag.SatisfyingNeeds), false);
            //    }
            //}

            //if (Find.TickManager.TicksGame % 250 == 0)
                //Log.Message("Ticks => " + Find.TickManager.TicksGame);
            if (Find.TickManager.TicksGame > nextBloodChangeTick)
            {
                int math = Find.TickManager.TicksGame + GenDate.TicksPerDay;
                //Log.Message("BLOOD TICKS SET TO => " + math);
                nextBloodChangeTick = math;
                AdjustBlood(BloodChangePerDay);
            }

            if (!Starving)
            {
                lastNonStarvingTick = Find.TickManager.TicksGame;
            }

            if (!IsFrozen)
            {

                if (Starving)
                {

                    if (CompVampire != null && CompVampire.IsVampire)
                    {

                        CompVampire.Notify_Starving(lastNonStarvingTick);
                    }
                    else if (!pawn.Dead)
                    {

                        HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, 1f);
                        pawn.Kill(null);
                    }
                }
            }
        }

        public override void SetInitialLevel()
        {
            //base.CurLevelPercentage = 1.0f;
            CurLevel = CurBloodPoints = MaxBloodPoints;
            if (Current.ProgramState == ProgramState.Playing)
            {
                lastNonStarvingTick = Find.TickManager.TicksGame;
            }
        }

        public override string GetTipString()
        {
            return string.Concat(new string[]
            {
                GetLabel(),
                ": ",
                CurLevelPercentage.ToStringPercent(),
                " (",
                CurLevel.ToString("0.##"),
                " / ",
                MaxLevel.ToString("0.##"),
                ")\n",
                GetDescription()
            });
        }

        public string GetLabel()
        {
            bool isVampire = pawn.IsVampire();
            /// CHJEES ANDROIDS ///////////////////////////////////////////////////////
            if (pawn.IsAndroid())
            {
                if (isVampire)
                    return "ROMV_AndroidCoolantVitae".Translate();
                return "ROMV_AndroidCoolant".Translate();
            }
            ///////////////////////////////////////////////////////////////////////////
            if (isVampire)
                return "ROMV_Vitae".Translate();
            return LabelCap;
        }

        public string GetDescription()
        {
            bool isVampire = pawn.IsVampire();
            /// CHJEES ANDROIDS ///////////////////////////////////////////////////////
            if (pawn.IsAndroid())
            {
                if (isVampire)
                    return "ROMV_AndroidCoolantVitaeDesc".Translate();
                return "ROMV_AndroidCoolantDesc".Translate();
            }
            ///////////////////////////////////////////////////////////////////////////
            if (isVampire)
                return "ROMV_VitaeDesc".Translate();
            return def.description;
        }
        
        public Color GetColorToUse()
        {
            bool isVampire = pawn.IsVampire();
            if (pawn.IsAndroid())
            {
                if (isVampire)
                    return VampireUtility.ColorAndroidCoolantVitae;
                return VampireUtility.ColorAndroidCoolant;
            }
            if (isVampire)
                return VampireUtility.ColorVitae; //new Color(0.65f, 0.008f, 0.008f);
            return VampireUtility.ColorBlood; //new Color(0.73f, 0.02f, 0.02f);


        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
        {
            if (threshPercents == null)
            {
                threshPercents = new List<float>();
            }
            threshPercents.Clear();
            for (int i = 1; i < MaxBloodPoints; i++)
            {
                threshPercents.Add(PercPerPoint * i);
            }
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if (doTooltip)
            {
                TooltipHandler.TipRegion(rect, new TipSignal(() => GetTipString(), rect.GetHashCode()));
            }

            if (pawn != null && pawn.Faction == Faction.OfPlayerSilentFail && pawn.VampComp() is CompVampire v && v.IsVampire)
            {
                //HostilityResponseModeUtility.DrawResponseButton(new Vector2(rect.width - 120f, 0f), pawn);
                BloodFeedModeUtility.DrawFeedModeButton(new Vector2(rect.width - 20f, 5f), pawn);
            }

            float num2 = 14f;
            float num3 = (customMargin < 0f) ? (num2 + 15f) : customMargin;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(rect2, GetLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Color colorToUse = GetColorToUse(); //(this.pawn?.IsVampire() ?? false) ? new Color(0.65f, 0.008f, 0.008f) : new Color(0.73f, 0.02f, 0.02f);
            Widgets.FillableBar(rect3, CurLevelPercentage, SolidColorMaterials.NewSolidColorTexture(colorToUse));
            //Widgets.FillableBar(rect3, this.CurLevelPercentage);
            if (drawArrows)
            {
                Widgets.FillableBarChangeArrows(rect3, GUIChangeArrow);
            }
            if (threshPercents != null)
            {
                for (int i = 0; i < Mathf.Min(threshPercents.Count, maxThresholdMarkers); i++)
                {
                    DrawBarThreshold(rect3, threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            if (!def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        // RimWorld.Need
        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }




    }
}
