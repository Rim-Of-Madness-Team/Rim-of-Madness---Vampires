using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse.AI;

namespace Vampire;

public class JobDriver_EnterTorpor : JobDriver
{
    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return pawn.Reserve(TargetA, job);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var prepare = Toils_General.Wait(500);
        prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        prepare.WithProgressBarToilDelay(TargetIndex.A);
        yield return prepare;
        yield return new Toil
        {
            initAction = delegate
            {
                var actor = pawn;
                var pod = (Building_Casket)actor.CurJob.targetA.Thing;

                actor.DeSpawn();
                pod.GetDirectlyHeldThings().TryAdd(actor);
                //pod.TryAcceptThing(actor, true);

                //if (!pod.def.building.isPlayerEjectable)
                //{
                //    int freeColonistsSpawnedOrInPlayerEjectablePodsCount = this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                //    if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                //    {
                //        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate().AdjustedFor(actor), action, false, null));
                //    }
                //    else
                //    {
                //        action();
                //    }
                //}
                //else
                //{
                //    action();
                //}
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}