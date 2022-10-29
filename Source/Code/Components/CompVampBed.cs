using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

/// <summary>
///     Spawns invisible beds on top of objects that are not actually beds.
///     This provids a good way to make sleeping points for vampires.
///     E.g. Caskets, Coffins, Sarcophagi
/// </summary>
public class CompVampBed : ThingComp
{
    private readonly bool harmonyPatchIsActiveForMultipleCharacters = false;
    private Building_Bed bed;

    public bool VampiresOnly = true;

    public Building_Bed Bed
    {
        get => bed;
        set => bed = value;
    }

    public CompProperties_VampBed Props => props as CompProperties_VampBed;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        SpawnBedAsNeeded();
    }

    /// <summary>
    ///     Adds 'Enter Torpor' option.
    /// </summary>
    /// <param name="selPawn"></param>
    /// <returns></returns>
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        //return base.CompFloatMenuOptions(selPawn);

        if ((selPawn?.IsVampire(true) ?? false) && parent is Building_Grave g && !g.HasCorpse)
            yield return new FloatMenuOption("ROMV_EnterTorpor".Translate(new object[]
            {
                selPawn.Label
            }), delegate { selPawn.jobs.TryTakeOrderedJob(new Job(VampDefOf.ROMV_EnterTorpor, parent)); });
    }

    /// <summary>
    ///     Checks to add / remove the bed and assign / unassign bed users as needed.
    /// </summary>
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
                    g.TryGetInnerInteractableThingOwner().Remove(t);
                //Log.Message("Removed " + t.Label);
                return;
            }

            SpawnBedAsNeeded();

            //Remove and add characters to the bed.
            if (bed == null || !bed.Spawned) return;
            var bedPawns = bed?.CompAssignableToPawn?.AssignedPawnsForReading?.ToList();
            if (bedPawns == null) return;
            AssignBedPawnsAsNeeded(g, bedPawns);
        }
    }

    /// <summary>
    ///     If no bed is spawned above the furniture, spawn a bed. This acts as an easy way
    ///     to avoid patching a lot of RimWorld methods.
    /// </summary>
    private void SpawnBedAsNeeded()
    {
        if (bed == null && parent.TryGetInnerInteractableThingOwner().Count == 0)
        {
            ThingDef stuff = null;
            if (parent is Building b) stuff = b.Stuff;
            bed = (Building_Bed)ThingMaker.MakeThing(Props.bedDef, stuff);
            GenSpawn.Spawn(bed, parent.Position, parent.Map, parent.Rotation);
            bed.SetFaction(parent.Faction);
            bed.Notify_ColorChanged();
            parent.Notify_ColorChanged();
        }
    }

    /// <summary>
    ///     Remove and add assignments to the bed as needed.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="bedPawns"></param>
    private void AssignBedPawnsAsNeeded(Building_Grave g, List<Pawn> bedPawns)
    {
        //Log.Message("1");
        if (g?.PositionHeld.GetFirstPawn(g.MapHeld) is { } p && !p.Awake())
        {
            //Log.Message("2a");
            if (bedPawns.Contains(p))
                return;
            bed.CompAssignableToPawn.TryAssignPawn(p);
            bed.Notify_ColorChanged();
            parent.Notify_ColorChanged();
        }
        //else
        //{
        //    Log.Message("2b");
        //    HashSet<Pawn> gravePawns = new HashSet<Pawn>(g.CompAssignableToPawn.AssignedPawnsForReading);
        //    HashSet<Pawn> bedPawnsSet = new HashSet<Pawn>(bedPawns);

        //    Log.Message("3b");

        //    if (gravePawns.SetEquals(bedPawnsSet)) return;
        //    else if (gravePawns.Count() >= 0)
        //    {
        //        if (bedPawnsSet.Count() >= 0)
        //        {
        //            foreach (var bp in bedPawnsSet)
        //            {
        //                bp?.ownership?.UnclaimBed();
        //            }
        //        }
        //    }
        //    else if (gravePawns.Count() < bedPawnsSet.Count() && harmonyPatchIsActiveForMultipleCharacters)
        //    {
        //        foreach (var bp in bedPawnsSet)
        //            g.CompAssignableToPawn.TryAssignPawn(bp);
        //    }   
        //}

//            if (g.AssignedPawns != null && g.AssignedPawns.Count() > 0)
//            {
//                if (bedPawns.Any())
//                {
//                    var tempBedPawns = new List<Pawn>(bedPawns);
//                    foreach (var bedPawn in tempBedPawns)
//                    {
//                        if (!g.AssignedPawns.Contains(bedPawn))
//                            bed.TryUnassignPawn(bedPawn);
//                    }
//                }
//                foreach (var gravePawn in g.AssignedPawns)
//                    bed.TryAssignPawn(gravePawn);
//            }
//            else if (bed.AssignedPawns.Any())
//                foreach (var pawn in bedPawns)
//                    bed.TryUnassignPawn(pawn);
    }


    /// <summary>
    ///     Destroy the bed when we despawn.
    /// </summary>
    /// <param name="map"></param>
    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        if (bed != null && bed.Spawned)
        {
            bed.Destroy();
            bed = null;
        }
    }

    /// <summary>
    ///     Save the bed info.
    /// </summary>
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref bed, "bed");
    }
}