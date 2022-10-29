using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Vampire;

[StaticConstructorOnStartup]
public class HediffComp_Shield : HediffComp
{
    private const float MinDrawSize = 1.2f;

    private const float MaxDrawSize = 1.55f;

    private const float MaxDamagedJitterDist = 0.05f;

    private const int JitterDurationTicks = 8;

    private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/BloodShield", ShaderDatabase.Transparent);

    private float energy;

    private readonly float EnergyLossPerDamage = 0.027f;

    private readonly float EnergyOnReset = 0.2f;

    private Vector3 impactAngleVect;

    private readonly int KeepDisplayingTicks = 1000;

    private int lastAbsorbDamageTick = -9999;

    private int lastKeepDisplayTick = -9999;

    //public override float GetSpecialApparelScoreOffset()
    //{
    //    return this.EnergyMax * this.ApparelScorePerEnergyMax;
    //}

    private bool setup;

    private readonly int StartingTicksToReset = 3200;

    private int ticksToReset = -1;

    public float EnergyMax => 1.1f;

    public string labelCap => Def.LabelCap;
    public string label => Def.label;

    private float EnergyGainPerTick => 0.13f / 60f;

    public float Energy => energy;

    public ShieldState ShieldState
    {
        get
        {
            if (ticksToReset > 0) return ShieldState.Resetting;
            return ShieldState.Active;
        }
    }

    private bool ShouldDisplay =>
        (!Pawn.Dead && !Pawn.Downed &&
         (!Pawn.IsPrisonerOfColony || (Pawn.MentalStateDef != null && Pawn.MentalStateDef.IsAggro))) ||
        Pawn.Faction.HostileTo(Faction.OfPlayer) ||
        Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks;

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref energy, "energy");
        Scribe_Values.Look(ref setup, "setup");
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
    }

    [DebuggerHidden]
    public IEnumerable<Gizmo> GetWornGizmos()
    {
        if (Find.Selector.SingleSelectedThing == Pawn)
            yield return new Gizmo_HediffShieldStatus
            {
                shield = this
            };
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (Pawn == null)
        {
            energy = 0f;
            return;
        }

        //if (this.ShieldState == ShieldState.Resetting)
        //{
        //    this.ticksToReset--;
        //    if (this.ticksToReset <= 0)
        //    {
        //        this.Reset();
        //    }
        //}
        if (!setup)
        {
            setup = true;
            energy = EnergyMax;
            KeepDisplaying();
        }
        //{
        //    this.energy += this.EnergyGainPerTick;
        //    if (this.energy > this.EnergyMax)
        //    {
        //        this.energy = this.EnergyMax;
        //    }
        //}
    }

    public bool CheckPreAbsorbDamage(DamageInfo dinfo)
    {
        if (ShieldState == ShieldState.Active && (dinfo.Instigator != null || dinfo.Def.isExplosive))
        {
            if (dinfo.Instigator != null)
            {
                var attachableThing = dinfo.Instigator as AttachableThing;
                if (attachableThing != null && attachableThing.parent == Pawn) return false;
            }

            energy -= dinfo.Amount * EnergyLossPerDamage;
            //if (dinfo.Def == DamageDefOf.EMP)
            //{
            //    this.energy = -1f;
            //}
            if (energy < 0f)
                Break();
            else
                AbsorbedDamage(dinfo);
            return true;
        }

        return false;
    }

    public void KeepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    private void AbsorbedDamage(DamageInfo dinfo)
    {
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        var loc = Pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
        var num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
        FleckMaker.Static(loc, Pawn.Map, FleckDefOf.ExplosionFlash, num);
        var num2 = (int)num;
        for (var i = 0; i < num2; i++) FleckMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(0.8f, 1.2f));
        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        KeepDisplaying();
    }

    public void NotifyRefilled()
    {
        energy = EnergyMax;
    }

    private void Break()
    {
        SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map));
        FleckMaker.Static(Pawn.TrueCenter(), Pawn.Map, FleckDefOf.ExplosionFlash, 12f);
        for (var i = 0; i < 6; i++)
        {
            var loc = Pawn.TrueCenter() +
                      Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
            FleckMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(0.8f, 1.2f));
        }

        energy = 0f;
        ticksToReset = StartingTicksToReset;
    }

    private void Reset()
    {
        if (Pawn.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map));
            FleckMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
        }

        ticksToReset = -1;
        energy = EnergyOnReset;
    }

    public void DrawWornExtras()
    {
        if (ShieldState == ShieldState.Active && ShouldDisplay)
        {
            var num = Mathf.Lerp(1.2f, 1.55f, energy);
            var vector = Pawn.Drawer.DrawPos;
            vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (num2 < 8)
            {
                var num3 = (8 - num2) / 8f * 0.05f;
                vector += impactAngleVect * num3;
                num -= num3;
            }

            float angle = Rand.Range(0, 360);
            var s = new Vector3(num, 1f, num);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }
    }
}