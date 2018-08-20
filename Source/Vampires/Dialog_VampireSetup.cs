using UnityEngine;
using Verse;

namespace Vampire
{
    public class Dialog_VampireSetup : Window
    {
        private Listing_Standard selectedVampireInfoListing = new Listing_Standard();


        public override void DoWindowContents(Rect inRect)
        {
            //Title
            Text.Font = GameFont.Medium;
            Widgets.Label(
                new Rect(0f, 0f, inRect.width, 45f
                ), "Vampire Settings");
            Text.Font = GameFont.Small;


            GUI.BeginGroup(inRect);
            Rect radioRect = new Rect(0f, 18f, inRect.width, inRect.height - 36);
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
            
            //if (selectedVampireInfoListing.)

            Rect acceptButton = new Rect((inRect.width * 0.5f), inRect.height - 38f, 150, 38);
            if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
            {
                this.Close(false);
            }
            GUI.EndGroup();
        }
    }
}