using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

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

        //Variables related to Ghouls
        private bool mixedBlood = false;
        private int curGhoulVitaePoints = Int32.MinValue;   
        #endregion Variables

        #region Properties
        public CompVampire CompVampire => pawn?.TryGetComp<CompVampire>();
        public bool IsAnimal => pawn?.RaceProps?.Animal ?? false;
        public bool IsFull => CurBloodPoints == MaxBloodPoints;
        public bool Starving => CompVampire != null && CompVampire.IsVampire && CurCategory == HungerCategory.Starving;
        public bool ShouldDie => CurBloodPoints == 0;
        public float PercPerPoint => 1f / MaxBloodPoints;

        public bool DrainingIsDeadly
        {
            get
            {
                return CurBloodPoints <= 2 ||
                       pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == BloodUtility.GetBloodLossDef(pawn)) is Hediff
                           bloodLoss && bloodLoss.CurStageIndex > 2;
            }
        }

        public bool BloodFixer
        {
            get { return bloodFixer; }
            set { bloodFixer = value; }
        }

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

        public int CurGhoulVitaePoints
        {
            get
            {
                if (curGhoulVitaePoints == Int32.MinValue)
                    curGhoulVitaePoints = 0;
                return curGhoulVitaePoints;
            }
            set => curGhoulVitaePoints = Mathf.Clamp(value, 0, MaxBloodPoints);
        }

        public float CurGhoulVitaePercentage => CurGhoulVitaePoints * PercPerPoint;

        public int MaxBloodPointsForAnimal(Pawn p)
        {
            PawnKindDef def = p.kindDef;
            int result = def.RaceProps.baseBodySize < 1f ? 1 : 2;
            if (def?.RaceProps?.trainability == TrainabilityDefOf.Advanced)
                result += 1;
            return result;
        }

        public int MaxBloodPoints
        {
            get
            {

                if (VampireSettings.GetBloodPointOverride(pawn.def) is int overrideResult && overrideResult != -1)
                    return overrideResult;

                int result = 7;
                
                if (IsAnimal)
                    result = MaxBloodPointsForAnimal(pawn);

                if (pawn.IsVampire(true))
                {
                    int gen = CompVampire.Generation;
                    result = gen > 7 ? 10 + Math.Abs(gen - 13) : 10 * Math.Abs(gen - 9);
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
                    //if (CompVampire.Generation < 7)
                    //{
                    //    return -3;
                    //}
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
        {
            get
            {
                if (CompVampire is CompVampire compVampire && compVampire.IsRevenant && CurLevel != MaxLevel)
                    return 1;
                return CompVampire != null && CompVampire.IsVampire ? -1 :
                    CurLevel == MaxLevel ? 0 : 1;
            }
        }

        public override float CurInstantLevel => base.CurInstantLevel;

        public override float CurLevel
        {
            get { return base.CurLevel; }
            set { base.CurLevel = value; }
        }

        public float ShouldFeedPerc => 0.7f;

        public override float MaxLevel => MaxBloodPoints;

        public float BloodWanted => MaxBloodPoints - CurBloodPoints;

        public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - lastNonStarvingTick);

        public Need_Blood(Pawn pawn) : base(pawn) { }

        public override void ExposeData()
        {
            //private int curBloodPoints = Int32.MinValue;
            //private int nextBloodChangeTick = Int32.MaxValue;
            base.ExposeData();
            Scribe_Values.Look(ref lastNonStarvingTick, "lastNonStarvingTick", -1);
            Scribe_Values.Look(ref nextBloodChangeTick, "nextBloodChangeTick", -1);
            Scribe_Values.Look(ref curBloodPoints, "curBloodPoints", -1);
            Scribe_Values.Look(ref curGhoulVitaePoints, "curGhoulVitaePoints", -1);
            Scribe_Values.Look(ref bloodFixer, "bloodFixer");
            Scribe_Values.Look(ref mixedBlood, "mixedBlood");
            Scribe_Values.Look(ref preferredFeedMode, "preferredFeedMode", PreferredFeedMode.HumanoidNonLethal);
            Scribe_Values.Look(ref preferredHumanoidFeedType, "preferredHumanoidFeedType", PreferredHumanoidFeedType.PrisonersOnly);
        }
        
        public int AdjustBlood(int amt, bool alert = true, bool ghoulify = false)
        {

            int prevBloodPoints = CurBloodPoints;

            if (ghoulify)
            {
                int prevGhoulVitaePoints = CurGhoulVitaePoints;
                int potentialGhoulVitae = CurGhoulVitaePoints + amt;
                if (!MixedBlood) MixedBlood = true;
                CurGhoulVitaePoints = Mathf.Clamp(potentialGhoulVitae, 0, MaxBloodPoints);
                return prevGhoulVitaePoints - CurGhoulVitaePoints;
            }
            
            CurBloodPoints = Mathf.Clamp(CurBloodPoints + amt, 0, MaxBloodPoints);
            CurLevelPercentage = CurBloodPoints * PercPerPoint;

            if (!pawn.IsVampire(true) && CurBloodPoints < prevBloodPoints)
            {
                int diff = prevBloodPoints - CurBloodPoints;
                BloodUtility.ApplyBloodLoss(pawn, diff * PercPerPoint);
            }

            if (CurBloodPoints == 0)
                Notify_NoBloodLeft(alert);

            return prevBloodPoints - CurBloodPoints;
        }

        public void Notify_NoBloodLeft(bool alert = true)
        {
            if (pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                if (alert)
                {
                    if (pawn.Spawned && !pawn.Downed)
                    {
                        if (pawn.IsVampire(true))
                            Messages.Message("ROMV_BloodDepletedVamp".Translate(pawn.LabelCap), MessageTypeDefOf.NeutralEvent);
                        else
                            Messages.Message("ROMV_BloodDepleted".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent);   
                    }
                }
            }

            if (!pawn.IsVampire(true))
            {
                BloodUtility.ApplyBloodLoss(pawn, 999f);
                if (!pawn.Dead) pawn.Kill(null);
            }
        }

        public void TransferBloodTo(int amt, Need_Blood otherPool, bool alert = true, bool ghoulify = false)
        {
            int removedAmt = AdjustBlood(-amt);
            if (removedAmt > 0) otherPool.AdjustBlood(removedAmt, alert, ghoulify);
        }

        public bool MixedBlood
        {
            get { return mixedBlood; }
            set { mixedBlood = value; }
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

                        BloodUtility.ApplyBloodLoss(pawn, 1f);
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

        public string GetGhoulVitaeLabelAndPoints()
        {
            return GetGhoulVitaeLabel() + ": " +
                   CurGhoulVitaePercentage.ToStringPercent() +
                   " (" + CurGhoulVitaePoints.ToString("0.##") +
                   " / " +
                   MaxBloodPoints.ToString("0.##") +
                   ")\n";
        }

        public string GetGhoulVitaeLabel() => 
            pawn.IsCoolantUser() ? "ROMV_CoolantVitae".Translate() : "ROMV_Vitae".Translate();

        public string GetVitaeLabel() => 
            pawn.IsVampire(true) ? "ROMV_CoolantVitae".Translate() : "ROMV_AndroidCoolant".Translate();

        public string GetLabel(bool ghoul = false)
        {
            if (ghoul)
            {
                if (pawn.IsCoolantUser())
                    return "ROMV_Coolant" + " & " + "ROMV_CoolantVitae".Translate();
                return LabelCap + " & " +  "ROMV_Vitae".Translate();
            }
            bool isVampire = pawn.IsVampire(true);
            /// CHJEES ANDROIDS ///////////////////////////////////////////////////////
            if (pawn.IsCoolantUser())
            {
                if (isVampire)
                    return "ROMV_CoolantVitae".Translate();
                return "ROMV_Coolant".Translate();
            }
            ///////////////////////////////////////////////////////////////////////////
            if (isVampire)
                return "ROMV_Vitae".Translate();
            return LabelCap;
        }

        public string GetDescription()
        {
            if (CurGhoulVitaePoints > 0)
            {
                return GetGhoulVitaeLabelAndPoints() + "ROMV_GhoulVitaeDesc".Translate();
            }
            bool isVampire = pawn.IsVampire(true);
            /// CHJEES ANDROIDS ///////////////////////////////////////////////////////
            if (pawn.IsCoolantUser())
            {
                if (isVampire)
                    return "ROMV_CoolantVitaeDesc".Translate();
                return "ROMV_CoolantDesc".Translate();
            }
            ///////////////////////////////////////////////////////////////////////////
            if (isVampire)
                return "ROMV_VitaeDesc".Translate();
            return def.description;
        }
        
        public Color GetColorToUse(bool ghoulVitae = false)
        {
            if (ghoulVitae)
                return VampireUtility.ColorVitae;
            bool isVampire = pawn.IsVampire(true);
            if (pawn.IsCoolantUser())
            {
                if (isVampire)
                    return VampireUtility.ColorAndroidCoolantVitae;
                return VampireUtility.ColorAndroidCoolant;
            }
            if (isVampire)
                return VampireUtility.ColorVitae; //new Color(0.65f, 0.008f, 0.008f);
            return VampireUtility.ColorBlood; //new Color(0.73f, 0.02f, 0.02f);


        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null)
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

            if (pawn != null && pawn.Faction == Faction.OfPlayerSilentFail && pawn.VampComp() is CompVampire v &&
                v.IsVampire)
                BloodFeedModeUtility.DrawFeedModeButton(new Vector2(rect.width - 20f, 5f), pawn);


            if (CurGhoulVitaePoints > 0)
            {
                DrawBar(rect.TopHalf(), maxThresholdMarkers, -1f, drawArrows, BarType.GhoulTopHalf);
                DrawBar(rect.BottomHalf(), maxThresholdMarkers, -1f, pawn?.VampComp()?.IsRevenant ?? drawArrows, BarType.GhoulBottomHalf);
            }
            else
                DrawBar(rect, maxThresholdMarkers, customMargin, drawArrows);


            Text.Font = GameFont.Small;
        }


        // RimWorld.Need
        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)(barRect.width <= 60f ? 1 : 2);
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

        private enum BarType
        {
            Standard,
            GhoulTopHalf,
            GhoulBottomHalf
        }

        private void DrawBar(Rect rect, float maxThresholdMarkers = Int32.MaxValue, float customMargin = -1f, bool drawArrows = false, BarType barType = BarType.Standard)
        {
            float num2 = (barType == BarType.Standard) ? 14f : 0f;
            float num3 = customMargin < 0f ? num2 + 15f : customMargin;
            float rect2yOffset = (barType != BarType.Standard) ? 10f : 0 ;

            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = rect.height <= 55f ? GameFont.Tiny : GameFont.Small;
            Text.Anchor = TextAnchor.LowerLeft;
            if (barType != BarType.GhoulBottomHalf)
            {
                float barLabelHeightDivider = (barType == BarType.GhoulTopHalf) ? 1f : 2f;
                float rect2yOffset2 = (barType == BarType.GhoulTopHalf) ? rect.y : rect.y + rect2yOffset;
                
                Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect2yOffset2, rect.width - num3 - rect.width * 0.1f,
                    rect.height / barLabelHeightDivider);
                Widgets.Label(rect2, GetLabel(barType == BarType.GhoulTopHalf));
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float rectYPlacement = (barType != BarType.Standard && barType != BarType.GhoulTopHalf) ? rect.y : rect.y + rect.height / 2f;
            float rectHeight = (barType == BarType.Standard) ? rect.height / 2f : rect.height / 2.15f;
            Rect rect3 = new Rect(rect.x, rectYPlacement + rect2yOffset, rect.width, rectHeight);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Color
                colorToUse =
                    GetColorToUse(barType == BarType.GhoulBottomHalf); //(this.pawn?.IsVampire(true) ?? false) ? new Color(0.65f, 0.008f, 0.008f) : new Color(0.73f, 0.02f, 0.02f);
            Widgets.FillableBar(rect3, (barType == BarType.GhoulBottomHalf)? CurGhoulVitaePercentage : CurLevelPercentage, SolidColorMaterials.NewSolidColorTexture(colorToUse));
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

        }


    }
}
