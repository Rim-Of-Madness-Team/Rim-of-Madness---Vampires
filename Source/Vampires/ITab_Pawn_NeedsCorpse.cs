using RimWorld;
using Verse;
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
                if (SelPawn != null)
                {
                    return SelPawn;
                }
                Corpse corpse = SelThing as Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
                return null;
            }
        }

        public override bool IsVisible => PawnForNeeds.needs != null && PawnForNeeds.needs.AllNeeds.Count > 0;

        public ITab_Pawn_NeedsCorpse()
        {
            labelKey = "TabNeeds";
            tutorTag = "Needs";
        }

        public override void OnOpen()
        {
            thoughtScrollPosition = default(Vector2);
        }

        protected override void FillTab()
        {
            NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, size.x, size.y), PawnForNeeds, ref thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            size = NeedsCardUtility.GetSize(PawnForNeeds);
        }
    }
}
