using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{

    public class HediffVampirism : HediffWithComps
    {
        private bool initialized = false;
        public bool firstVampire = false;
        public int generation = -1;
        public CompVampire sire = null;
        public BloodlineDef bloodline = null;
        //private Dictionary<Hediff, int> carriedBloodInfectors = null;
        //private Dictionary<Hediff, int> carriedBloodDrugEffects = null;
        //public bool IsInfectionCarrier => carriedBloodInfectors != null;
        //public bool IsDrugCarrier => carriedBloodDrugEffects != null;

        public override void PostTick()
        {
            base.PostTick();
            if (pawn.VampComp() is CompVampire v)
            {

                if (!initialized)
                {
                    initialized = true;

                    if (!firstVampire)
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
                        v.InitializeVampirism(sire?.AbilityUser ?? null, bloodline, generation, firstVampire);
                    }
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }

                if (Find.TickManager.TicksGame % 60 == 0)
                {
                    if (v.InSunlight)
                        HealthUtility.AdjustSeverity(pawn, VampDefOf.ROMV_SunExposure, 0.001f);
                    if (v.BloodPool?.CurLevelPercentage < 0.3f)
                        HealthUtility.AdjustSeverity(pawn, VampDefOf.ROMV_TheBeast, 0.001f);
                    if (pawn.health.hediffSet is HediffSet hdSet)
                    {
                        if (hdSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia) is Hediff hypoThermia)
                            hdSet.hediffs.Remove(hypoThermia);
                        else if (hdSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke) is Hediff heatStroke)
                            hdSet.hediffs.Remove(heatStroke);
                    }
                }

            }
        }

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

        private string GetGenerationSuffix(Pawn genPawn)
        {
            return " (" + AddOrdinal(genPawn.VampComp()?.Generation ?? -1) + ")";
        }

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
                    if (this?.pawn?.VampComp()?.Thinblooded ?? false)
                        s.AppendLine("ROMV_HI_Thinblooded".Translate());
                    s.AppendLine("ROMV_HI_Immunities".Translate());
                    if (!this.comps.NullOrEmpty())
                        foreach (HediffComp compProps in this.comps)
                            if (compProps is JecsTools.HediffComp_DamageSoak dmgSoak)
                                s.AppendLine(dmgSoak.CompTipStringExtra);
                }
                catch (NullReferenceException)
                {
                    //Log.Message(e.ToString());
                }
                return s.ToString();
            }
        }

        private void AppendBasicVampireInfo(StringBuilder s)
        {
            string bloodlineLabel =
                GetBloodlineLabel();
            string sireLabel =
                GetSireLabel();
            string sireGeneration = GetGenerationSuffix(pawn?.VampComp().Sire) ?? "Unknown";
            sireLabel += sireGeneration;
            s.AppendLine("ROMV_HI_Bloodline".Translate(bloodlineLabel));
            s.AppendLine("ROMV_HI_Sire".Translate(sireLabel));
        }

        private void AppendSoulNamesTo(StringBuilder s)
        {
            string[] soulNames = new string[pawn.VampComp().Souls.Count];
            for (int i = 0; i < soulNames.Length; i++)
                soulNames[i] = pawn.VampComp().Souls.ElementAt(i).LabelShort;
            s.AppendLine("ROMV_HI_Souls".Translate(string.Join(", ", soulNames)));
        }

        private void AppendChilderNames(StringBuilder s)
        {
            string[] childerNames = new string[pawn.VampComp().Childer.Count];
            for (int i = 0; i < childerNames.Length; i++)
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

        public static string AddOrdinal(int num)
        {
            if (num <= 0) return "";

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialized, "initialized");
            //Scribe_Collections.Look<Hediff, int>(ref this.carriedBloodInfectors, "carriedBloodInfectors", LookMode.Deep, LookMode.Value);
            //Scribe_Collections.Look<Hediff, int>(ref this.carriedBloodDrugEffects, "carriedBloodDrugEffects", LookMode.Deep, LookMode.Value);
        }
    }
}
