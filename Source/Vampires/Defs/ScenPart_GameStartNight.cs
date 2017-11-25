using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace Vampire
{
    public class ScenPart_GameStartNight : ScenPart
    {
        private string text;

        private string textKey;

        private SoundDef closeSound;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 5f);
            this.text = Widgets.TextArea(scenPartRect, this.text, false);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.text, "text", null, false);
            Scribe_Values.Look<string>(ref this.textKey, "textKey", null, false);
            Scribe_Defs.Look<SoundDef>(ref this.closeSound, "closeSound");
        }

        public override void PostGameStart()
        {
            Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 32500);
        }
    }
}
