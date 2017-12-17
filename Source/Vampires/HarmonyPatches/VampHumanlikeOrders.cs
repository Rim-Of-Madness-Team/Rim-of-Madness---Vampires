using System;
using System.Collections.Generic;
using JecsTools;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire
{
    public class VampHumanlikeOrders : FloatMenuPatch
    {
        public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> GetFloatMenus()
        {
            List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> FloatMenus = new List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>();
            
            _Condition feedCondition = new _Condition(_ConditionType.IsType, typeof(Pawn));
            Func <Vector3, Pawn, Thing, List<FloatMenuOption>> feedFunc = delegate (Vector3 clickPos, Pawn pawn, Thing curThing)
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();
                bool pawnIsVampire = pawn.IsVampire();
                if (pawnIsVampire && curThing is Pawn victim && victim != pawn && !victim.RaceProps.IsMechanoid)
                {

                    CompVampire selVampComp = pawn.GetComp<CompVampire>();
                    int curBloodVictim = victim?.BloodNeed()?.CurBloodPoints ?? 0;
                    bool victimIsVampire = victim.IsVampire();
                    // FEED //////////////////////////
                    if (!victimIsVampire || (selVampComp?.Bloodline?.canFeedOnVampires ?? false))
                    {
                        Action action = delegate
                        {
                            Job job = new Job(VampDefOf.ROMV_Feed, victim);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        opts.Add(new FloatMenuOption("ROMV_Feed".Translate(new object[]
                        {
                                victim.LabelCap
                        }) + ((curBloodVictim == 1) ? " " + "ROMV_LethalWarning".Translate() : ""), action, MenuOptionPriority.High, null, victim));
                        // SIP //////////////////////////
                        if (curBloodVictim > 1)
                        {
                            Action action2 = delegate
                            {
                                Job job = new Job(VampDefOf.ROMV_Sip, victim);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            };
                            opts.Add(new FloatMenuOption("ROMV_Sip".Translate(new object[]
                            {
                            victim.LabelCap
                            }), action2, MenuOptionPriority.High, null, victim));
                        }
                    };
                    //EMBRACE /////////////////////
                    if (victim?.RaceProps?.Humanlike ?? false)
                    {
                        if (selVampComp.Thinblooded)
                        {
                            opts.Add(new FloatMenuOption("ROMV_CannotEmbrace".Translate(new object[]
                            {
                            victim.LabelCap
                            } + " (" + "ROMV_Thinblooded".Translate() + ")"), null, MenuOptionPriority.High, null, victim));
                        }
                        else
                        {
                            Action actionTwo = delegate
                            {
                                Job job = new Job(VampDefOf.ROMV_Embrace, victim);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            };
                            opts.Add(new FloatMenuOption("ROMV_Embrace".Translate(new object[]
                            {
                            victim.LabelCap
                            }), actionTwo, MenuOptionPriority.High, null, victim));
                        }
                    }

                    //Diablerie /////////////////////
                    if (victimIsVampire)
                    {
                        Action action = delegate
                        {
                            Job job = new Job(VampDefOf.ROMV_FeedVampire, victim);
                            job.count = 1;
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        opts.Add(new FloatMenuOption("ROMV_FeedVampire".Translate(new object[]
                        {
                                victim.LabelCap
                        }), action, MenuOptionPriority.High, null, victim));
                        Action action2 = delegate
                        {
                            Job job = new Job(VampDefOf.ROMV_Diablerie, victim);
                            job.count = 1;
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        string benefitWarning = (selVampComp.Generation < victim.VampComp().Generation) ? " " + "ROMV_DiablerieNoBenefit".Translate() : "";
                        opts.Add(new FloatMenuOption("ROMV_Diablerie".Translate(new object[]
                        {
                                victim.LabelCap
                        }) + benefitWarning, action2, MenuOptionPriority.High, null, victim));
                    }

                }
                return opts;
            };
            KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>> curSec = new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>(feedCondition, feedFunc);
            FloatMenus.Add(curSec);
            return FloatMenus;
        }
        
    }
}
