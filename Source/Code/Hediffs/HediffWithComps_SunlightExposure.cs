using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Vampire;

public class HediffWithComps_SunlightExposure : HediffWithComps, ISizeReporter
{
    private readonly int checkRate = 110;
    private int curSunDamage = 2;
    private Effecter sunBurningEffect;
    private Sustainer sustainer;
    public int ticksRemaining = GenDate.TicksPerHour;

    public float CurrentSize()
    {
        switch (CurStageIndex)
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

    public override void Tick()
    {
        base.Tick();

        if (pawn == null && pawn?.Corpse?.InnerPawn == null) return;


        if (sustainer != null && !sustainer.Ended)
        {
            sustainer.Maintain();
        }
        else
        {
            var def = SoundDef.Named("FireBurning");
            var info = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick);
            info.volumeFactor *= 2;
            sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, def, info);
        }

        if (sunBurningEffect != null)
        {
            sunBurningEffect.EffectTick(pawn, pawn);
            if (Find.TickManager.TicksGame % 20 == 0)
            {
                if (CurStageIndex > 1 && Rand.Value > 0.5f) FleckMaker.ThrowSmoke(pawn.DrawPos, pawn.Map, 1f);
                if (CurStageIndex > 1 && Rand.Value < CurStageIndex * 0.31f)
                    FleckMaker.ThrowFireGlow(pawn.PositionHeld.ToVector3(), pawn.Map, 1f);
            }
        }

        if (Find.TickManager.TicksGame % checkRate == 0)
        {
            if (
                VampireSettings.Get.damageToggle && //Damage in sunlight enabled
                pawn?.PositionHeld is { } pos && pos != default && //Position exists
                pos.Roofed(pawn?.MapHeld ?? Find.CurrentMap) == false && //Position has no roofing
                VampireUtility.IsDaylight(pawn)) //In daylight
            {
                if (sunBurningEffect == null)
                {
                    var effecterDef = EffecterDefOf.RoofWork;
                    if (effecterDef != null) sunBurningEffect = effecterDef.Spawn();
                }

                if (CurStageIndex > 1) Burn(); //Time to burn
                Severity += 0.017f;
            }
            else
            {
                curSunDamage = 5;
                Severity -= 0.2f;
                if (sunBurningEffect != null) sunBurningEffect = null;
                if (pawn?.MentalStateDef == VampDefOf.ROMV_Rotschreck) pawn.MentalState.RecoverFromState();
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
                var dmgRange = curSunDamage;
                var dinfo = new DamageInfo(DamageDefOf.Burn, Rand.Range(1, curSunDamage));
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                if (CurStageIndex > 2)
                    curSunDamage += Rand.Range(1, 2);

                ApplyBurnDamage(dinfo);
                if (pawn.Dead) RotCorpseAway();
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
        if (pawn?.mindState?.mentalStateHandler is { } h)
        {
            if (pawn.InMentalState && pawn.MentalStateDef != VampDefOf.ROMV_Rotschreck) h.CurState.RecoverFromState();
            if (CurStageIndex > 1 && Rand.Value < Severity && !pawn.InMentalState)
                h.TryStartMentalState(VampDefOf.ROMV_Rotschreck);
        }
    }

    public void RotCorpseAway()
    {
        if (pawn?.Corpse?.GetComp<CompRottable>() is { } r)
            pawn.Corpse.GetComp<CompRottable>().RotProgress = 999999999f;
    }
}