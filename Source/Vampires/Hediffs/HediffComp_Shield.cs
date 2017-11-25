using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public class HediffComp_Shield : HediffComp
    {
        private const float MinDrawSize = 1.2f;

        private const float MaxDrawSize = 1.55f;

        private const float MaxDamagedJitterDist = 0.05f;

        private const int JitterDurationTicks = 8;

        private float energy;

        private int ticksToReset = -1;

        private int lastKeepDisplayTick = -9999;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private int StartingTicksToReset = 3200;

        private float EnergyOnReset = 0.2f;

        private float EnergyLossPerDamage = 0.027f;

        private int KeepDisplayingTicks = 1000;

        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/BloodShield", ShaderDatabase.Transparent);

        public float EnergyMax
        {
            get
            {
                return 1.1f;
            }
        }

        public string labelCap
        {
            get
            {
                return this.Def.LabelCap;
            }
        }
        public string label
        {
            get
            {
                return this.Def.label;
            }
        }

        private float EnergyGainPerTick
        {
            get
            {
                return 0.13f / 60f;
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                return !this.Pawn.Dead && !this.Pawn.Downed && (!this.Pawn.IsPrisonerOfColony || (this.Pawn.MentalStateDef != null && this.Pawn.MentalStateDef.IsAggro)) || (this.Pawn.Faction.HostileTo(Faction.OfPlayer) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<bool>(ref this.setup, "setup", false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        [DebuggerHidden]
        public IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == this.Pawn)
            {
                yield return new Gizmo_HediffShieldStatus
                {
                    shield = this
                };
            }
        }

        //public override float GetSpecialApparelScoreOffset()
        //{
        //    return this.EnergyMax * this.ApparelScorePerEnergyMax;
        //}

        private bool setup = false;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.Pawn == null)
            {
                this.energy = 0f;
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
                this.energy = this.EnergyMax;
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
            if (this.ShieldState == ShieldState.Active && ((dinfo.Instigator != null && !dinfo.Instigator.Position.AdjacentTo8WayOrInside(this.Pawn.Position)) || dinfo.Def.isExplosive))
            {
                if (dinfo.Instigator != null)
                {
                    AttachableThing attachableThing = dinfo.Instigator as AttachableThing;
                    if (attachableThing != null && attachableThing.parent == this.Pawn)
                    {
                        return false;
                    }
                }
                this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
                //if (dinfo.Def == DamageDefOf.EMP)
                //{
                //    this.energy = -1f;
                //}
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                return true;
            }
            return false;
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShieldAbsorbDamage.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.Pawn.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, this.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, this.Pawn.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        public void NotifyRefilled()
        {
            this.energy = this.EnergyMax;
        }

        private void Break()
        {
            SoundDefOf.EnergyShieldBroken.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map, false));
            MoteMaker.MakeStaticMote(this.Pawn.TrueCenter(), this.Pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = this.Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, this.Pawn.Map, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
        }

        private void Reset()
        {
            if (this.Pawn.Spawned)
            {
                SoundDefOf.EnergyShieldReset.PlayOneShot(new TargetInfo(this.Pawn.Position, this.Pawn.Map, false));
                MoteMaker.ThrowLightningGlow(this.Pawn.TrueCenter(), this.Pawn.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }

        public void DrawWornExtras()
        {
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
                Vector3 vector = this.Pawn.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, HediffComp_Shield.BubbleMat, 0);
            }
        }
    }
}
