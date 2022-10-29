using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

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
///     Duplicate of Need_Food
/// </summary>
public class Need_Blood : Need
{
    public Need_Blood(Pawn pawn) : base(pawn)
    {
    }

    public override int GUIChangeArrow
    {
        get
        {
            if (CompVampire is { } compVampire && compVampire.IsRevenant && CurLevel != MaxLevel)
                return 1;
            return CompVampire != null && CompVampire.IsVampire ? -1 :
                CurLevel == MaxLevel ? 0 : 1;
        }
    }

    public override float CurInstantLevel => base.CurInstantLevel;

    public override float CurLevel
    {
        get => base.CurLevel;
        set => base.CurLevel = value;
    }

    public float ShouldFeedPerc => 0.7f;

    public override float MaxLevel => MaxBloodPoints;

    public float BloodWanted => MaxBloodPoints - CurBloodPoints;

    public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - lastNonStarvingTick);

    public bool MixedBlood
    {
        get => mixedBlood;
        set => mixedBlood = value;
    }

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
        Scribe_Values.Look(ref preferredHumanoidFeedType, "preferredHumanoidFeedType",
            PreferredHumanoidFeedType.PrisonersOnly);
    }

    public int AdjustBlood(int amt, bool alert = true, bool ghoulify = false)
    {
        var prevBloodPoints = CurBloodPoints;

        if (ghoulify)
        {
            var prevGhoulVitaePoints = CurGhoulVitaePoints;
            var potentialGhoulVitae = CurGhoulVitaePoints + amt;
            if (!MixedBlood) MixedBlood = true;
            CurGhoulVitaePoints = Mathf.Clamp(potentialGhoulVitae, 0, MaxBloodPoints);
            return prevGhoulVitaePoints - CurGhoulVitaePoints;
        }

        CurBloodPoints = Mathf.Clamp(CurBloodPoints + amt, 0, MaxBloodPoints);
        CurLevelPercentage = CurBloodPoints * PercPerPoint;

        if (!pawn.IsVampire(true) && CurBloodPoints < prevBloodPoints)
        {
            var diff = prevBloodPoints - CurBloodPoints;
            BloodUtility.ApplyBloodLoss(pawn, diff * PercPerPoint);
        }

        if (CurBloodPoints == 0)
            Notify_NoBloodLeft(alert);

        return prevBloodPoints - CurBloodPoints;
    }

    public void Notify_NoBloodLeft(bool alert = true)
    {
        if (pawn != null && pawn.Faction == Faction.OfPlayer)
            if (alert)
                if (pawn.Spawned && !pawn.Downed)
                {
                    if (pawn.IsVampire(true))
                        Messages.Message("ROMV_BloodDepletedVamp".Translate(pawn.LabelCap),
                            MessageTypeDefOf.NeutralEvent);
                    else
                        Messages.Message("ROMV_BloodDepleted".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent);
                }

        if (!pawn.IsVampire(true))
        {
            BloodUtility.ApplyBloodLoss(pawn, 999f);
            if (!pawn.Dead) pawn.Kill(null);
        }
    }

    public void TransferBloodTo(int amt, Need_Blood otherPool, bool alert = true, bool ghoulify = false)
    {
        var removedAmt = AdjustBlood(-amt);
        if (removedAmt > 0) otherPool.AdjustBlood(removedAmt, alert, ghoulify);
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
            var math = Find.TickManager.TicksGame + GenDate.TicksPerDay;
            //Log.Message("BLOOD TICKS SET TO => " + math);
            nextBloodChangeTick = math;
            AdjustBlood(BloodChangePerDay);
        }

        if (!Starving) lastNonStarvingTick = Find.TickManager.TicksGame;

        if (!IsFrozen)
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

    public override void SetInitialLevel()
    {
        //base.CurLevelPercentage = 1.0f;
        CurLevel = CurBloodPoints = MaxBloodPoints;
        if (Current.ProgramState == ProgramState.Playing) lastNonStarvingTick = Find.TickManager.TicksGame;
    }

    public override string GetTipString()
    {
        return string.Concat(GetLabel(), ": ", CurLevelPercentage.ToStringPercent(), " (", CurLevel.ToString("0.##"),
            " / ", MaxLevel.ToString("0.##"), ")\n", GetDescription());
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

    public string GetGhoulVitaeLabel()
    {
        return pawn.IsCoolantUser() ? "ROMV_CoolantVitae".Translate() : "ROMV_Vitae".Translate();
    }

    public string GetVitaeLabel()
    {
        return pawn.IsVampire(true) ? "ROMV_CoolantVitae".Translate() : "ROMV_AndroidCoolant".Translate();
    }

    public string GetLabel(bool ghoul = false)
    {
        if (ghoul)
        {
            if (pawn.IsCoolantUser())
                return "ROMV_Coolant" + " & " + "ROMV_CoolantVitae".Translate();
            return LabelCap + " & " + "ROMV_Vitae".Translate();
        }

        var isVampire = pawn.IsVampire(true);
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
        if (CurGhoulVitaePoints > 0) return GetGhoulVitaeLabelAndPoints() + "ROMV_GhoulVitaeDesc".Translate();
        var isVampire = pawn.IsVampire(true);
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
        var isVampire = pawn.IsVampire(true);
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

    public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true,
        bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
    {
        if (threshPercents == null) threshPercents = new List<float>();
        threshPercents.Clear();
        for (var i = 1; i < MaxBloodPoints; i++) threshPercents.Add(PercPerPoint * i);
        if (rect.height > 70f)
        {
            var num = (rect.height - 70f) / 2f;
            rect.height = 70f;
            rect.y += num;
        }

        if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);
        if (doTooltip && Mouse.IsOver(rect)) 
            TooltipHandler.TipRegion(rect, new TipSignal(() => GetTipString(), rect.GetHashCode()));

        float num2 = 14f;
        float num3 = (customMargin >= 0f) ? customMargin : (num2 + 15f);
        if (rect.height < 50f)
        {
            num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
        }
        if (drawLabel)
        {
            Text.Font = ((rect.height > 55f) ? GameFont.Small : GameFont.Tiny);
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f), GetLabel(CurGhoulVitaePoints > 0));
            Text.Anchor = TextAnchor.UpperLeft;
        }
        Rect rect3 = rect;
        if (drawLabel)
        {
            rect3.y += rect.height / 2f;
            rect3.height -= rect.height / 2f;
        }
        rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
        if (DebugSettings.ShowDevGizmos)
        {
            float lineHeight = Text.LineHeight;
            Rect rect4 = new Rect(rect3.xMax - lineHeight, rect3.y - lineHeight, lineHeight, lineHeight);
            if (Widgets.ButtonImage(rect4.ContractedBy(4f), Verse.TexButton.Plus, true))
            {
                this.CurLevelPercentage += 0.1f;
            }
            if (Mouse.IsOver(rect4))
            {
                TooltipHandler.TipRegion(rect4, "+ 10%");
            }
            Rect rect5 = new Rect(rect4.xMin - lineHeight, rect3.y - lineHeight, lineHeight, lineHeight);
            if (Widgets.ButtonImage(rect5.ContractedBy(4f), Verse.TexButton.Minus, true))
            {
                this.CurLevelPercentage -= 0.1f;
            }
            if (Mouse.IsOver(rect5))
            {
                TooltipHandler.TipRegion(rect5, "- 10%");
            }
        }

        if (pawn != null && pawn.Faction == Faction.OfPlayerSilentFail && pawn.VampComp() is { } v &&
            v.IsVampire)
            BloodFeedModeUtility.DrawFeedModeButton(new Vector2(rect.width - 20f, 5f), pawn);


        if (CurGhoulVitaePoints > 0)
        {
            DrawBar(rect.TopHalf(), maxThresholdMarkers, -1f, drawArrows, BarType.GhoulTopHalf);
            DrawBar(rect.BottomHalf(), maxThresholdMarkers, -1f, pawn?.VampComp()?.IsRevenant ?? drawArrows,
                BarType.GhoulBottomHalf);
        }
        else
        {
            DrawBar(rect, maxThresholdMarkers, customMargin, drawArrows);
        }


        Text.Font = GameFont.Small;
    }


    // RimWorld.Need
    private void DrawBarThreshold(Rect barRect, float threshPct)
    {
        float num = barRect.width <= 60f ? 1 : 2;
        var position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f,
            num, barRect.height / 2f);
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

    private void DrawBar(Rect rect, float maxThresholdMarkers = int.MaxValue, float customMargin = -1f,
        bool drawArrows = false, BarType barType = BarType.Standard)
    {
        var num2 = barType == BarType.Standard ? 14f : 0f;
        var num3 = customMargin < 0f ? num2 + 15f : customMargin;
        var rect2yOffset = barType != BarType.Standard ? 10f : 0;

        if (rect.height < 50f) num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
        Text.Font = rect.height <= 55f ? GameFont.Tiny : GameFont.Small;
        Text.Anchor = TextAnchor.LowerLeft;

        Text.Anchor = TextAnchor.UpperLeft;
        var rectYPlacement = barType != BarType.Standard && barType != BarType.GhoulTopHalf
            ? rect.y
            : rect.y + rect.height / 2f;
        var rectHeight = barType == BarType.Standard ? rect.height / 2f : rect.height / 2.15f;
        var rect3 = new Rect(rect.x, rectYPlacement + rect2yOffset, rect.width, rectHeight);
        rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
        var
            colorToUse =
                GetColorToUse(barType ==
                              BarType
                                  .GhoulBottomHalf); //(this.pawn?.IsVampire(true) ?? false) ? new Color(0.65f, 0.008f, 0.008f) : new Color(0.73f, 0.02f, 0.02f);
        Widgets.FillableBar(rect3, barType == BarType.GhoulBottomHalf ? CurGhoulVitaePercentage : CurLevelPercentage,
            SolidColorMaterials.NewSolidColorTexture(colorToUse));
        //Widgets.FillableBar(rect3, this.CurLevelPercentage);
        if (drawArrows) Widgets.FillableBarChangeArrows(rect3, GUIChangeArrow);
        if (threshPercents != null)
            for (var i = 0; i < Mathf.Min(threshPercents.Count, maxThresholdMarkers); i++)
                DrawBarThreshold(rect3, threshPercents[i]);
        var curInstantLevelPercentage = CurInstantLevelPercentage;
        if (curInstantLevelPercentage >= 0f) DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
        if (!def.tutorHighlightTag.NullOrEmpty()) UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
    }

    private enum BarType
    {
        Standard,
        GhoulTopHalf,
        GhoulBottomHalf
    }

    #region Variables

    private int curBloodPoints = int.MinValue;
    private int nextBloodChangeTick = -1;
    private int lastNonStarvingTick = -1;
    private bool bloodFixer;
    public PreferredFeedMode preferredFeedMode = PreferredFeedMode.HumanoidNonLethal;
    public PreferredHumanoidFeedType preferredHumanoidFeedType = PreferredHumanoidFeedType.All;

    //Variables related to Ghouls
    private bool mixedBlood;
    private int curGhoulVitaePoints = int.MinValue;

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
                   (pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(x => x.def == BloodUtility.GetBloodLossDef(pawn))
                       is { } bloodLoss && bloodLoss.CurStageIndex > 2);
        }
    }

    public bool BloodFixer
    {
        get => bloodFixer;
        set => bloodFixer = value;
    }

    public int NextBloodChangeTick
    {
        get
        {
            if (nextBloodChangeTick == int.MaxValue)
                nextBloodChangeTick = Find.TickManager.TicksGame + GenDate.TicksPerDay;
            return nextBloodChangeTick;
        }
    }

    public int CurBloodPoints
    {
        get
        {
            if (curBloodPoints == int.MinValue)
                curBloodPoints = MaxBloodPoints;
            return curBloodPoints;
        }
        set => curBloodPoints = value;
    }

    public int CurGhoulVitaePoints
    {
        get
        {
            if (curGhoulVitaePoints == int.MinValue)
                curGhoulVitaePoints = 0;
            return curGhoulVitaePoints;
        }
        set => curGhoulVitaePoints = Mathf.Clamp(value, 0, MaxBloodPoints);
    }

    public float CurGhoulVitaePercentage => CurGhoulVitaePoints * PercPerPoint;

    public int MaxBloodPointsForAnimal(Pawn p)
    {
        var def = p.kindDef;
        var result = def.RaceProps.baseBodySize < 1f ? 1 : 2;
        if (def?.RaceProps?.trainability == TrainabilityDefOf.Advanced)
            result += 1;
        return result;
    }

    public int MaxBloodPoints
    {
        get
        {
            if (VampireSettings.GetBloodPointOverride(pawn.def) is { } overrideResult && overrideResult != -1)
                return overrideResult;

            var result = 7;

            if (IsAnimal)
                result = MaxBloodPointsForAnimal(pawn);

            if (pawn.IsVampire(true))
            {
                var gen = CompVampire.Generation;
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
                //if (CompVampire.Generation < 7)
                //{
                //    return -3;
                //}
                return -1;
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

    #endregion Properties
}