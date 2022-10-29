using System;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire;

public class HediffVampirismGenerationBonus : HediffWithComps
{
    public override string LabelBase
    {
        get
        {
            if (pawn.VampComp().Generation != -1)
                return base.LabelBase + " (" +
                       "ROMV_HI_Generation".Translate(VampireStringUtility.AddOrdinal(pawn.VampComp().Generation)) +
                       ")";
            return base.LabelBase;
        }
    }


    public override string TipStringExtra
    {
        get
        {
            var s = new StringBuilder();
            try
            {
                var painFactor = def.stages[0].painFactor.ToStringPercent();
                var sensesFactor = def.stages[0].capMods.First().offset.ToStringPercent();
                s.AppendLine("ROMV_HI_Pain".Translate(painFactor));
                s.AppendLine("ROMV_HI_Senses".Translate(sensesFactor));
                s.AppendLine("ROMV_HI_Vigor".Translate(sensesFactor));
                s.AppendLine("ROMV_HI_Immunities".Translate());
                if (!comps.NullOrEmpty())
                    foreach (var compProps in comps)
                    {
                        if (compProps is JecsTools.HediffComp_DamageSoak dmgSoak)
                            s.AppendLine(dmgSoak.CompTipStringExtra);
                        if (compProps is JecsTools.HediffComp_ExtraMeleeDamages xDmg)
                            s.AppendLine(xDmg.CompTipStringExtra);
                    }
            }
            catch (NullReferenceException)
            {
                //Log.Message(e.ToString());
            }

            return s.ToString();
        }
    }

    public override bool ShouldRemove => def != pawn.GenerationDef();

    public override void PostRemoved()
    {
        base.PostRemoved();
        if (def != pawn.GenerationDef()) HealthUtility.AdjustSeverity(pawn, pawn.GenerationDef(), 1.0f);
    }
}