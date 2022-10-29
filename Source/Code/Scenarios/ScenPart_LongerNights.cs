using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class ScenPart_LongerNights : ScenPart
{
    public float nightsLength = 0.1f;

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        var scenPartRect = listing.GetScenPartRect(this, RowHeight * 2 + 31f);
        DoVampModifierEditInterface(new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, 31f));
    }

    // RimWorld.ScenPart_PawnModifier
    protected void DoVampModifierEditInterface(Rect rect)
    {
        var rect2 = new Rect(rect.x, rect.y, rect.width, 31 * 3);
        var rect3 = rect2.LeftPart(0.4f).Rounded();
        var rect4 = rect2.RightPart(0.6f).Rounded();

        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(rect3, "ROMV_NightLength".Translate().CapitalizeFirst());

        Text.Anchor = TextAnchor.UpperLeft;
        nightsLength = Widgets.HorizontalSlider(rect4, nightsLength, 0f, 1f, false, nightsLength.ToStringPercent(), "",
            "", 0.1f);
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref nightsLength, "nightsLength", 0.1f);
    }

    public override string Summary(Scenario scen)
    {
        return GetLongerNightsSummary().CapitalizeFirst();
    }

    public string GetLongerNightsSummary()
    {
        return this?.nightsLength == 1.0f
            ? "ROMV_EternalDarkness".Translate()
            : "ROMV_NightLengthBy".Translate(nightsLength.ToStringPercent());
    }

    public override void Randomize()
    {
        base.Randomize();
        nightsLength = Rand.Range(0.1f, 1.0f);
    }
}