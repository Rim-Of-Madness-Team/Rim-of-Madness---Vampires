using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire;

public class HediffVampirism : HediffWithComps
{
    public BloodlineDef bloodline;
    public bool firstVampire = false;
    public int generation = -1;
    private bool initialized;
    public CompVampire sire;

    public override string TipStringExtra
    {
        get
        {
            var s = new StringBuilder();
            try
            {
                AppendBasicVampireInfo(s);
                if (pawn?.VampComp()?.Childer?.NullOrEmpty() ?? true)
                    s.AppendLine("ROMV_HI_Childer".Translate("ROMV_HI_None".Translate()));
                else
                    AppendChilderNames(s);
                if (!pawn?.VampComp()?.Souls?.NullOrEmpty() ?? false)
                    AppendSoulNamesTo(s);
/*                    if (this?.pawn?.VampComp()?.Thinblooded ?? false)
                        s.AppendLine("ROMV_HI_Thinblooded".Translate());
                    s.AppendLine("ROMV_HI_Immunities".Translate());
                    if (!this.comps.NullOrEmpty())
                        foreach (HediffComp compProps in this.comps)
                            if (compProps is JecsTools.HediffComp_DamageSoak dmgSoak)
                                s.AppendLine(dmgSoak.CompTipStringExtra);*/
            }
            catch (NullReferenceException)
            {
                //Log.Message(e.ToString());
            }

            return s.ToString();
        }
    }

    public override void PostTick()
    {
        base.PostTick();
        if (!(pawn?.VampComp() is { } v)) return;
        if (!initialized)
        {
            initialized = true;

            if (!firstVampire && pawn != VampireTracker.Get.FirstVampire)
            {
                if (sire == null)
                    sire = VampireRelationUtility.FindSireFor(pawn, bloodline, generation).VampComp();
                if (generation < 0)
                    generation = sire.Generation + 1;
                if (bloodline == null)
                    bloodline = sire.Bloodline;
            }

            if (v.IsVampire && v.Sire == null)
            {
                if (pawn == VampireTracker.Get.FirstVampire || firstVampire)
                    v.InitializeVampirism(null, VampDefOf.ROMV_Caine, 1, true);
                else
                    v.InitializeVampirism(sire?.AbilityUser ?? null, bloodline, generation, firstVampire);
            }

            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        if (Find.TickManager.TicksGame % 60 != 0) return;

        if (VampireSettings.Get.damageToggle && v.InSunlight)
            HealthUtility.AdjustSeverity(pawn, VampDefOf.ROMV_SunExposure, 0.001f);


        if (v.BloodPool?.CurLevelPercentage < 0.3f)
            HealthUtility.AdjustSeverity(pawn, VampDefOf.ROMV_TheBeast, 0.001f);

        //Remove nonsensical hediffs
        if (pawn.health.hediffSet is { } hdSet)
        {
            if (hdSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia) is { } hypoThermia)
                hdSet.hediffs.Remove(hypoThermia);
            else if (hdSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke) is { } heatStroke)
                hdSet.hediffs.Remove(heatStroke);
            else if (hdSet.GetFirstHediffOfDef(VampDefOfTwo.GutWorms) is { } gutWorms)
                hdSet.hediffs.Remove(gutWorms);
            else if (hdSet.GetFirstHediffOfDef(VampDefOfTwo.MuscleParasites) is { } muscleParasites)
                hdSet.hediffs.Remove(muscleParasites);
            else if (hdSet.GetFirstHediffOfDef(VampDefOfTwo.FibrousMechanites) is { } fibrousMechanites)
                hdSet.hediffs.Remove(fibrousMechanites);
            else if (hdSet.GetFirstHediffOfDef(VampDefOfTwo.SensoryMechanites) is { } sensoryMechanites)
                hdSet.hediffs.Remove(sensoryMechanites);
        }

        //If no generational bonus exists...
        if (!pawn.health.hediffSet.HasHediff(pawn.GenerationDef()))
            HealthUtility.AdjustSeverity(pawn, pawn.GenerationDef(), 1.0f);
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        if (pawn.health.hediffSet.HasHediff(pawn.GenerationDef()))
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(pawn.GenerationDef());
            if (hediff != null) pawn.health.RemoveHediff(hediff);
        }
    }


/*
        public override string LabelBase
        {
            get
            {
                if (pawn.VampComp().Generation != -1)
                {
                    return "ROMV_HI_VampGeneration".Translate(AddOrdinal(pawn.VampComp().Generation));
                }
                if (generation != -1)
                {
                    return "ROMV_HI_VampGeneration".Translate(AddOrdinal(generation));
                }
                return "Vampire";
            }
        }
*/

    private string GetGenerationSuffix(Pawn genPawn)
    {
        return " (" + VampireStringUtility.AddOrdinal(genPawn.VampComp()?.Generation ?? -1) + ")";
    }

    private void AppendBasicVampireInfo(StringBuilder s)
    {
        var bloodlineLabel =
            GetBloodlineLabel();
        var sireLabel =
            GetSireLabel();
        var sireGeneration = GetGenerationSuffix(pawn?.VampComp().Sire) ?? "Unknown";
        sireLabel += sireGeneration;
        s.AppendLine("ROMV_HI_Bloodline".Translate(bloodlineLabel));
        s.AppendLine("ROMV_HI_Sire".Translate(sireLabel));
    }

    private void AppendSoulNamesTo(StringBuilder s)
    {
        var soulNames = new string[pawn.VampComp().Souls.Count];
        for (var i = 0; i < soulNames.Length; i++)
            soulNames[i] = pawn.VampComp().Souls.ElementAt(i).LabelShort;
        s.AppendLine("ROMV_HI_Souls".Translate(string.Join(", ", soulNames)));
    }

    private void AppendChilderNames(StringBuilder s)
    {
        var childerNames = new string[pawn.VampComp().Childer.Count];
        for (var i = 0; i < childerNames.Length; i++)
            childerNames[i] = pawn.VampComp().Childer.ElementAt(i).LabelShort;
        s.AppendLine("ROMV_HI_Childer".Translate(string.Join(", ", childerNames)));
    }

    private string GetSireLabel()
    {
        return this?.pawn?.VampComp()?.Sire?.LabelCap ?? "Unknown";
    }

    private string GetBloodlineLabel()
    {
        return this?.pawn?.VampComp()?.Bloodline?.LabelCap ?? this?.bloodline?.label ?? "Unknown";
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref initialized, "initialized");
        //Scribe_Collections.Look<Hediff, int>(ref this.carriedBloodInfectors, "carriedBloodInfectors", LookMode.Deep, LookMode.Value);
        //Scribe_Collections.Look<Hediff, int>(ref this.carriedBloodDrugEffects, "carriedBloodDrugEffects", LookMode.Deep, LookMode.Value);
    }
}