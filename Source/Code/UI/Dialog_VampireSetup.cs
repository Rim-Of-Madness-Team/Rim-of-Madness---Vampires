using UnityEngine;
using Verse;

namespace Vampire;

public class Dialog_VampireSetup : Window
{
    private readonly string header = "Vampire Settings";
    private readonly Listing_Standard selectedVampireInfoListing = new();


    public override void DoWindowContents(Rect inRect)
    {
        //Title
        Text.Font = GameFont.Medium;
        Widgets.Label(
            new Rect(inRect.width * 0.5f - header.GetWidthCached() * 0.5f, 0f, inRect.width, 45f
            ), header);
        Text.Font = GameFont.Small;


        //GUI.BeginGroup(inRect);

        var radioRect = new Rect(0f, 40f, inRect.width, inRect.height - 36);
        selectedVampireInfoListing.Begin(radioRect);
        if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Disabled".Translate(),
                VampireSettings.Get.mode == GameMode.Disabled, 0f,
                "ROMV_GameMode_DisabledDesc".Translate()))
        {
            VampireSettings.Get.mode = GameMode.Disabled;
            VampireSettings.Get.ApplySettings();
        }

        selectedVampireInfoListing.Gap(6f);
        if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_EventsOnly".Translate(),
                VampireSettings.Get.mode == GameMode.EventsOnly,
                0f, "ROMV_GameMode_EventsOnlyDesc".Translate()))
        {
            VampireSettings.Get.mode = GameMode.EventsOnly;
            VampireSettings.Get.ApplySettings();
        }

        selectedVampireInfoListing.Gap(6f);

        if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Standard".Translate(),
                VampireSettings.Get.mode == GameMode.Standard, 0f, "ROMV_GameMode_StandardDesc".Translate()))
        {
            VampireSettings.Get.mode = GameMode.Standard;
            VampireSettings.Get.ApplySettings();
        }

        selectedVampireInfoListing.Gap(6f);

        if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Custom".Translate(),
                VampireSettings.Get.mode == GameMode.Custom, 0f, "ROMV_GameMode_CustomDesc".Translate()))
        {
            VampireSettings.Get.mode = GameMode.Custom;
            VampireSettings.Get.ApplySettings();
        }

        if (VampireSettings.Get.mode == GameMode.Custom)
        {
            selectedVampireInfoListing.Gap(24f);
            var spawnPctRect = selectedVampireInfoListing.GetRect(22f);
            VampireSettings.Get.spawnPct = //selectedVampireInfoListing.Slider(1f, 0f, 100f) * 0.01f;
                Widgets.HorizontalSlider(spawnPctRect, VampireSettings.Get.spawnPct, 0.01f, 1f, true,
                    "ROMV_Slider_GlobalSpread".Translate() + ": " + VampireSettings.Get.spawnPct.ToStringPercent(),
                    "ROMV_Slider_GlobalSpreadLeft".Translate(),
                    "ROMV_Slider_GlobalSpreadRight".Translate());
            selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);
            TooltipHandler.TipRegion(spawnPctRect, () => "ROMV_Slider_GlobalSpreadTooltip".Translate(), 9513127);
            selectedVampireInfoListing.Gap(24f);
            var lowestActiveVampGenRect = selectedVampireInfoListing.GetRect(22f);
            VampireSettings.Get.lowestActiveVampGen = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                (int)Widgets.HorizontalSlider(lowestActiveVampGenRect,
                    VampireSettings.Get.lowestActiveVampGen, 4f, 14f, false,
                    "ROMV_Slider_LowestActiveGen".Translate() + ": " + VampireSettings.Get.lowestActiveVampGen,
                    VampireGen.VAMP_MINSPAWNGENERATION.ToString(),
                    VampireGen.VAMP_MAXSPAWNGENERATION.ToString(), 1f);
            TooltipHandler.TipRegion(lowestActiveVampGenRect, () => "ROMV_Slider_LowestActiveGenTooltip".Translate(),
                57438827);
            selectedVampireInfoListing.Gap(24f);
            var sunDimmingRect = selectedVampireInfoListing.GetRect(22f);
            VampireSettings.Get.sunDimming = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                Widgets.HorizontalSlider(sunDimmingRect,
                    VampireSettings.Get.sunDimming, 0f, 1f, false,
                    "ROMV_Slider_SunDimming".Translate() + ": " + VampireSettings.Get.sunDimming,
                    0f.ToStringPercent(),
                    1f.ToStringPercent());
            TooltipHandler.TipRegion(sunDimmingRect, () => "ROMV_Slider_SunDimmingTooltip".Translate(), 75433216);
            selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);
        }

        selectedVampireInfoListing.End();

        //if (selectedVampireInfoListing.)

        var acceptButton = new Rect(inRect.width * 0.5f, inRect.height - 38f * 2, 150, 38);
        if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
        {
            VampireSettings.Get.ApplySettings();
            Close(false);
        }

        //GUI.EndGroup();
    }
}