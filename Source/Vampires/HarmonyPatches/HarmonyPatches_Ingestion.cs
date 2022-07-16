using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_Ingestion(Harmony harmony)
        {
            //Float Menus: Adds vampire blood consumption/consume buttons and hide regular consumption/consume
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FloatMenus_Consume)));


            //Vampires should not try to do drugs when idle.
            harmony.Patch(AccessTools.Method(typeof(JobGiver_IdleJoy), "TryGiveJob"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamps_DontDoIdleDrugs)));


            //Vampires should not be given food by wardens.
            harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "get_CanBeBroughtFood"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamps_DontWantGuestFood)));


            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "InappropriateForTitle"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WhoNeeds___Titles)), null);

            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WillEat_Nothing)), null);
            
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillEat", new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool) }),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(WillEat_NothingDef)), null);
            
            
            harmony.Patch(AccessTools.Method(typeof(Toils_Ingest), "FinalizeIngest"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(VampiresDontIngestFood)), null);


            //Fixes random red errors relating to food need checks in this method (WillIngestStackCountOf).
            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "WillIngestStackCountOf"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_WillIngestStackCountOf)), null);
            //Log.Message("04");
            //Prevents restful times.
            harmony.Patch(AccessTools.Method(typeof(JoyGiver_SocialRelax), "TryFindIngestibleToNurse"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(INeverDrink___Wine)), null);

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
            if (ingester.jobs.curJob.GetTarget(ingestibleInd).Thing.def.graphicData.texPath == "Things/Item/Resource/BloodWine")
                return true;

            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ingestibleInd).Thing;
                if (thing.def.IsNutritionGivingIngestible)
                {
                    if (Rand.Range(0f, 1f) > 0.5f)
                        ingester.jobs.StartJob(JobMaker.MakeJob(VampDefOf.ROMV_BloodVomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
                }
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
        public static bool WillEat_Nothing(Pawn p, Thing food, Pawn getter, bool careIfNotAcceptableForTitle, ref bool __result)
        {
            if (p.IsVampire(true))
            {
                if (food?.TryGetComp<CompBloodItem>() is CompBloodItem c)
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
        public static bool WillEat_NothingDef(Pawn p, ThingDef food, Pawn getter, bool careIfNotAcceptableForTitle, ref bool __result)
        {
            if (p.IsVampire(true))
            {
                if (food?.GetCompProperties<CompProperties_BloodItem>() is CompProperties_BloodItem c)
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
            IntVec3 c = IntVec3.FromVector3(clickPos);
            CompVampire selVampComp = pawn.VampComp();
            bool pawnIsVampire = pawn.IsVampire(true);
            if (selVampComp != null && pawnIsVampire)
            {
                //Hide food consumption from menus.
                Thing food = c.GetThingList(pawn.Map)
                    .FirstOrDefault(t => t.GetType() != typeof(Pawn) && t?.def?.ingestible != null);
                if (food != null)
                {
                    //string text = (!food.def.ingestible.ingestCommandString.NullOrEmpty()) ? string.Format(food.def.ingestible.ingestCommandString, food.LabelShort) : ((string)"ConsumeThing".Translate(food.LabelShort, food));
                    string text = "";

                    if (food?.def?.ingestible?.ingestCommandString == null ||
                        food.def.ingestible.ingestCommandString == "")
                    {
                        text = ((string)"ConsumeThing".Translate(food.LabelShort, food));
                    }
                    else
                    {
                        text = string.Format(food.def.ingestible.ingestCommandString, food.LabelShort);
                    }

                    FloatMenuOption o = opts.FirstOrDefault(x => x.Label.Contains(text));
                    if (o != null)
                    {
                        opts.Remove(o);
                    }
                }

                //Hide corpse consumption from menus.
                Thing corpse = c.GetThingList(pawn.Map).FirstOrDefault(t => t is Corpse);
                if (corpse != null)
                {
                    string text;
                    if (corpse.def.ingestible.ingestCommandString.NullOrEmpty())
                    {
                        text = "ConsumeThing".Translate(new object[]
                        {
                            corpse.LabelShort
                        });
                    }
                    else
                    {
                        text = string.Format(corpse.def.ingestible.ingestCommandString, corpse.LabelShort);
                    }

                    FloatMenuOption o = opts.FirstOrDefault(x => x.Label.Contains(text));
                    if (o != null)
                    {
                        opts.Remove(o);
                    }
                }

                //Add blood consumption
                Thing bloodItem = c.GetThingList(pawn.Map)
                    .FirstOrDefault(t => t.def.GetCompProperties<CompProperties_BloodItem>() != null);
                if (bloodItem != null)
                {
                    string text = "";
                    if (bloodItem.def.ingestible.ingestCommandString.NullOrEmpty())
                    {
                        text = "ConsumeThing".Translate(new object[]
                        {
                            bloodItem.LabelShort
                        });
                    }
                    else
                    {
                        text = string.Format(bloodItem.def.ingestible.ingestCommandString, bloodItem.LabelShort);
                    }

                    if (!bloodItem.IsSociallyProper(pawn))
                    {
                        text = text + " (" + "ReservedForPrisoners".Translate() + ")";
                    }

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
                        MenuOptionPriority priority =
                            !(bloodItem is Corpse) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                        item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
                        {
                            bloodItem.SetForbidden(false);
                            Job job = new Job(VampDefOf.ROMV_ConsumeBlood, bloodItem);
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
            if (pawn.IsVampire(true) && __result is Job j && j.def == JobDefOf.Ingest &&
                j.targetA.Thing is ThingWithComps t && t.def.IsDrug && t.def.graphicData.texPath != "Things/Item/Resource/BloodWine")
                __result = null;
        }

        // RimWorld.Pawn_GuestTracker
        public static void Vamps_DontWantGuestFood(Pawn_GuestTracker __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_GuestTracker), "pawn").GetValue(__instance);
            if (pawn != null && pawn.IsVampire(true))
            {
                __result = false;
            }
        }



    }
}
