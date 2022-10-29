using System.Linq;
using Verse;

namespace Vampire;

public class HediffComp_Hidden : HediffComp_Disappears
{
    private bool activated;

    private IntVec3 startPosition = IntVec3.Invalid;
    public bool Activated => activated;

    public Graphic BodyGraphic { get; set; } = null;

    public new HediffCompProperties_Hidden Props => (HediffCompProperties_Hidden)props;

    public override bool CompShouldRemove => base.CompShouldRemove || Pawn.VampComp().CurrentForm != null ||
                                             (!Props.canMove && startPosition != IntVec3.Invalid &&
                                              Pawn.PositionHeld != startPosition) ||
                                             Pawn?.MapHeld?.GetComponent<MapComponent_HiddenTracker>()?.hiddenCharacters
                                                 ?.FirstOrDefault(x => x == Pawn) == null;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (!activated)
        {
            activated = true;
            startPosition = Pawn.PositionHeld;
            if (Pawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                    x != parent && x.TryGetComp<HediffComp_Hidden>() != null) is HediffWithComps h)
                Pawn.health.hediffSet.hediffs.Remove(h);
            Pawn.VampComp().CurrentForm = null;
            Pawn.VampComp().CurFormGraphic = null;
            Pawn.Map.GetComponent<MapComponent_HiddenTracker>()?.Notify_AddHiddenCharacter(Pawn);
        }

        if (CompShouldRemove) Pawn.Map.GetComponent<MapComponent_HiddenTracker>()?.Notify_RemoveHiddenCharacter(Pawn);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        Pawn?.Map?.GetComponent<MapComponent_HiddenTracker>()?.Notify_RemoveHiddenCharacter(Pawn);
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref activated, "activated");
    }
}