using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vampire;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vampire
{
    public class JobDriver_BloodVomit : JobDriver
    {
        private int ticksLeft;

        private PawnPosture lastPosture;

        public override PawnPosture Posture
        {
            get
            {
                return this.lastPosture;
            }
        }

        public override void Notify_LastPosture(PawnPosture posture, LayingDownState layingDown)
        {
            this.lastPosture = posture;
            this.layingDown = layingDown;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
            Scribe_Values.Look<PawnPosture>(ref this.lastPosture, "lastPosture", PawnPosture.Standing, false);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil to = new Toil()
            {
                initAction = delegate
                {
                    ticksLeft = Rand.Range(300, 900);
                    int num = 0;
                    IntVec3 c;
                    while (true)
                    {
                        c = pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
                        num++;
                        if (num > 12)
                        {
                            break;
                        }
                        if (c.InBounds(pawn.Map) && c.Standable(pawn.Map))
                        {
                            goto IL_A1;
                        }
                    }
                    c = pawn.Position;
                    IL_A1:
                    pawn.CurJob.targetA = c;
                    pawn.rotationTracker.FaceCell(c);
                    pawn.pather.StopDead();
                },
                tickAction = delegate
                {
                    int curTicks = ticksLeft;
                    if (curTicks % 150 == 149)
                    {
                        FilthMaker.MakeFilth(pawn.CurJob.targetA.Cell, pawn.Map, ThingDefOf.FilthBlood, pawn.LabelIndefinite(), 1);
                        if (pawn.BloodNeed() is Need_Blood n && n.CurBloodPoints > 0)
                        {
                            n.AdjustBlood(-1);
                        }
                    }
                    ticksLeft--;

                    if (curTicks - 1 <= 0)
                    {

                        ReadyForNextToil();
                        TaleRecorder.RecordTale(TaleDefOf.Vomited, new object[]
                        {
                                pawn
                        });

                    }

                }
            };
            to.defaultCompleteMode = ToilCompleteMode.Never;
            to.WithEffect(EffecterDef.Named("ROMV_BloodVomit"), TargetIndex.A);
            to.PlaySustainerOrSound(() => SoundDef.Named("Vomit"));
            yield return to;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
