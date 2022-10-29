using RimWorld;
using Verse;

namespace Vampire;

public class HediffComp_Possession : HediffComp_Disappears
{
    public new HediffCompProperties_Possession Props => (HediffCompProperties_Possession)props;

    public void ActivateEffect(Pawn activator)
    {
        var text = Pawn.LabelIndefinite();
        if (Pawn.IsVampire(true))
        {
            Messages.Message("ROMV_CannotPossessVamps".Translate(), MessageTypeDefOf.RejectInput);
            return;
        }

        if (Pawn.guest != null) Pawn.guest.SetGuestStatus(null);
        var flag = Pawn.Name != null;
        if (Pawn.Faction != activator.Faction) Pawn.SetFaction(activator.Faction, Pawn);
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (CompShouldRemove) HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("HeartAttack"), 1.0f);
    }
}