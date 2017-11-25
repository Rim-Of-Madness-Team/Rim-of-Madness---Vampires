using System;
using RimWorld;
using Verse;

namespace Vampire
{
    public class HediffComp_Possession : HediffComp_Disappears
    {

        public new HediffCompProperties_Possession Props
        {
            get
            {
                return (HediffCompProperties_Possession)this.props;
            }
        }
        
        public void ActivateEffect(Pawn activator)
        {
            string text = this.Pawn.LabelIndefinite();
            if (this.Pawn.guest != null)
            {
                this.Pawn.guest.SetGuestStatus(null, false);
            }
            bool flag = this.Pawn.Name != null;
            if (this.Pawn.Faction != activator.Faction)
            {
                this.Pawn.SetFaction(activator.Faction, this.Pawn);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (CompShouldRemove)
            {
                HealthUtility.AdjustSeverity(this.Pawn, HediffDef.Named("HeartAttack"), 1.0f);
            }
        }
    }
}
