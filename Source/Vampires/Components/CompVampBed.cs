using System.Collections.Generic;
using System.Linq;
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
        public CompProperties_VampBed Props => props as CompProperties_VampBed;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            
            if (bed == null && parent.TryGetInnerInteractableThingOwner().Count == 0)
            {
                ThingDef stuff = null;
                if (parent is Building b)
                {
                    stuff = b.Stuff;
                }
                bed = (Building_Bed)ThingMaker.MakeThing(Props.bedDef, stuff);
                GenSpawn.Spawn(bed, parent.Position, parent.Map, parent.Rotation);
                bed.SetFaction(parent.Faction);

            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            //return base.CompFloatMenuOptions(selPawn);

            if ((selPawn?.IsVampire() ?? false) && parent is Building_Grave g && !g.HasCorpse)
            {
                yield return new FloatMenuOption("ROMV_EnterTorpor".Translate(new object[]
                {
                    selPawn.Label
                }), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(new Job(VampDefOf.ROMV_EnterTorpor, parent));
                });
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent is Building_Grave g)
            {
                if (g.HasAnyContents)
                {
                    if (bed != null && bed.Spawned)
                    {
                        bed.Destroy();
                        bed = null;
                    }
                    if (g.TryGetInnerInteractableThingOwner().FirstOrDefault(x => x is MinifiedThing) is MinifiedThing t)
                    {
                        g.TryGetInnerInteractableThingOwner().Remove(t);
                        //Log.Message("Removed " + t.Label);
                    }
                    return;
                }

                if (bed == null && parent.TryGetInnerInteractableThingOwner().Count == 0)
                {
                    ThingDef stuff = null;
                    if (parent is Building b)
                    {
                        stuff = b.Stuff;
                    }
                    bed = (Building_Bed)ThingMaker.MakeThing(Props.bedDef, stuff);
                    GenSpawn.Spawn(bed, parent.Position, parent.Map, parent.Rotation);
                    bed.SetFaction(parent.Faction);
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
                bed.Destroy();
                bed = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref bed, "bed");
        }
    }
}
