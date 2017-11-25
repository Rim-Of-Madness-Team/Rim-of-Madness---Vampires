using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public static class VampireBiteUtility
    {
        public static readonly float BITEFACTOR = 0.2f;

        public static void MakeNew(Pawn actor, Pawn victim)
        {
            string dmgLabel;
            float dmgAmount;
            DamageDef dmgDef;
            RulePackDef dmgRules;

            if (TryGetFangsDmgInfo(actor.GetVampFangs(), out dmgLabel, out dmgAmount, out dmgDef, out dmgRules))
            {
                BodyPartRecord neckPart = victim.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == BodyPartDefOf.Neck);
                if (neckPart == null) neckPart = victim.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.depth == BodyPartDepth.Outside);
                if (neckPart == null) neckPart = victim.health.hediffSet.GetNotMissingParts().RandomElement();
                if (neckPart != null)
                {
                    GenClamor.DoClamor(actor, 10f, ClamorType.Harm);
                    actor.Drawer.Notify_MeleeAttackOn(victim);
                    victim.TakeDamage(new DamageInfo(dmgDef, (int)(dmgAmount * BITEFACTOR), -1, actor, neckPart));
                    BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(RulePackDefOf.Combat_Hit, dmgRules,
                        actor, victim, ImplementOwnerTypeDefOf.Bodypart, dmgLabel, null, null);
                    Find.BattleLog.Add(battleLogEntry_MeleeCombat);
                }
            }
             
        }

        public static bool TryGetFangsDmgInfo(HediffWithComps fangs, out string label, out float dmg, out DamageDef dDef, out RulePackDef rules)
        {
            label = "";
            dmg = 0f;
            dDef = null;
            rules = null;
            if (fangs != null && fangs.def.CompProps<HediffCompProperties_VerbGiver>()?
                .tools is List<Tool> tools && !tools.NullOrEmpty())
            {
                Tool firstTool = tools.First();
                ToolCapacityDef capacityDef = firstTool.capacities.First();
                ManeuverDef manueverDef = DefDatabase<ManeuverDef>.AllDefs.FirstOrDefault(x => x.requiredCapacity == capacityDef);
                label = firstTool.label;
                dmg = firstTool.power;
                dDef = manueverDef.verb.meleeDamageDef;
                rules = manueverDef.combatLogRules;
                return true;
            }
            return false;
        }

        public static HediffWithComps GetVampFangs(this Pawn actor)
        {
            return actor?.health?.hediffSet?.GetFirstHediffOfDef(actor.VampComp().Bloodline.fangsHediff) as HediffWithComps;
        }

        public static BodyPartGroupDef GetFangGroupDef(Pawn actor)
        {
            if (GetVampFangs(actor) is Hediff_AddedPart addedPart)
            {
                return addedPart?.Part?.groups?.First() ?? null;
            }
            return null;
        }

        public static void CleanBite(Pawn actor, Pawn victim)
        {
            HediffWithComps fangs = actor.GetVampFangs();

            string dmgLabel;
            float dmgAmount;
            DamageDef dmgDef;
            RulePackDef dmgRules;

            if (fangs != null && TryGetFangsDmgInfo(fangs, out dmgLabel, out dmgAmount, out dmgDef, out dmgRules))
            {
                if (victim.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_Injury y && !y.IsOld()) is Hediff_Injury inj)
                {
                    inj.Heal((int)inj.Severity + 1);
                }
                Find.BattleLog.Add(
                    new BattleLogEntry_StateTransition(victim,
                    RulePackDef.Named("ROMV_BiteCleaned"), actor, null, null)
                );
            }
        }
    }
}
