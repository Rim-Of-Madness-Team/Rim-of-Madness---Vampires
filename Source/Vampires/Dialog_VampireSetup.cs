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
                new Rect(0f, inRect.width * 0.5f + header.GetWidthCached(), inRect.width, 45f
                ), header);
            Text.Font = GameFont.Small;


            //GUI.BeginGroup(inRect);

            Rect radioRect = new Rect(0f, 40f, inRect.width, inRect.height - 36);
            selectedVampireInfoListing.Begin(radioRect);
            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Disabled".Translate(),
                VampireSettingsInit.mode == GameMode.Disabled, 0f,
                "ROMV_GameMode_DisabledDesc".Translate()))
            {
                VampireSettingsInit.mode = GameMode.Disabled;
            }
            selectedVampireInfoListing.Gap(6f);
            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_EventsOnly".Translate(),
                VampireSettingsInit.mode == GameMode.EventsOnly,
                0f, "ROMV_GameMode_EventsOnlyDesc".Translate()))
            {
                VampireSettingsInit.mode = GameMode.EventsOnly;
            }
            selectedVampireInfoListing.Gap(6f);

            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Standard".Translate(),
                VampireSettingsInit.mode == GameMode.Standard, 0f, "ROMV_GameMode_StandardDesc".Translate()))
            {
                VampireSettingsInit.mode = GameMode.Standard;
            }
            selectedVampireInfoListing.Gap(6f);

            if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Custom".Translate(),
                VampireSettingsInit.mode == GameMode.Custom, 0f, "ROMV_GameMode_CustomDesc".Translate()))
            {
                VampireSettingsInit.mode = GameMode.Custom;
            }

            selectedVampireInfoListing.Gap(24f);
            VampireSettingsInit.spawnPct = //selectedVampireInfoListing.Slider(1f, 0f, 100f) * 0.01f;
                Widgets.HorizontalSlider(selectedVampireInfoListing.GetRect(22f), VampireSettingsInit.spawnPct, 0f, 100f, false,
                    "ROMV_Slider_GlobalSpread".Translate(), "ROMV_Slider_GlobalSpreadLeft".Translate(),
                    "ROMV_Slider_GlobalSpreadRight".Translate(), 1f) * 0.01f;
            selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);
            selectedVampireInfoListing.Gap(24f);
            VampireSettingsInit.lowestActiveVampGen = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
                (int) Widgets.HorizontalSlider(selectedVampireInfoListing.GetRect(22f),
                    VampireSettingsInit.lowestActiveVampGen, 0f, 100f, false,
                    "ROMV_Slider_GlobalSpread".Translate(), "ROMV_Slider_GlobalSpreadLeft".Translate(),
                    "ROMV_Slider_GlobalSpreadRight".Translate(), 1f);
            
            selectedVampireInfoListing.Gap(selectedVampireInfoListing.verticalSpacing);

            selectedVampireInfoListing.End();

            //if (selectedVampireInfoListing.)

            Rect acceptButton = new Rect((inRect.width * 0.5f), inRect.height - (38f * 2), 150, 38);
            if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
            {
                VampireSettingsInit.ApplySettings();
                this.Close(false);
            }
            //GUI.EndGroup();
        }
    }
}