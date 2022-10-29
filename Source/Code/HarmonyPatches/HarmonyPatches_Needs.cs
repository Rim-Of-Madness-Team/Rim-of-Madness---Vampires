using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Needs(Harmony harmony)
    {
        // NEEDS
        //////////////////////////////////////////////////////////////////////////////
        //Fixes issues with having no food need.
        harmony.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(ShouldHaveNeed_Vamp)));
        //Log.Message("01");
        harmony.Patch(AccessTools.Method(typeof(ThinkNode_ConditionalNeedPercentageAbove), "Satisfied"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Satisfied_Vamp)));
        //Log.Message("02");
        //Vampires vomit blood instead of their digested meals.
        harmony.Patch(AccessTools.Method(typeof(JobDriver_Vomit), "MakeNewToils"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeNewToils_VampVomit)));
        //Log.Message("03");

        //Log.Message("05");
        harmony.Patch(AccessTools.Method(typeof(JobGiver_GetJoy), "TryGiveJob"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(INeverDrink___Juice)));
        //Log.Message("06");

        //RimWorld 1.1 Patches
    }


    // RimWorld.Pawn_NeedsTracker
    public static void ShouldHaveNeed_Vamp(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
    {
        var p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (p.VampComp() != null && p.VampComp().IsVampire)
            if (nd == NeedDefOf.Food)
            {
                __result = false;
                return;
            }

        if (nd == VampDefOf.ROMV_Blood)
        {
            if (p?.RaceProps?.IsMechanoid ?? false)
            {
                __result = false;
                return;
            }

            var typeString = p.GetType().ToString();
            //////Log.Message(typeString);
            ///
            if (DefDatabase<BloodNeedExceptions>.GetNamedSilentFail("ExceptionsList") is { } exceptionList)
            {
                foreach (var s in exceptionList.thingClasses)
                    if (p?.GetType()?.ToString() == s)
                    {
                        __result = false;
                        return;
                    }

                foreach (var d in exceptionList.raceThingDefs)
                    if (p?.def.defName == d)
                    {
                        __result = false;
                        return;
                    }
            }
        }
    }

    // RimWorld.ThinkNode_ConditionalNeedPercentageAbove
    public static bool Satisfied_Vamp(ThinkNode_ConditionalNeedPercentageAbove __instance, Pawn pawn,
        ref bool __result)
    {
        if (pawn.VampComp() is { } v && v.IsVampire &&
            Traverse.Create(__instance).Field("need").GetValue<NeedDef>() == NeedDefOf.Food)
        {
            __result = true;
            return false;
        }

        return true;
    }


    //public class JobDriver_Vomit : JobDriver
    public static bool MakeNewToils_VampVomit(JobDriver_Vomit __instance, ref IEnumerable<Toil> __result)
    {
        if (__instance.pawn.IsVampire(true))
        {
            var to = new Toil
            {
                initAction = delegate
                {
                    AccessTools.Field(typeof(JobDriver_Vomit), "ticksLeft")
                        .SetValue(__instance, Rand.Range(300, 900));
                    var num = 0;
                    IntVec3 c;
                    while (true)
                    {
                        c = __instance.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
                        num++;
                        if (num > 12) break;

                        if (c.InBounds(__instance.pawn.Map) && c.Standable(__instance.pawn.Map)) goto IL_A1;
                    }

                    c = __instance.pawn.Position;
                    IL_A1:
                    __instance.pawn.CurJob.targetA = c;
                    __instance.pawn.rotationTracker.FaceCell(c);
                    __instance.pawn.pather.StopDead();
                },
                tickAction = delegate
                {
                    var curTicks = Traverse.Create(__instance).Field("ticksLeft").GetValue<int>();
                    if (curTicks % 150 == 149)
                    {
                        var sourcePawn = __instance.pawn?.VampComp().MostRecentVictim != null
                            ? __instance.pawn.VampComp().MostRecentVictim
                            : __instance.pawn;
                        var vomitFilthDef = BloodUtility.GetBloodFilthDef(sourcePawn);
                        FilthMaker.TryMakeFilth(__instance.pawn.CurJob.targetA.Cell, __instance.pawn.Map,
                            vomitFilthDef, __instance.pawn.LabelIndefinite());
                        if (__instance.pawn.BloodNeed() is { } n && n.CurBloodPoints > 0) n.AdjustBlood(-1);

                        //if (__instance.pawn.needs.food.CurLevelPercentage > 0.1f)
                        //{
                        //    __instance.pawn.needs.food.CurLevel -= __instance.pawn.needs.food.MaxLevel * 0.04f;
                        //}
                    }

                    AccessTools.Field(typeof(JobDriver_Vomit), "ticksLeft").SetValue(__instance, curTicks - 1);

                    if (curTicks - 1 <= 0)
                    {
                        __instance.ReadyForNextToil();
                        TaleRecorder.RecordTale(TaleDefOf.Vomited, __instance.pawn);
                    }
                }
            };
            to.defaultCompleteMode = ToilCompleteMode.Never;
            if (__instance.pawn?.VampComp()?.MostRecentVictim != null &&
                __instance.pawn?.VampComp()?.MostRecentVictim?.IsCoolantUser() == true)
                to.WithEffect(DefDatabase<EffecterDef>.GetNamed("ROMV_CoolantVomit"), TargetIndex.A);
            else
                to.WithEffect(DefDatabase<EffecterDef>.GetNamed("ROMV_BloodVomit"), TargetIndex.A);
            to.PlaySustainerOrSound(() => SoundDef.Named("Vomit"));
            __result = __result.AddItem(to);

            return false;
        }

        return true;
    }


    // RimWorld.JobGiver_GetJoy
    //protected override Job TryGiveJob(Pawn pawn)
    public static bool INeverDrink___Juice(Pawn pawn, ref Job __result)
    {
        if (pawn.IsVampire(true) && __result != null && __result.def == JobDefOf.Ingest)
        {
            __result = null;
            return false;
        }

        return true;
    }
}