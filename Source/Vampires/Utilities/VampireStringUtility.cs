using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Vampire
{
    public static class VampireStringUtility
    {

        public static string GetVampireTooltip(BloodlineDef bloodline, int generation)
        {
            int gen = generation;
            int math = gen > 7 ? 10 + Math.Abs(gen - 13) : 10 * Math.Abs(gen - 9);

            StringBuilder s = new StringBuilder();
            s.AppendLine(bloodline.LabelCap);
            s.AppendLine(bloodline.description);
            s.AppendLine("---");
            s.AppendLine("ROMV_Disciplines".Translate());
            if (bloodline.disciplines is List<DisciplineDef> dDefs && !dDefs.NullOrEmpty())
            {
                foreach (DisciplineDef dDef in dDefs)
                {
                    s.AppendLine(" -" + dDef.LabelCap);
                }
            }
            s.AppendLine("---");
            s.AppendLine("ROMV_HI_VitaeAvailable".Translate(math));
            return s.ToString();
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

        public static string GetGenerationDescription(int generation)
        {
            var s = new StringBuilder();
            HediffDef hediffGeneration = null;
            switch (generation) 
            {
                case 0:
                    return "";
                case 1:
                    hediffGeneration = VampDefOf.ROM_Generations_Caine;
                    break;
                case 2:
                    hediffGeneration = VampDefOf.ROM_Generations_TheThree;
                    break;
                case 3:
                    hediffGeneration = VampDefOf.ROM_Generations_Antediluvian;
                    break;
                case 4:
                case 5:
                    hediffGeneration = VampDefOf.ROM_Generations_Methuselah;
                    break;
                case 6:
                case 7:
                case 8:
                    hediffGeneration = VampDefOf.ROM_Generations_Elder;
                    break;
                case 9:
                case 10:
                    hediffGeneration = VampDefOf.ROM_Generations_Ancillae;
                    break;
                case 11:
                case 12:
                case 13:
                    hediffGeneration = VampDefOf.ROM_Generations_Neonate;
                    break;
                case 14:
                default:
                    hediffGeneration = VampDefOf.ROM_Generations_Thinblood;
                    break;
            }

            try
            {
                if (generation != -1)
                    s.AppendLine(hediffGeneration.LabelCap + 
                        " (" + 
                        "ROMV_HI_Generation".Translate(VampireStringUtility.AddOrdinal(generation)) +
                        ")");
                else
                    s.AppendLine(hediffGeneration.LabelCap);
                string painFactor = hediffGeneration.stages[0].painFactor.ToStringPercent();
                string sensesFactor = hediffGeneration.stages[0].capMods.First().offset.ToStringPercent();
                s.AppendLine("ROMV_HI_Pain".Translate(painFactor));
                s.AppendLine("ROMV_HI_Senses".Translate(sensesFactor));
                s.AppendLine("ROMV_HI_Vigor".Translate(sensesFactor));
                s.AppendLine("ROMV_HI_Immunities".Translate());
                if (!hediffGeneration.comps.NullOrEmpty())
                    foreach (HediffCompProperties compProps in hediffGeneration.comps)
                    {
                        if (compProps is JecsTools.HediffCompProperties_DamageSoak dProps)
                        {
                            var ss = new StringBuilder();
                            if (dProps.settings.NullOrEmpty())
                            {
                                ss.AppendLine("JT_HI_DamageSoaked".Translate(
                                    (dProps.damageType != null) ? 
                                    dProps.damageToSoak.ToString() + " (" + dProps.damageType.LabelCap + ") " :
                                    dProps.damageToSoak.ToString() + " (" + "AllDays".Translate() + ")"));
                            }
                            else
                            {
                                foreach (var setting in dProps.settings)
                                {
                                    ss.AppendLine("JT_HI_DamageSoaked".Translate((setting.damageType != null) ?
                                        setting.damageToSoak.ToString() + " (" + setting.damageType.LabelCap + ") " :
                                        setting.damageToSoak.ToString() + " (" + "AllDays".Translate() + ")"));
                                }
                            }
                            s.AppendLine(ss.ToString().TrimEndNewlines());
                        }

                        if (compProps is JecsTools.HediffCompProperties_ExtraMeleeDamages mProps)
                        {
                            var sss = new StringBuilder();
                            var extraDamages = mProps?.ExtraDamages;
                            if (!extraDamages.NullOrEmpty())
                            {
                                sss.AppendLine("JT_HI_ExtraDamages".Translate());
                                for (var i = 0; i < extraDamages.Count; i++)
                                    sss.AppendLine("  +" + extraDamages[i].amount + " " + extraDamages[i].def.LabelCap);
                            }
                            s.AppendLine(sss.ToString().TrimEndNewlines());

                        }
                    }
            }
            catch (NullReferenceException)
            {
                //Log.Message(e.ToString());
            }
            return s.ToString();
        }
    }
}
