using System.Text;
using Verse;

namespace Vampire;

public enum GhoulPower
{
    Modern = 0,
    Old = 1,
    Ancient = 2,
    Primeval = 3
}

public enum GhoulType
{
    Standard,
    Independent,
    Revenant
}

public class HediffGhoul : HediffWithComps
{
    public BloodlineDef bloodline = null;
    public Pawn domitor;
    public GhoulPower ghoulPower = GhoulPower.Modern;
    public GhoulType ghoulType = GhoulType.Standard;
    private bool initialized;

    public override bool ShouldRemove => initialized && pawn.BloodNeed().CurGhoulVitaePoints == 0;

    public override string TipStringExtra
    {
        get
        {
            var s = new StringBuilder();

            var thrallData = pawn?.VampComp()?.ThrallData;
            if (thrallData == null)
                return s.ToString();
            s.AppendLine("ROMV_HI_GhoulDomitor".Translate(domitor?.LabelCap + " (" +
                VampireStringUtility.AddOrdinal(domitor?.VampComp().Generation ?? -1) + ")" ?? "Unknown"));
            s.AppendLine("ROMV_HI_Bloodline".Translate(domitor?.VampComp()?.Bloodline.LabelCap ??
                                                       this?.bloodline?.label ?? "Unknown"));
            if (thrallData.BondStage is { } bondStage)
            {
                s.AppendLine("ROMV_HI_BondStage".Translate());
                s.AppendLine("   " + bondStage.GetString());
            }

            if (ghoulType == GhoulType.Revenant)
                s.AppendLine("ROMV_HI_GhoulRevenant".Translate());
            s.AppendLine("ROMV_HI_GhoulImmunities".Translate());
            s.AppendLine(base.TipStringExtra);
//                if (!this.comps.NullOrEmpty())
//                {
//                    foreach (HediffComp compProps in this.comps)
//                    {
//                        if (compProps is JecsTools.HediffComp_DamageSoak dmgSoak)
//                        {
//                            s.AppendLine(dmgSoak.CompTipStringExtra);
//                        }
//                    }   
//                }
            return s.ToString().TrimEndNewlines();
        }
    }


    public override void PostTick()
    {
        base.PostTick();
        var compVampire = pawn.VampComp();
        if (compVampire == null) return;

        if (!initialized)
        {
            initialized = true;

            if (compVampire.IsVampire && compVampire.ThrallData == null)
                compVampire.InitializeGhoul(domitor);
        }

        if (Find.TickManager.TicksGame % 60 == 0)
        {
            if (!compVampire.BeenGhoulBefore)
                compVampire.BeenGhoulBefore = true;

            if (compVampire.ThrallData == null)
                return;
            switch (compVampire.ThrallData.BondStage)
            {
                case BondStage.FirstTaste:
                case BondStage.SecondTaste:
                    Severity = 0.151f;
                    break;
                case BondStage.Thrall:
                    switch (ghoulPower)
                    {
                        case GhoulPower.Modern:
                            Severity = 0.251f;
                            break;
                        case GhoulPower.Old:
                            Severity = 0.51f;
                            break;
                        case GhoulPower.Ancient:
                            Severity = 0.751f;
                            break;
                        case GhoulPower.Primeval:
                            Severity = 1.0f;
                            break;
                    }

                    break;
            }

            compVampire.ThrallData.CheckBondStage();
        }
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        pawn.VampComp().Notify_DeGhouled();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref initialized, "initialized");
        Scribe_Values.Look(ref ghoulPower, "ghoulPower");
        Scribe_Values.Look(ref ghoulType, "ghoulType");
        Scribe_References.Look(ref domitor, "domitor");
    }
}