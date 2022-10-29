using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class ITab_Pawn_NeedsCorpse : ITab
{
    private Vector2 thoughtScrollPosition;

    public ITab_Pawn_NeedsCorpse()
    {
        labelKey = "TabNeeds";
        tutorTag = "Needs";
    }

    private Pawn PawnForNeeds
    {
        get
        {
            if (SelPawn != null) return SelPawn;
            var corpse = SelThing as Corpse;
            if (corpse != null) return corpse.InnerPawn;
            return null;
        }
    }

    public override bool IsVisible => PawnForNeeds.needs != null && PawnForNeeds.needs.AllNeeds.Count > 0;

    public override void OnOpen()
    {
        thoughtScrollPosition = default;
    }

    protected override void FillTab()
    {
        NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, size.x, size.y), PawnForNeeds,
            ref thoughtScrollPosition);
    }

    protected override void UpdateSize()
    {
        size = NeedsCardUtility.GetSize(PawnForNeeds);
    }
}