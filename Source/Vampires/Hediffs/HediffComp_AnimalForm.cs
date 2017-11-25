using System;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;

namespace Vampire
{
    public class HediffComp_AnimalForm : HediffComp_Disappears
    {
        private bool activated = false;
        public bool Activated => activated;

        private Graphic bodyGraphic = null;
        public Graphic BodyGraphic { get => bodyGraphic; set => bodyGraphic = value; }

        public new HediffCompProperties_AnimalForm Props
        {
            get
            {
                return (HediffCompProperties_AnimalForm)this.props;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine("ROMV_HI_BodySize".Translate(Props.animalToChangeInto.RaceProps.baseBodySize.ToStringPercent()));
                s.AppendLine("ROMV_HI_HealthScale".Translate(Props.animalToChangeInto.RaceProps.baseHealthScale.ToStringPercent()));
                return s.ToString();
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!activated)
            {
                activated = true;
                if (this.Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x != this.parent && x.TryGetComp<HediffComp_AnimalForm>() != null) is HediffWithComps h)
                {
                    this.Pawn.health.hediffSet.hediffs.Remove(h);
                }
                this.Pawn.VampComp().CurrentForm = this.Props.animalToChangeInto;
                this.Pawn.VampComp().CurFormGraphic = null;

                //Log.Message("CurrentForm set to " + this.Props.animalToChangeInto.label);
            }
            if (CompShouldRemove)
            {
                this.Pawn.VampComp().CurrentForm = null;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.activated, "activated", false);
        }
    }
}
