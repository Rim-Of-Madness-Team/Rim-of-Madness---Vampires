using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Ingestion(Harmony harmony)
    {
        //Log.Message("Ingestion: 1");
        //Float Menus: Adds vampire blood consumption/consume buttons and hide regular consumption/consume
        harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FloatMenus_Consume)));

        //Log.Message("Ingestion: 2");
        //Vampires should not try to do drugs when idle.
        harmony.Patch(AccessTools.Method(typeof(JobGiver_IdleJoy), "TryGiveJob"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamps_DontDoIdleDrugs)));

        //Log.Message("Ingestion: 3");
        //Vampires should not be given food by wardens.
        harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "get_CanBeBroughtFood"), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamps_DontWantGuestFood)));

        //Log.Message("Ingestion: 4");
        harmony.Patch(AccessTools.Method(typeof(FoodUtility), "InappropriateForTitle"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(WhoNeeds___Titles)));

        //Log.Message("Ingestion: 5");
        harmony.Patch(
            AccessTools.Method(typeof(FoodUtility), "WillEat",
                new[] { typeof(Pawn), typeof(Thing), typeof(Pawn), 
                    //typeof(bool), RW1.4 unstable
                    typeof(bool) }),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(WillEat_Nothing)));

        //Log.Message("Ingestion: 6");
        harmony.Patch(
            AccessTools.Method(typeof(FoodUtility), "WillEat",
                new[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), 
                    //typeof(bool), RW1.4 unstable 
                    typeof(bool) }),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(WillEat_NothingDef)));

        //Log.Message("Ingestion: 7");
        harmony.Patch(AccessTools.Method(typeof(Toils_Ingest), "FinalizeIngest"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresDontIngestFood)));

        //Log.Message("Ingestion: 8");
        //Fixes random red errors relating to food need checks in this method (WillIngestStackCountOf).
        harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillIngestStackCountOf"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_WillIngestStackCountOf)));
        
        //Log.Message("Ingestion: 9");
        //Prevents restful times.
        harmony.Patch(AccessTools.Method(typeof(JoyGiver_SocialRelax), "TryFindIngestibleToNurse"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(INeverDrink___Wine)));
    }


    // RimWorld.JoyGiver_SocialRelax
    public static bool INeverDrink___Wine(IntVec3 center, Pawn ingester, out Thing ingestible, ref bool __result)
    {
        ingestible = null;
        if (ingester.IsVampire(true))
        {
            __result = false;
            return false;
        }

        return true;
    }


    // RimWorld.FoodUtility
    public static bool Vamp_WillIngestStackCountOf(Pawn ingester, ThingDef def, ref int __result)
    {
        if (ingester.IsVampire(true))
        {
            __result = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, 1);
            return false;
        }

        return true;
    }


    // RimWorld.Toils_Ingest    
    public static bool VampiresDontIngestFood(Pawn ingester, TargetIndex ingestibleInd, ref Toil __result)
    {
        if (!ingester.IsVampire(true))
            return true;
        if (ingester.jobs.curJob.GetTarget(ingestibleInd).Thing.def.graphicData.texPath ==
            "Things/Item/Resource/BloodWine")
            return true;

        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var thing = curJob.GetTarget(ingestibleInd).Thing;
            if (thing.def.IsNutritionGivingIngestible)
                if (Rand.Range(0f, 1f) > 0.5f)
                    ingester.jobs.StartJob(JobMaker.MakeJob(VampDefOf.ROMV_BloodVomit), JobCondition.InterruptForced,
                        null, true);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        __result = toil;
        return false;
    }


    public static bool WhoNeeds___Titles(ThingDef food, Pawn p, bool allowIfStarving, ref bool __result)
    {
        if (p.IsVampire(true))
        {
            __result = false;
            return false;
        }

        return true;
    }


    // RimWorld.FoodUtility.WillEat (Thing)
    public static bool WillEat_Nothing(Pawn p, Thing food, Pawn getter, bool careIfNotAcceptableForTitle, 
        //bool allowVenerated, RW 1.4 unstable
        ref bool __result)
    {
        if (p.IsVampire(true))
        {
            if (food?.TryGetComp<CompBloodItem>() is { } c)
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }

        return true;
    }


    // RimWorld.FoodUtility.WillEat (ThingDef)
    public static bool WillEat_NothingDef(Pawn p, ThingDef food, Pawn getter, bool careIfNotAcceptableForTitle, 
        //bool allowVenerated, RW1.4 unstable
        ref bool __result)
    {
        if (p.IsVampire(true))
        {
            if (food?.GetCompProperties<CompProperties_BloodItem>() is { } c)
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }

        return true;
    }


    // RimWorld.FloatMenuMakerMap
    private static void Vamp_FloatMenus_Consume(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
    {
        var c = IntVec3.FromVector3(clickPos);
        var selVampComp = pawn.VampComp();
        var pawnIsVampire = pawn.IsVampire(true);
        if (selVampComp != null && pawnIsVampire)
        {
            //Hide food consumption from menus.
            var food = c.GetThingList(pawn.Map)
                .FirstOrDefault(t => t.GetType() != typeof(Pawn) && t?.def?.ingestible != null);
            if (food != null)
            {
                //string text = (!food.def.ingestible.ingestCommandString.NullOrEmpty()) ? string.Format(
                //food.def.ingestible.ingestCommandString, food.LabelShort) : ((string)"ConsumeThing".
                //Translate(food.LabelShort, food));
                
                var text = "";

                if (food?.def?.ingestible?.ingestCommandString == null ||
                    food.def.ingestible.ingestCommandString == "")
                    text = "ConsumeThing".Translate(food.LabelShort, food);
                else
                    text = string.Format(food.def.ingestible.ingestCommandString, food.LabelShort);

                var o = opts.FirstOrDefault(x => x.Label.Contains(text));
                if (o != null) opts.Remove(o);
            }

            //Hide corpse consumption from menus.
            var corpse = c.GetThingList(pawn.Map).FirstOrDefault(t => t is Corpse);
            if (corpse != null)
            {
                string text;
                if (corpse.def.ingestible.ingestCommandString.NullOrEmpty())
                    text = "ConsumeThing".Translate(corpse.LabelShort, corpse);
                else
                    text = string.Format(corpse.def.ingestible.ingestCommandString, corpse.LabelShort);

                var o = opts.FirstOrDefault(x => x.Label.Contains(text));
                if (o != null) opts.Remove(o);
            }

            //Add blood consumption
            var bloodItem = c.GetThingList(pawn.Map)
                .FirstOrDefault(t => t.def.GetCompProperties<CompProperties_BloodItem>() != null);
            if (bloodItem != null)
            {
                var text = "";
                if (bloodItem.def.ingestible.ingestCommandString.NullOrEmpty())
                    text = "ConsumeThing".Translate(bloodItem.LabelShort, bloodItem);
                else
                    text = string.Format(bloodItem.def.ingestible.ingestCommandString, bloodItem.LabelShort);

                if (!bloodItem.IsSociallyProper(pawn)) text = text + " (" + "ReservedForPrisoners".Translate() + ")";

                FloatMenuOption item5;
                if (bloodItem.def.IsPleasureDrug && pawn.IsTeetotaler())
                {
                    item5 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")",
                        null);
                }
                else if (!pawn.CanReach(bloodItem, PathEndMode.OnCell, Danger.Deadly))
                {
                    item5 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null);
                }
                else
                {
                    var priority =
                        !(bloodItem is Corpse) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                    item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
                    {
                        bloodItem.SetForbidden(false);
                        var job = new Job(VampDefOf.ROMV_ConsumeBlood, bloodItem);
                        job.count = BloodUtility.WillConsumeStackCountOf(pawn, bloodItem.def);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }, priority), pawn, bloodItem);
                }

                opts.Add(item5);
            }
        }
    }


    // JobGiver_IdleJoy
    public static void Vamps_DontDoIdleDrugs(JobGiver_IdleJoy __instance, Pawn pawn, ref Job __result)
    {
        if (pawn.IsVampire(true) && __result is { } j && j.def == JobDefOf.Ingest &&
            j.targetA.Thing is ThingWithComps t && t.def.IsDrug &&
            t.def.graphicData.texPath != "Things/Item/Resource/BloodWine")
            __result = null;
    }

    // RimWorld.Pawn_GuestTracker
    public static void Vamps_DontWantGuestFood(Pawn_GuestTracker __instance, ref bool __result)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(Pawn_GuestTracker), "pawn").GetValue(__instance);
        if (pawn != null && pawn.IsVampire(true)) __result = false;
    }
}