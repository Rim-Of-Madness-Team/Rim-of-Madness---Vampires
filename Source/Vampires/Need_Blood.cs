using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using Verse.AI;

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
        #endregion Variables

        #region Properties
        public CompVampire CompVampire => this.pawn?.TryGetComp<CompVampire>();
        public bool IsAnimal => this.pawn?.RaceProps?.Animal ?? false;
        public bool IsFull => this.CurBloodPoints == this.MaxBloodPoints;
        public bool Starving => CompVampire != null && CompVampire.IsVampire && this.CurCategory == HungerCategory.Starving;
        public bool ShouldDie => CurBloodPoints == 0;
        public float PercPerPoint => 1f / MaxBloodPoints;
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
                    result = MaxBloodPointsForAnimal(this.pawn);
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
                    if (base.CurLevelPercentage <= 0f)
                        return HungerCategory.Starving;

                    if (base.CurLevelPercentage < PercPerPoint * 2)
                        return HungerCategory.UrgentlyHungry;

                    if (base.CurLevelPercentage < MaxLevel)
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
            => this.MaxBloodPoints - this.CurBloodPoints;
        
        public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - this.lastNonStarvingTick);

        public Need_Blood(Pawn pawn) : base(pawn) { }

        public override void ExposeData()
        {
            //private int curBloodPoints = Int32.MinValue;
            //private int nextBloodChangeTick = Int32.MaxValue;
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastNonStarvingTick, "lastNonStarvingTick", -1, false);
            Scribe_Values.Look<int>(ref this.nextBloodChangeTick, "nextBloodChangeTick", -1, false);
            Scribe_Values.Look<int>(ref this.curBloodPoints, "curBloodPoints", -1, false);
            Scribe_Values.Look<bool>(ref this.bloodFixer, "bloodFixer", false);
            Scribe_Values.Look<PreferredFeedMode>(ref this.preferredFeedMode, "preferredFeedMode", PreferredFeedMode.HumanoidNonLethal);
        }
        
        public int AdjustBlood(int amt)
        {
            int prevBloodPoints = CurBloodPoints;

            CurBloodPoints = Mathf.Clamp(CurBloodPoints + amt, 0, MaxBloodPoints);
            CurLevelPercentage = CurBloodPoints * PercPerPoint;

            if (!this.pawn.IsVampire() && CurBloodPoints < prevBloodPoints)
            {
                int diff = prevBloodPoints - CurBloodPoints;
                HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.BloodLoss, diff * PercPerPoint);
            }

            if (CurBloodPoints == 0)
                Notify_NoBloodLeft();

            return prevBloodPoints - CurBloodPoints;
        }

        public void Notify_NoBloodLeft()
        {
            if (this.pawn.Faction == Faction.OfPlayer)
            {
                if (this.pawn.IsVampire())
                    Messages.Message("ROMV_BloodDepletedVamp".Translate(this.pawn.LabelCap), MessageTypeDefOf.NeutralEvent);
                else
                    Messages.Message("ROMV_BloodDepleted".Translate(this.pawn.LabelCap), MessageTypeDefOf.NegativeEvent);
            }

            if (!this.pawn.IsVampire())
            {
                HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.BloodLoss, 999f);
                if (!this.pawn.Dead) this.pawn.Kill(null);
            }
        }

        public void TransferBloodTo(int amt, Need_Blood otherPool)
        {
            int removedAmt = AdjustBlood(-amt);
            if (removedAmt > 0) otherPool.AdjustBlood(removedAmt);
        }
        
        public override void NeedInterval()
        {
            if (!bloodFixer)
            {
                bloodFixer = true;
                this.nextBloodChangeTick = -1;
            }
            
            if (pawn.RaceProps != null && pawn.RaceProps.Humanlike && pawn.Faction != null && pawn.Faction == Faction.OfPlayer &&
                !pawn.Drafted && (pawn?.IsVampire() ?? false) && (CurLevelPercentage < ShouldFeedPerc) &&
                (pawn.CurJob.def != VampDefOf.ROMV_ConsumeBlood && 
                pawn.CurJob.def != VampDefOf.ROMV_Feed && 
                pawn.CurJob.def != VampDefOf.ROMV_Sip))
            {
                if (JobGiver_GetBlood.FeedJob(pawn) is Job j)
                {
                    this.pawn.jobs.StartJob(j, JobCondition.InterruptForced, null, false, true, null, new JobTag?(JobTag.SatisfyingNeeds), false);
                }
            }

            //if (Find.TickManager.TicksGame % 250 == 0)
                //Log.Message("Ticks => " + Find.TickManager.TicksGame);
            if (Find.TickManager.TicksGame > this.nextBloodChangeTick)
            {
                int math = Find.TickManager.TicksGame + GenDate.TicksPerDay;
                //Log.Message("BLOOD TICKS SET TO => " + math);
                this.nextBloodChangeTick = math;
                AdjustBlood(BloodChangePerDay);
            }

            if (!this.Starving)
            {
                this.lastNonStarvingTick = Find.TickManager.TicksGame;
            }

            if (!base.IsFrozen)
            {

                if (this.Starving)
                {

                    if (CompVampire != null && CompVampire.IsVampire)
                    {

                        CompVampire.Notify_Starving(lastNonStarvingTick);
                    }
                    else if (!this.pawn.Dead)
                    {

                        HealthUtility.AdjustSeverity(this.pawn, HediffDefOf.BloodLoss, 1f);
                        this.pawn.Kill(null);
                    }
                }
            }
        }

        public override void SetInitialLevel()
        {
            //base.CurLevelPercentage = 1.0f;
            this.CurLevel = this.CurBloodPoints = this.MaxBloodPoints;
            if (Current.ProgramState == ProgramState.Playing)
            {
                this.lastNonStarvingTick = Find.TickManager.TicksGame;
            }
        }

        public override string GetTipString()
        {
            return string.Concat(new string[]
            {
                this.GetLabel(),
                ": ",
                base.CurLevelPercentage.ToStringPercent(),
                " (",
                this.CurLevel.ToString("0.##"),
                " / ",
                this.MaxLevel.ToString("0.##"),
                ")\n",
                this.GetDescription()
            });
        }

        public string GetLabel()
        {
            if (this.pawn.IsVampire())
            {
                return "ROMV_Vitae".Translate();
            }
            return this.LabelCap;
        }

        public string GetDescription()
        {
            if (this.pawn.IsVampire())
            {
                return "ROMV_VitaeDesc".Translate();
            }
            return this.def.description;
        }
        
        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>();
            }
            this.threshPercents.Clear();
            for (int i = 1; i < MaxBloodPoints; i++)
            {
                this.threshPercents.Add(PercPerPoint * i);
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
                TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
            }

            if (pawn != null && pawn.VampComp() is CompVampire v && v.IsVampire)
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
            Widgets.Label(rect2, this.GetLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Color colorToUse = (this.pawn?.IsVampire() ?? false) ? new Color(0.65f, 0.008f, 0.008f) : new Color(0.73f, 0.02f, 0.02f);
            Widgets.FillableBar(rect3, this.CurLevelPercentage, SolidColorMaterials.NewSolidColorTexture(colorToUse));
            //Widgets.FillableBar(rect3, this.CurLevelPercentage);
            if (drawArrows)
            {
                Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
            }
            if (this.threshPercents != null)
            {
                for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
                {
                    this.DrawBarThreshold(rect3, this.threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = this.CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            if (!this.def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        // RimWorld.Need
        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
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
