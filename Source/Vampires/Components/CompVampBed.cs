using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace Vampire
{
    //Spawns invisible beds on things.
    //Perfect for vampires
    public class CompVampBed : ThingComp
    {
        private Building_Bed bed;
        public Building_Bed Bed { get => bed; set => bed = value; }
        public CompProperties_VampBed Props => this.props as CompProperties_VampBed;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (bed == null && this.parent.TryGetInnerInteractableThingOwner().Count == 0)
            {
                ThingDef stuff = null;
                if (this.parent is Building b)
                {
                    stuff = b.Stuff;
                }
                bed = (Building_Bed)ThingMaker.MakeThing(Props.bedDef, stuff);
                GenSpawn.Spawn(bed, this.parent.Position, this.parent.Map, parent.Rotation);
                bed.SetFaction(this.parent.Faction);

            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            //return base.CompFloatMenuOptions(selPawn);

            if ((selPawn?.IsVampire() ?? false) && this.parent is Building_Grave g && !g.HasCorpse)
            {
                yield return new FloatMenuOption("ROMV_EnterTorpor".Translate(new object[]
                {
                    selPawn.Label
                }), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(new Job(VampDefOf.ROMV_EnterTorpor, this.parent));
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (this.parent is Building_Grave g)
            {
                if (g.HasAnyContents)
                {
                    if (bed != null && bed.Spawned)
                    {
                        bed.Destroy(DestroyMode.Vanish);
                        bed = null;
                    }
                    if (g.TryGetInnerInteractableThingOwner().FirstOrDefault(x => x is MinifiedThing) is MinifiedThing t)
                    {
                        g.TryGetInnerInteractableThingOwner().Remove(t);
                        //Log.Message("Removed " + t.Label);
                    }
                    return;
                }

                if (bed == null && this.parent.TryGetInnerInteractableThingOwner().Count == 0)
                {
                    ThingDef stuff = null;
                    if (this.parent is Building b)
                    {
                        stuff = b.Stuff;
                    }
                    bed = (Building_Bed)ThingMaker.MakeThing(Props.bedDef, stuff);
                    GenSpawn.Spawn(bed, this.parent.Position, this.parent.Map, parent.Rotation);
                    bed.SetFaction(this.parent.Faction);
                }
                if (bed != null && bed.Spawned && g.assignedPawn != null && ((bed?.AssignedPawns?.Contains(g.assignedPawn) ?? false) == false))
                {
                    bed.TryAssignPawn(g.assignedPawn);
                }


            }
        }


        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (bed != null && bed.Spawned)
            {
                bed.Destroy(DestroyMode.Vanish);
                bed = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Building_Bed>(ref this.bed, "bed");
        }
    }
}
