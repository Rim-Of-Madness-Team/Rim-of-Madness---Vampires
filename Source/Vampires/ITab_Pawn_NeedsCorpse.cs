using RimWorld;
using Verse;
using System;
using UnityEngine;

namespace Vampire
{
    public class ITab_Pawn_NeedsCorpse : ITab
    {
        private Vector2 thoughtScrollPosition;

        private Pawn PawnForNeeds
        {
            get
            {
                if (base.SelPawn != null)
                {
                    return base.SelPawn;
                }
                Corpse corpse = base.SelThing as Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
                return null;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return PawnForNeeds.needs != null && PawnForNeeds.needs.AllNeeds.Count > 0;
            }
        }

        public ITab_Pawn_NeedsCorpse()
        {
            this.labelKey = "TabNeeds";
            this.tutorTag = "Needs";
        }

        public override void OnOpen()
        {
            this.thoughtScrollPosition = default(Vector2);
        }

        protected override void FillTab()
        {
            NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), PawnForNeeds, ref this.thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            this.size = NeedsCardUtility.GetSize(PawnForNeeds);
        }
    }
}
