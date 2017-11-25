using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    static partial class HarmonyPatches
    {
        // Verse.Dialog_DebugActionsMenu
        public static void DoListingItems_MapTools_Vamp(Dialog_DebugActionsMenu __instance)
        {
            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoLabel").Invoke(__instance, new object[] { "Tools - Vampirism" });
            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Spawn Vampire (Random)", new Action(()=>
                {
                    Pawn randomVampire =
                    VampireGen.GenerateVampire(VampireUtility.RandHigherGeneration, VampireUtility.RandBloodline, null, null, false);
                    GenSpawn.Spawn(randomVampire, UI.MouseCell(), Find.VisibleMap);

                })
            });


            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Give Vampirism (Default)", new Action(()=>
            {
                Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
                if (pawn != null)
                {
                    if (!pawn.IsVampire())
                    {
                        pawn.health.AddHediff(VampDefOf.ROM_Vampirism, null, null);
                        pawn.Drawer.Notify_DebugAffected();
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, pawn.LabelShort + " is now a vampire", -1f);
                    }
                    else
                        Messages.Message(pawn.LabelCap + " is already a vampire.", MessageTypeDefOf.RejectInput);
                }
            })});

            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Give Vampirism (w/Settings)", new Action(()=>
            {
                Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
                if (pawn != null)
                {
                    //pawn.health.AddHediff(VampDefOf.ROM_Vampirism, null, null);
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(HarmonyPatches.Options_Bloodlines(pawn)));
                    //DebugTools.curTool = null;
                }
            })});

            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Add Blood (1)", new Action(()=>
            {
                Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
                if (pawn != null && pawn?.BloodNeed() is Need_Blood b)
                {
                        b.AdjustBlood(1);
                        pawn.Drawer.Notify_DebugAffected();
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "+1 Blood", -1f);
                }
            })});

            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Drain Blood (1)", new Action(()=>
            {
                Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
                if (pawn != null && pawn?.BloodNeed() is Need_Blood b)
                {
                        b.AdjustBlood(-1);
                        pawn.Drawer.Notify_DebugAffected();
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "-1 Blood", -1f);
                }
            })});


            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolMap").Invoke(__instance, new object[] {
                "Add XP (100)", new Action(()=>
            {
                Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
                if (pawn != null && pawn?.VampComp() is CompVampire v)
                {
                        v.XP += 100;
                        pawn.Drawer.Notify_DebugAffected();
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "+100 XP", -1f);
                }
            })});
        }

        // Verse.DebugTools_Health
        private static List<DebugMenuOption> Options_Bloodlines(Pawn p)
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (BloodlineDef current in DefDatabase<BloodlineDef>.AllDefs)
            {
                list.Add(new DebugMenuOption(current.LabelCap, DebugMenuOptionMode.Action, delegate
                {
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(HarmonyPatches.Options_Generation(p, current)));

                }));
            }
            return list;
        }

        private static List<DebugMenuOption> Options_Generation(Pawn p, BloodlineDef bloodline)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            for (int i = 1; i < 14; i++)
            {
                int curGen = i;
                list.Add(new DebugMenuOption(curGen.ToString(), DebugMenuOptionMode.Action, delegate
                {
                    p.VampComp().InitializeVampirism(null, bloodline, curGen, curGen == 1);
                    //Log.Message("0" + p.LabelShort + " " + i.ToString());
                    p.Drawer.Notify_DebugAffected();
                    MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + " is now a vampire", -1f);
                }));
            }
            return list;
        }

    }
}
