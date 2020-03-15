using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
                    //Make the sound and mote
                    GenClamor.DoClamor(actor, 10f, ClamorDefOf.Harm);
                    actor.Drawer.Notify_MeleeAttackOn(victim);

                    //Create the battle log entry
                    BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(dmgRules, true,
                        actor, victim, ImplementOwnerTypeDefOf.Hediff, dmgLabel);
                    battleLogEntry_MeleeCombat.def = LogEntryDefOf.MeleeAttack;

                    //Apply the melee damage
                    DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
                    damageResult = victim.TakeDamage(new DamageInfo(dmgDef, (int)(dmgAmount * BITEFACTOR), 0.5f, -1, actor, neckPart));
                    damageResult.AssociateWithLog(battleLogEntry_MeleeCombat);

                    //Add to the battle log
                    Find.BattleLog.Add(battleLogEntry_MeleeCombat);

                    //Transfer any alcohol
                    bool addHediff = false;
                    if (victim?.health?.hediffSet?.GetHediffs<Hediff_Alcohol>()?.FirstOrDefault(x => x.def == HediffDefOf.AlcoholHigh) is Hediff victimAlcohol)
                    {
                        Hediff actorAlcohol;
                        if (actor?.health?.hediffSet?.GetHediffs<Hediff_Alcohol>()?.FirstOrDefault(x => x.def == HediffDefOf.AlcoholHigh) is Hediff findActorAlcohol)
                        {
                            actorAlcohol = findActorAlcohol;
                        }
                        else
                        {
                            addHediff = true;
                            actorAlcohol = HediffMaker.MakeHediff(HediffDefOf.AlcoholHigh, actor, null);
                        }
                        actorAlcohol.Severity += victimAlcohol.Severity;

                        if (addHediff)
                            actor.health.AddHediff(actorAlcohol);

                        victim.health.RemoveHediff(victimAlcohol);
                    }
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
                rules = manueverDef.combatLogRulesHit;
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

                Hediff_Injury neckInjury = (Hediff_Injury)victim.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_Injury y && !y.IsPermanent() && y?.Part?.def == BodyPartDefOf.Neck);
                if (neckInjury == null)
                {
                    neckInjury = (Hediff_Injury)victim.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_Injury y && !y.IsPermanent());
                }
                if (neckInjury != null)
                {
                    neckInjury.Heal((int)neckInjury.Severity + 1);
                    Find.BattleLog.Add(
                        new BattleLogEntry_StateTransition(victim,
                        RulePackDef.Named("ROMV_BiteCleaned"), actor, null, null)
                    );
                    if (victim.IsGhoul() && victim?.VampComp()?.ThrallData?.BondStage == BondStage.Thrall)
                    {
                        TryRemoveHarmedMemory(actor, victim);
                    }
                    else if (!victim.health.capacities.CanBeAwake)
                    {
                        TryRemoveHarmedMemory(actor, victim);
                    }
                }

            }
        }

        public static bool TryRemoveHarmedMemory(Pawn actor, Pawn victim)
        {
            if (victim?.needs?.mood?.thoughts?.memories is MemoryThoughtHandler m)
            {
                m.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HarmedMe, actor);
                return true;
            }
            return false;
        }
    }
}
