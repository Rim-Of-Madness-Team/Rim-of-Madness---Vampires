using UnityEngine;
using Verse;

namespace Vampire
{
    public class Dialog_VampireSetup : Window
    {
        private Listing_Standard selectedVampireInfoListing = new Listing_Standard();
        private string header = "Vampire Settings";


        public override void DoWindowContents(Rect inRect)
        {
            //Title
            Text.Font = GameFont.Medium;
            Widgets.Label(
                new Rect((inRect.width * 0.5f) - (header.GetWidthCached() * 0.5f), 0f, inRect.width, 45f
                ), header);
            Text.Font = GameFont.Small;


            //GUI.BeginGroup(inRect);

            Rect radioRect = new Rect(0f, 40f, inRect.width, inRect.height - 36);
            selectedVampireInfoListing.Begin(radioRect);
            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Disabled".Translate(),
                VampireSettingsInit.Get.mode == GameMode.Disabled, 0f,
                "ROMV_GameMode_DisabledDesc".Translate()))
            {
                VampireSettingsInit.Get.mode = GameMode.Disabled;
            }

            selectedVampireInfoListing.Gap(6f);
            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_EventsOnly".Translate(),
                VampireSettingsInit.Get.mode == GameMode.EventsOnly,
                0f, "ROMV_GameMode_EventsOnlyDesc".Translate()))
            {
                VampireSettingsInit.Get.mode = GameMode.EventsOnly;
            }

            selectedVampireInfoListing.Gap(6f);

            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Standard".Translate(),
                VampireSettingsInit.Get.mode == GameMode.Standard, 0f, "ROMV_GameMode_StandardDesc".Translate()))
            {
                VampireSettingsInit.Get.mode = GameMode.Standard;
            }

            selectedVampireInfoListing.Gap(6f);

            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Custom".Translate(),
                VampireSettingsInit.Get.mode == GameMode.Custom, 0f, "ROMV_GameMode_CustomDesc".Translate()))
            {
                VampireSettingsInit.Get.mode = GameMode.Custom;
            }

            if (VampireSettingsInit.Get.mode == GameMode.Custom)
            {
                selectedVampireInfoListing.Gap(24f);
                var spawnPctRect = selectedVampireInfoListing.GetRect(22f);
                VampireSettingsInit.Get.spawnPct = //selectedVampireInfoListing.Slider(1f, 0f, 100f) * 0.01f;
                    Widgets.HorizontalSlider(spawnPctRect, VampireSettingsInit.Get.spawnPct, 0.01f, 1f, true,
                        "ROMV_Slider_GlobalSpread".Translate() + ": " + VampireSettingsInit.Get.spawnPct.ToStringPercent(),
                        "ROMV_Slider_GlobalSpreadLeft".Translate(),
                        "ROMV_Slider_GlobalSpreadRight".Translate(), -1f);
                selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);
                TooltipHandler.TipRegion(spawnPctRect, () => "ROMV_Slider_GlobalSpreadTooltip".Translate(), 9513127);
                selectedVampireInfoListing.Gap(24f);
                var lowestActiveVampGenRect = selectedVampireInfoListing.GetRect(22f);
                VampireSettingsInit.Get.lowestActiveVampGen = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                    (int) Widgets.HorizontalSlider(lowestActiveVampGenRect,
                        VampireSettingsInit.Get.lowestActiveVampGen, 4f, 14f, false,
                        "ROMV_Slider_LowestActiveGen".Translate() + ": " + VampireSettingsInit.Get.lowestActiveVampGen,
                        VampireGen.VAMP_MINSPAWNGENERATION.ToString(),
                        VampireGen.VAMP_MAXSPAWNGENERATION.ToString(), 1f);
                TooltipHandler.TipRegion(lowestActiveVampGenRect, () => "ROMV_Slider_LowestActiveGenTooltip".Translate(), 57438827);
                selectedVampireInfoListing.Gap(24f);
                var sunDimmingRect = selectedVampireInfoListing.GetRect(22f);
                VampireSettingsInit.Get.sunDimming = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                    Widgets.HorizontalSlider(sunDimmingRect,
                        VampireSettingsInit.Get.sunDimming, 0f, 1f, false,
                        "ROMV_Slider_SunDimming".Translate() + ": " + VampireSettingsInit.Get.sunDimming,
                        0f.ToStringPercent(),
                        1f.ToStringPercent(), -1f);
                TooltipHandler.TipRegion(sunDimmingRect, () => "ROMV_Slider_SunDimmingTooltip".Translate(), 75433216);
                selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);
            }

            selectedVampireInfoListing.End();

            //if (selectedVampireInfoListing.)

            Rect acceptButton = new Rect((inRect.width * 0.5f), inRect.height - (38f * 2), 150, 38);
            if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
            {
                VampireSettingsInit.Get.ApplySettings();
                this.Close(false);
            }

            //GUI.EndGroup();
        }
    }
}