using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Vampire
{
    /// <summary>
    /// A special version of a projectile.
    /// This one "stores" a base object and "delivers" it.
    /// </summary>
    public class FlyingObject : ThingWithComps
    {
        protected Vector3 origin;
        protected Vector3 destination;
        protected float speed = 30.0f;
        protected int ticksToImpact;
        protected Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        public DamageInfo? impactDamage;

        protected int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((origin - destination).magnitude / (speed / 100f));
                if (num < 1)
                {
                    num = 1;
                }
                return num;
            }
        }


        protected IntVec3 DestinationCell => new IntVec3(destination);

        public virtual Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (destination - origin) * (1f - (float)ticksToImpact / (float)StartingTicksToImpact);
                return origin + b + Vector3.up * def.Altitude;
            }
        }

        public virtual Quaternion ExactRotation => Quaternion.LookRotation(destination - origin);

        public override Vector3 DrawPos => ExactPosition;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref origin, "origin");
            Scribe_Values.Look(ref destination, "destination");
            Scribe_Values.Look(ref ticksToImpact, "ticksToImpact");
            Scribe_Values.Look(ref timesToDamage, "timesToDamage");
            Scribe_Values.Look(ref damageLaunched, "damageLaunched", true);
            Scribe_Values.Look(ref explosion, "explosion");
            Scribe_References.Look(ref assignedTarget, "assignedTarget");
            Scribe_References.Look(ref launcher, "launcher");
            Scribe_References.Look(ref flyingThing, "flyingThing");
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            Launch(launcher, Position.ToVector3Shifted(), targ, flyingThing);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            //Despawn the object to fly
            if (flyingThing.Spawned) flyingThing.DeSpawn();

            this.launcher = launcher;
            this.origin = origin;
            impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            if (targ.Thing != null)
            {
                assignedTarget = targ.Thing;
            }
            destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
            ticksToImpact = StartingTicksToImpact;
        }

        public override void Tick()
        {
            base.Tick();
            Vector3 exactPosition = ExactPosition;
            ticksToImpact--;
            if (!ExactPosition.InBounds(Map))
            {
                ticksToImpact++;
                Position = ExactPosition.ToIntVec3();
                Destroy();
                return;
            }

            Position = ExactPosition.ToIntVec3();
            if (ticksToImpact <= 0)
            {
                if (DestinationCell.InBounds(Map))
                {
                    Position = DestinationCell;
                }
                ImpactSomething();
                return;
            }

        }

        public override void Draw()
        {
            if (flyingThing != null)
            {
                if (flyingThing is Pawn)
                {
                    if (DrawPos == null) return;
                    if (!DrawPos.ToIntVec3().IsValid) return;
                    Pawn pawn = flyingThing as Pawn;
                    pawn.Drawer.DrawAt(DrawPos);
                    //Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.graphic.MatFront, 0);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, DrawPos, ExactRotation, flyingThing.def.DrawMatSingle, 0);
                }
                Comps_PostDraw();
            }
        }

        private void ImpactSomething()
        {
            if (assignedTarget != null)
            {
                Pawn pawn = assignedTarget as Pawn;
                if (pawn != null && pawn.GetPosture() != PawnPosture.Standing && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f)
                {
                    Impact(null);
                    return;
                }
                Impact(assignedTarget);
                return;
            }
            else
            {
                Impact(null);
                return;
            }
        }

        public bool damageLaunched = true;
        public bool explosion = false;
        public int timesToDamage = 3;
        protected virtual void Impact(Thing hitThing)
        {

            if (hitThing == null)
            {

                if (Position.GetThingList(Map).FirstOrDefault(x => x == assignedTarget) is Pawn p)
                {

                    hitThing = p;
                }
            }


            if (impactDamage != null)
            {

                for (int i = 0; i < timesToDamage; i++)
                    if (damageLaunched)
                        flyingThing.TakeDamage(impactDamage.Value);
                    else
                        hitThing.TakeDamage(impactDamage.Value);
                if (explosion)
                    GenExplosion.DoExplosion(Position, Map, 0.9f, DamageDefOf.Stun, this);

            }
            GenSpawn.Spawn(flyingThing, Position, Map);
            Destroy();
        }


    }
}
