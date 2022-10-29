using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

public class JobDriver_BloodVomit : JobDriver
{
    private int ticksLeft;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft");
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        var to = new Toil
        {
            initAction = delegate
            {
                ticksLeft = Rand.Range(300, 900);
                var num = 0;
                IntVec3 c;
                while (true)
                {
                    c = pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
                    num++;
                    if (num > 12) break;
                    if (c.InBounds(pawn.Map) && c.Standable(pawn.Map)) goto IL_A1;
                }

                c = pawn.Position;
                IL_A1:
                pawn.CurJob.targetA = c;
                pawn.rotationTracker.FaceCell(c);
                pawn.pather.StopDead();
            },
            tickAction = delegate
            {
                var curTicks = ticksLeft;
                if (curTicks % 150 == 149)
                {
                    var sourcePawn = pawn?.VampComp()?.MostRecentVictim != null
                        ? pawn.VampComp()?.MostRecentVictim
                        : pawn;
                    FilthMaker.TryMakeFilth(pawn.CurJob.targetA.Cell, pawn.Map,
                        BloodUtility.GetBloodFilthDef(sourcePawn), sourcePawn.LabelIndefinite());
                    if (pawn.BloodNeed() is { } n && n.CurBloodPoints > 0) n.AdjustBlood(-1);
                }

                ticksLeft--;

                if (curTicks - 1 <= 0)
                {
                    ReadyForNextToil();
                    TaleRecorder.RecordTale(TaleDefOf.Vomited, pawn);
                }
            }
        };
        to.defaultCompleteMode = ToilCompleteMode.Never;
        if (pawn?.VampComp()?.MostRecentVictim != null && pawn?.VampComp()?.MostRecentVictim?.IsCoolantUser() == true)
            to.WithEffect(DefDatabase<EffecterDef>.GetNamed("ROMV_CoolantVomit"), TargetIndex.A);
        else
            to.WithEffect(DefDatabase<EffecterDef>.GetNamed("ROMV_BloodVomit"), TargetIndex.A);
        to.PlaySustainerOrSound(() => SoundDef.Named("Vomit"));
        yield return to;
    }

    public override bool TryMakePreToilReservations(bool uhuh)
    {
        return true;
    }
}