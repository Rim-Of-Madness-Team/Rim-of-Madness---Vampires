using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace Vampire
{
    public class HediffWithComps_SunlightExposure : HediffWithComps, ISizeReporter
    {
        private int curSunDamage = 2;
        private int checkRate = 110;
        public int ticksRemaining = GenDate.TicksPerHour;
        private Sustainer sustainer = null;
        private Effecter sunBurningEffect = null;

        public override void Tick()
        {
            base.Tick();

            if (this.pawn == null && this.pawn?.Corpse?.InnerPawn == null)
            {
                return;
            }


            if (this.sustainer != null && !this.sustainer.Ended)
            {
                this.sustainer.Maintain();
            }
            else
            {
                //LongEventHandler.ExecuteWhenFinished(delegate
                //{
                SoundDef def = SoundDef.Named("FireBurning");
                SoundInfo info = SoundInfo.InMap(new TargetInfo(this.pawn.Position, this.pawn.Map, false), MaintenanceType.PerTick);
                info.volumeFactor *= 2;
                //this.sustainer = def.TrySpawnSustainer(info); //SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, def, info);
                this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, def, info);
                //});
            }

            if (sunBurningEffect != null)
            {
                sunBurningEffect.EffectTick(this.pawn, this.pawn);
                if (Find.TickManager.TicksGame % 20 == 0)
                {
                    if (this.CurStageIndex > 1 && Rand.Value > 0.5f) MoteMaker.ThrowSmoke(this.pawn.DrawPos, this.pawn.Map, 1f);
                    if (this.CurStageIndex > 1 && Rand.Value < (this.CurStageIndex * 0.31f))
                        MoteMaker.ThrowFireGlow(this.pawn.PositionHeld, this.pawn.Map, 1f);
                }
            }
            if (Find.TickManager.TicksGame % checkRate == 0)
            {
                if (pawn?.PositionHeld is IntVec3 pos && pos != default(IntVec3) && pos.Roofed(pawn?.MapHeld ?? Find.VisibleMap) == false &&
                    VampireUtility.IsDaylight(pawn))
                {
                    if (sunBurningEffect == null)
                    {
                        EffecterDef effecterDef = EffecterDefOf.RoofWork;
                        if (effecterDef != null)
                        {
                            sunBurningEffect = effecterDef.Spawn();
                        }
                    }

                    if (this.CurStageIndex > 1)
                    {
                        Burn();
                    }
                    this.Severity += 0.017f;
                }
                else
                {
                    curSunDamage = 5;
                    this.Severity -= 0.2f;
                    if (sunBurningEffect != null) sunBurningEffect = null;
                    if (pawn?.MentalStateDef == VampDefOf.ROMV_Rotschreck) { pawn.MentalState.RecoverFromState();  }
                    if (pawn?.CurJob?.def == VampDefOf.ROMV_DigAndHide) pawn.jobs.StopAll();
                }
            }
        }

        public void Burn()
        {
            if (pawn != null)
            {                
                if (!pawn.Dead)
                {
                    int dmgRange = curSunDamage;
                    DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, Rand.Range(1, curSunDamage), -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                    dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                    if (this.CurStageIndex > 2)
                        curSunDamage += Rand.Range(1, 2);

                    ApplyBurnDamage(dinfo);
                    if (pawn.Dead)
                    {
                        RotCorpseAway();
                    }
                }
                else
                {
                    RotCorpseAway();
                }

            }
        }

        public void ApplyBurnDamage(DamageInfo dinfo)
        {
            pawn.TakeDamage(dinfo);
            if (pawn?.mindState?.mentalStateHandler is MentalStateHandler h)
            {
                if (pawn.InMentalState && pawn.MentalStateDef != VampDefOf.ROMV_Rotschreck)
                {
                    h.CurState.RecoverFromState();
                }
                if (this.CurStageIndex > 1 && Rand.Value < this.Severity && !pawn.InMentalState)
                {
                    h.TryStartMentalState(VampDefOf.ROMV_Rotschreck);
                }
            }
        }

        public void RotCorpseAway()
        {

            if (pawn?.Corpse?.GetComp<CompRottable>() is CompRottable r)
            {
                pawn.Corpse.GetComp<CompRottable>().RotProgress = 999999999f;
            }
        }

        public float CurrentSize()
        {
            switch (this.CurStageIndex)
            {
                case 1:
                    return 8;
                case 2:
                    return 14;
                case 3:
                    return 20;
                case 4:
                    return 30;
                default:
                    return 0;
            }
        }
    }
}
