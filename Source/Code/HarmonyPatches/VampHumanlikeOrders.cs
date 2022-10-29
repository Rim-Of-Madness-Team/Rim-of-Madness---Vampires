using System;
using System.Collections.Generic;
using JecsTools;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Vampire;

public class VampHumanlikeOrders : FloatMenuPatch
{
    public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>
        GetFloatMenus()
    {
        List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> FloatMenus = new();

        _Condition feedCondition = new _Condition(_ConditionType.IsType, typeof(Pawn));
        var feedFunc = delegate(Vector3 clickPos, Pawn pawn, Thing curThing)
        {
            var opts = new List<FloatMenuOption>();
            var pawnIsVampire = pawn.IsVampire(true);
            if (pawnIsVampire && curThing is Pawn victim && victim != pawn && !victim.RaceProps.IsMechanoid)
            {
                var selVampComp = pawn.GetComp<CompVampire>();
                var curBloodVictim = victim?.BloodNeed()?.CurBloodPoints ?? 0;
                var curBloodActor = pawn?.BloodNeed()?.CurBloodPoints ?? 0;
                var victimIsVampire = victim.IsVampire(true);
                // FEED //////////////////////////
                if (!victimIsVampire || (selVampComp?.Bloodline?.canFeedOnVampires ?? false))
                {
                    Action action = delegate
                    {
                        var job = new Job(VampDefOf.ROMV_Feed, victim);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job);
                    };
                    opts.Add(new FloatMenuOption("ROMV_Feed".Translate(new object[]
                    {
                        victim.LabelCap
                    }) + (curBloodVictim == 1
                        ? new TaggedString(" ") + "ROMV_LethalWarning".Translate()
                        : new TaggedString("")), action, MenuOptionPriority.High, null, victim));
                    // SIP //////////////////////////
                    if (curBloodVictim > 1)
                    {
                        Action action2 = delegate
                        {
                            var job = new Job(VampDefOf.ROMV_Sip, victim);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        opts.Add(new FloatMenuOption("ROMV_Sip".Translate(new object[]
                        {
                            victim.LabelCap
                        }), action2, MenuOptionPriority.High, null, victim));
                    }
                }

                ;
                if (!victimIsVampire && (victim?.RaceProps?.Humanlike ?? false))
                {
                    //EMBRACE /////////////////////
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
                            var job = new Job(VampDefOf.ROMV_Embrace, victim);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        opts.Add(new FloatMenuOption("ROMV_Embrace".Translate(new object[]
                        {
                            victim.LabelCap
                        }), actionTwo, MenuOptionPriority.High, null, victim));
                    }

                    //GIVE BLOOD (Ghoul) ////////////////////
                    if (curBloodActor > 0)
                    {
                        Action actionThree = delegate
                        {
                            var job = new Job(VampDefOf.ROMV_GhoulBloodBond, victim);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job);
                        };
                        opts.Add(new FloatMenuOption(
                            "ROMV_GiveVitae".Translate() +
                            (!victim.IsGhoul()
                                ? new TaggedString(" (") + "ROMV_CreateGhoul".Translate() + ")"
                                : new TaggedString("")), actionThree, MenuOptionPriority.High, null, victim));
                    }
                }


                //Diablerie ////////////////////
                if (victimIsVampire)
                {
                    Action action = delegate
                    {
                        var job = new Job(VampDefOf.ROMV_FeedVampire, victim);
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
                        var job = new Job(VampDefOf.ROMV_Diablerie, victim);
                        job.count = 1;
                        job.playerForced = true;
                        pawn.jobs.TryTakeOrderedJob(job);
                    };
                    string benefitWarning =
                        selVampComp.Generation <= victim.VampComp().Generation
                            ? new TaggedString(" ") + "ROMV_DiablerieNoBenefit".Translate()
                            : new TaggedString("");
                    opts.Add(new FloatMenuOption("ROMV_Diablerie".Translate(new object[]
                    {
                        victim.LabelCap
                    }) + benefitWarning, action2, MenuOptionPriority.High, null, victim));
                }
            }

            return opts;
        };
        KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>> curSec = new(feedCondition,
            feedFunc);
        FloatMenus.Add(curSec);
        return FloatMenus;
    }
}