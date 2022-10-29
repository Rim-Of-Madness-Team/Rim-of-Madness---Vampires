using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_GhoulBloodBond : JobDriver
{
    private const float BaseFeedTime = 320f;
    private const float BaseEmbraceTime = 1000f;
    private float workLeft = -1f;

    protected Pawn Victim
    {
        get
        {
            if (job.targetA.Thing is Pawn p) return p;
            if (job.targetA.Thing is Corpse c) return c.InnerPawn;
            return null;
        }
    }

    protected CompVampire CompThrall => Victim.GetComp<CompVampire>();
    protected CompVampire CompMaster => GetActor().GetComp<CompVampire>();
    protected Need_Blood BloodThrall => CompThrall.BloodPool;
    protected Need_Blood BloodMaster => CompMaster.BloodPool;

    private void DoEffect()
    {
        BloodMaster.TransferBloodTo(1, BloodThrall);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        //this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(delegate { return pawn == Victim; });
        this.FailOn(delegate { return BloodMaster.CurBloodPoints == 0; });
        this.FailOnAggroMentalState(TargetIndex.A);

        yield return Toils_Reserve.Reserve(TargetIndex.A);
        var newDomitor = GetActor();
        var toil = newDomitor?.Faction == TargetA.Thing?.Faction
            ? Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            : Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        var gotoToil = toil;
        yield return gotoToil;
        var grappleToil = new Toil
        {
            initAction = delegate
            {
                MoteMaker.MakeColonistActionOverlay(newDomitor, ThingDefOf.Mote_ColonistAttacking);

                workLeft = BaseFeedTime;
                var victim = TargetA.Thing as Pawn;
                if (victim != null)
                {
                    if (newDomitor.InAggroMentalState || victim.InAggroMentalState ||
                        victim.Faction != newDomitor.Faction)
                    {
                        var grappleBonus = newDomitor is PawnTemporary ? 100 : 0;
                        if (!JecsTools.GrappleUtility.TryGrapple(newDomitor, victim, grappleBonus))
                        {
                            EndJobWith(JobCondition.Incompletable);
                            PawnUtility.ForceWait(newDomitor, (int)(BaseFeedTime * 0.15f));
                            return;
                        }
                    }

                    if (newDomitor.IsVampire(true))
                        //VampireBiteUtility.MakeNew(GetActor(), GetActor()); //Actor opens their own wound.
                        victim.stances.stunner.StunFor((int)BaseFeedTime, newDomitor);
                }
            }
        };
        yield return grappleToil;
        var feedToil = new Toil
        {
            tickAction = delegate
            {
                //try
                //{
                if (TargetA.Thing is Pawn victim && victim.Spawned && !victim.Dead)
                {
                    workLeft--;
                    VampireWitnessUtility.HandleWitnessesOf(job.def, newDomitor, victim);
//                            if (victim?.needs?.mood?.thoughts?.memories != null)
//                            {
//                                var victimThoughtDef = VampDefOf.ROMV_IDrankVitae;
//                                Thought_Memory victimThought = null;
//                                if (victimThoughtDef != null) victimThought = (Thought_Memory)ThoughtMaker.MakeThought(victimThoughtDef);
//                                if (victimThought != null)
//                                {
//                                    victim.needs.mood.thoughts.memories.TryGainMemory(victimThought);
//                                }
//                            }
                    if (workLeft <= 0f)
                    {
                        if (newDomitor?.VampComp() is { } v && v.IsVampire &&
                            newDomitor.Faction == Faction.OfPlayer)
                        {
                            MoteMaker.ThrowText(newDomitor.DrawPos, newDomitor.Map, "XP +" + 15);
                            v.XP += 15;
                        }

                        workLeft = BaseFeedTime;
                        MoteMaker.MakeColonistActionOverlay(newDomitor, ThingDefOf.Mote_ColonistAttacking);

                        if (!victim.IsGhoul())
                            CompThrall.InitializeGhoul(newDomitor);
                        else
                            CompThrall.ThrallData.TryAdjustBondStage(newDomitor, 1);
                        BloodMaster.TransferBloodTo(1, BloodThrall, true, true);
                        GhoulUtility.GiveVitaeEffects(victim, newDomitor);
                        //VampireBiteUtility.CleanBite(GetActor(), GetActor());
                        ReadyForNextToil();
                    }
                }
                else
                {
                    ReadyForNextToil();
                }
                //}
                //catch(Exception e)
                //{
                //    Log.Message(e.ToString());
                //    thisDriver.ReadyForNextToil();
                //}
            },
            defaultCompleteMode = ToilCompleteMode.Never
        };
        feedToil.socialMode = RandomSocialMode.Off;
        feedToil.WithProgressBar(TargetIndex.A, () => 1f - workLeft / BaseFeedTime);
        feedToil.PlaySustainerOrSound(() => ThingDefOf.Beer.ingestible.ingestSound);
        yield return feedToil;
    }

    public static bool ShouldContinueFeeding(Pawn feeder, Pawn victim)
    {
        return false;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}