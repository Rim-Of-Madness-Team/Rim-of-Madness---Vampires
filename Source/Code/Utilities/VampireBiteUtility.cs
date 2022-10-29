using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

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
            var neckPart = victim.health.hediffSet.GetNotMissingParts()
                .FirstOrDefault(x => x.def == BodyPartDefOf.Neck);
            if (neckPart == null)
                neckPart = victim.health.hediffSet.GetNotMissingParts()
                    .FirstOrDefault(x => x.depth == BodyPartDepth.Outside);
            if (neckPart == null) neckPart = victim.health.hediffSet.GetNotMissingParts().RandomElement();
            if (neckPart != null)
            {
                //Make the sound and mote
                GenClamor.DoClamor(actor, 10f, ClamorDefOf.Harm);
                actor.Drawer.Notify_MeleeAttackOn(victim);

                //Create the battle log entry
                var battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(dmgRules, true,
                    actor, victim, ImplementOwnerTypeDefOf.Hediff, dmgLabel);
                battleLogEntry_MeleeCombat.def = LogEntryDefOf.MeleeAttack;

                //Apply the melee damage
                var damageResult = new DamageWorker.DamageResult();
                damageResult =
                    victim.TakeDamage(new DamageInfo(dmgDef, (int)(dmgAmount * BITEFACTOR), 0.5f, -1, actor, neckPart));
                damageResult.AssociateWithLog(battleLogEntry_MeleeCombat);

                //Add to the battle log
                Find.BattleLog.Add(battleLogEntry_MeleeCombat);

                //Transfer any alcohol
                var addHediff = false;
                if (victim?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh) is { } victimAlcohol)
                {
                    Hediff actorAlcohol;
                    if (actor?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh) is { } findActorAlcohol)
                    {
                        actorAlcohol = findActorAlcohol;
                    }
                    else
                    {
                        addHediff = true;
                        actorAlcohol = HediffMaker.MakeHediff(HediffDefOf.AlcoholHigh, actor);
                    }

                    actorAlcohol.Severity += victimAlcohol.Severity;

                    if (addHediff)
                        actor.health.AddHediff(actorAlcohol);

                    victim.health.RemoveHediff(victimAlcohol);
                }
            }
        }
    }

    public static bool TryGetFangsDmgInfo(HediffWithComps fangs, out string label, out float dmg, out DamageDef dDef,
        out RulePackDef rules)
    {
        label = "";
        dmg = 0f;
        dDef = null;
        rules = null;
        if (fangs != null && fangs.def.CompProps<HediffCompProperties_VerbGiver>()?
                .tools is { } tools && !tools.NullOrEmpty())
        {
            var firstTool = tools.First();
            var capacityDef = firstTool.capacities.First();
            var manueverDef = DefDatabase<ManeuverDef>.AllDefs.FirstOrDefault(x => x.requiredCapacity == capacityDef);
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
        if (GetVampFangs(actor) is Hediff_AddedPart addedPart) return addedPart?.Part?.groups?.First() ?? null;
        return null;
    }

    public static void CleanBite(Pawn actor, Pawn victim)
    {
        var fangs = actor.GetVampFangs();

        string dmgLabel;
        float dmgAmount;
        DamageDef dmgDef;
        RulePackDef dmgRules;

        if (fangs != null && TryGetFangsDmgInfo(fangs, out dmgLabel, out dmgAmount, out dmgDef, out dmgRules))
        {
            foreach (var neckInjury in victim.health.hediffSet.hediffs.Where(x =>
                         x is Hediff_Injury y && !y.IsPermanent() && y?.Part?.def == BodyPartDefOf.Neck))
            {
                neckInjury.Heal((int)neckInjury.Severity + 1);
                Find.BattleLog.Add(
                    new BattleLogEntry_StateTransition(victim,
                        RulePackDef.Named("ROMV_BiteCleaned"), actor, null, null)
                );
                if (victim.IsGhoul() && victim?.VampComp()?.ThrallData?.BondStage == BondStage.Thrall)
                    TryRemoveHarmedMemory(actor, victim);
                else if (!victim.health.capacities.CanBeAwake) TryRemoveHarmedMemory(actor, victim);
            }
        }
    }

    public static bool TryRemoveHarmedMemory(Pawn actor, Pawn victim)
    {
        if (victim?.needs?.mood?.thoughts?.memories is { } m)
        {
            m.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HarmedMe, actor);
            return true;
        }

        return false;
    }
}