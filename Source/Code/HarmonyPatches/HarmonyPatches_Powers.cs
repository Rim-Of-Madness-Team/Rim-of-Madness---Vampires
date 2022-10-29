using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Powers(Harmony harmony)
    {
        // VAMPIRIC POWERS
        ///////////////////////////////////////////////////////////////////////////////////
        // Add vampire XP every time a pawn learns a skill.
        harmony.Patch(AccessTools.Method(typeof(SkillRecord), "Learn"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Learn_PostFix)));
        //Log.Message("60");
        //Allow fortitude to soak damage
        harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), "PreApplyDamage"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(VampFortitude)));
        //Log.Message("61");
        //Adds blood shield
        harmony.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(GetGizmos_PostFix)));
        //Log.Message("62");
        harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawEquipment"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawEquipment_PostFix)));
        //Log.Message("63");
        //Remove vampire's ability to bleed.
        harmony.Patch(AccessTools.Method(typeof(Hediff_Injury), "get_BleedRate"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(get_VampBleedRate)));
        //Log.Message("64");
        //Vampires are not affected by Hypothermia nor Heatstroke
        harmony.Patch(AccessTools.Method(typeof(HediffGiver_Heat), "OnIntervalPassed"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreStrokeAndHypotherm)));

        harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "TryCastShot"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TryCastShot)));

        //Utilizes the new RimWorld Royalty invisibility code for Vampire abilities.
        harmony.Patch(AccessTools.Method(typeof(PawnUtility), "IsInvisible"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IsInvisible)));
        //Log.Message("34");

        //TODO Fix it

        ////Log.Message("62");
        //            harmony.Patch(AccessTools.Method(typeof(HediffGiver_Hypothermia), "OnIntervalPassed"), null,
        //                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_IgnoreStrokeAndHypotherm)));


        //            ////Log.Message("63");
        //            harmony.Patch(
        //                AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff",
        //                    new Type[] {typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?)}),
        //                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHediff)), null);
        ////Log.Message("64");
    }

    // RimWorld.PawnUtility
    public static void Vamp_IsInvisible(Pawn pawn, ref bool __result)
    {
        if (__result == false)
        {
            var hediffs = pawn.health.hediffSet.hediffs;
            for (var i = 0; i < hediffs.Count; i++)
                if (hediffs[i].TryGetComp<HediffComp_Hidden>() != null)
                {
                    __result = true;
                    return;
                }
        }
    }

    public static void Learn_PostFix(SkillRecord __instance, float xp, bool direct = false)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(SkillRecord), "pawn").GetValue(__instance);
        if (xp > 0 && pawn.TryGetComp<CompVampire>() is { } compVamp &&
            (compVamp.IsVampire || compVamp.IsGhoul) && Find.TickManager.TicksGame > compVamp.ticksToLearnXP)
        {
            var delay = 132;
            if (__instance.def == SkillDefOf.Intellectual || __instance.def == SkillDefOf.Plants) delay += 52;
            compVamp.ticksToLearnXP = Find.TickManager.TicksGame + delay;
            //////Log.Message("XP");
            compVamp.XP++;
        }
    }


    // Verse.Pawn_HealthTracker
    public static bool VampFortitude(Pawn_HealthTracker __instance, DamageInfo dinfo, out bool absorbed)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_HealthTracker), "pawn").GetValue(__instance);
        if (pawn != null)
        {
            if (pawn is PawnTemporary t)
            {
                t.Drawer.Notify_DebugAffected();
                absorbed = true;
                return false;
            }

            if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
            {
                if (dinfo.Instigator is Pawn instigator && instigator.health.hediffSet.hediffs != null &&
                    instigator.health.hediffSet.hediffs.Count > 0)
                    if (instigator.health.hediffSet.hediffs.FirstOrDefault(x =>
                            x.TryGetComp<HediffComp_Hidden>() != null) is HediffWithComps hide &&
                        hide.TryGetComp<HediffComp_Hidden>() is { } hideComp)
                        ////Log.Message("Original damage: " + dinfo.Amount);
                        if (hideComp.Props.damageFactor > 0f)
                            dinfo.SetAmount((int)(dinfo.Amount * hideComp.Props.damageFactor));
                    ////Log.Message("Damage applied: " + dinfo.Amount);

                if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_ReadMind>() != null)
                        is HediffWithComps h && h.TryGetComp<HediffComp_ReadMind>() is { } rm)
                    if (rm.MindBeingRead == dinfo.Instigator)
                    {
                        pawn.Drawer.Notify_DebugAffected();
                        absorbed = true;
                        return false;
                    }

                if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_AnimalForm>() != null)
                        is HediffWithComps ht && ht.TryGetComp<HediffComp_AnimalForm>().Props.immuneTodamage)
                {
                    pawn.Drawer.Notify_DebugAffected();
                    absorbed = true;
                    return false;
                }

                if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_Hidden>() != null) is
                        HediffWithComps httt && httt.TryGetComp<HediffComp_Hidden>().Props.immuneTodamage)
                {
                    pawn.Drawer.Notify_DebugAffected();
                    absorbed = true;
                    return false;
                }

                if (pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.TryGetComp<HediffComp_Shield>() != null) is
                        HediffWithComps htt && htt.TryGetComp<HediffComp_Shield>() is { } shield)
                    if (shield.CheckPreAbsorbDamage(dinfo))
                    {
                        absorbed = true;
                        return false;
                    }
            }
        }

        absorbed = false;
        return true;
    }


    public static IEnumerable<Gizmo> gizmoGetter(HediffComp_Shield compHediffShield)
    {
        if (compHediffShield.GetWornGizmos() != null)
        {
            var enumerator = compHediffShield.GetWornGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }
        }
    }

    public static void GetGizmos_PostFix(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        var pawn = __instance;
        if (pawn.health != null)
            if (pawn.health.hediffSet != null)
                if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
                {
                    var shieldHediff =
                        pawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                            x.TryGetComp<HediffComp_Shield>() != null);
                    if (shieldHediff != null)
                    {
                        var shield = shieldHediff.TryGetComp<HediffComp_Shield>();
                        if (shield != null) __result = __result.Concat(gizmoGetter(shield));
                    }
                }
    }


    // Verse.PawnRenderer
    public static void DrawEquipment_PostFix(PawnRenderer __instance, Vector3 rootLoc)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(PawnRenderer), "pawn").GetValue(__instance);
        if (pawn.health != null)
            if (pawn.health.hediffSet != null)
                if (pawn.health.hediffSet.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
                {
                    var shieldHediff =
                        pawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                            x.TryGetComp<HediffComp_Shield>() != null);
                    if (shieldHediff != null)
                    {
                        var shield = shieldHediff.TryGetComp<HediffComp_Shield>();
                        if (shield != null) shield.DrawWornExtras();
                    }
                }
    }


    // Verse.Hediff_Injury
    public static void get_VampBleedRate(Hediff_Injury __instance, ref float __result)
    {
        if (__instance.pawn is { } p && p.IsVampire(true)) __result = 0f;
    }

    // Verse.HediffGiver_Heat
    public static void Vamp_IgnoreStrokeAndHypotherm(Pawn pawn, Hediff cause)
    {
        if (cause?.def == HediffDefOf.Hypothermia ||
            cause?.def == HediffDefOf.Heatstroke)
            if (pawn?.health?.hediffSet?.GetFirstHediffOfDef(cause.def) is { } h)
                pawn.health.RemoveHediff(h);
    }


    // Verse.Verb_Shoot
    public static bool Vamp_TryCastShot(Verb_Shoot __instance, ref bool __result)
    {
        if (__instance?.CasterPawn is { } p && p.IsVampire(true))
        {
            if (__instance.CasterPawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                    x.TryGetComp<HediffComp_AnimalForm>() != null) is HediffWithComps ht &&
                ht.TryGetComp<HediffComp_AnimalForm>() is { } af && !af.Props.canGiveDamage)
            {
                //__instance.CasterPawn.health.hediffSet.hediffs.Remove(ht);
                __instance.CasterPawn.health.RemoveHediff(ht);
                __instance.CasterPawn.VampComp().CurrentForm = null;
                __instance.CasterPawn.VampComp().CurFormGraphic = null;
                __instance.CasterPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }

            if (__instance.CasterPawn.health.hediffSet.hediffs.FirstOrDefault(x =>
                    x.TryGetComp<HediffComp_Hidden>() != null) is HediffWithComps htt &&
                htt.TryGetComp<HediffComp_Hidden>() is { } hf && !hf.Props.canGiveDamage)
            {
                //__instance.CasterPawn.health.hediffSet.hediffs.Remove(htt);
                __instance.CasterPawn.health.RemoveHediff(htt);
                __instance.CasterPawn.VampComp().CurrentForm = null;
                __instance.CasterPawn.VampComp().CurFormGraphic = null;
                __instance.CasterPawn.Drawer.renderer.graphics.nakedGraphic = null;
                __instance.CasterPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        return true;
    }
}