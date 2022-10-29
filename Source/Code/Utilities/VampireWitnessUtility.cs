using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Vampire;

/// <summary>
///     When crimes are committed, like feeding on visitors,
///     these are some reactions they can have
/// </summary>
public enum CrimeReaction
{
    None,
    MoodColonist,
    MoodVisitor,
    MoodVisitorFlee,
    MoodVisitorAggro
}

/// <summary>
/// </summary>
public struct Crime
{
    public JobDef CrimeDef;
    public ThoughtDef ColonistThought;
    public ThoughtDef VisitorThought;

    public Crime(JobDef newCrimeDef, ThoughtDef newColonistThought, ThoughtDef newVisitorThought)
    {
        CrimeDef = newCrimeDef;
        ColonistThought = newColonistThought;
        VisitorThought = newVisitorThought;
    }
}

/// <summary>
///     This class handles all issues where vampires are feeding on
///     folks in public.
/// </summary>
public static class VampireWitnessUtility
{
    public static List<Crime> AllCrimes()
    {
        return new List<Crime>
        {
            new(VampDefOf.ROMV_Feed,
                VampDefOf.ROMV_WitnessedVampireFeeding, VampDefOf.ROMV_WitnessedVampireFeedingVisitor),
            new(VampDefOf.ROMV_Embrace, VampDefOf.ROMV_WitnessedVampireEmbrace,
                VampDefOf.ROMV_WitnessedVampireEmbraceVisitor),
            new(VampDefOf.ROMV_Diablerie, VampDefOf.ROMV_WitnessedVampireDiablerie,
                VampDefOf.ROMV_WitnessedVampireDiablerieVisitor)
        };
    }

    public static Crime? GetCrime(JobDef jobDef)
    {
        return AllCrimes().FirstOrDefault(x => x.CrimeDef == jobDef);
    }

    /// <summary>
    ///     Gets all the witnesses of a criminal's acts.
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns></returns>
    public static List<Pawn> WitnessesOf(Pawn criminal, Pawn victim, JobDef crime)
    {
        List<Pawn> result = null;
        var map = criminal.Map;
        var num = 0;
        while (num < 100f)
        {
            var intVec = criminal.Position + GenRadial.RadialPattern[num];
            if (intVec.InBounds(map))
                if (GenSight.LineOfSight(intVec, criminal.Position, map, true))
                {
                    var thingList = intVec.GetThingList(map);
                    for (var i = 0; i < thingList.Count; i++)
                        //
                        if (thingList[i] is Pawn p && p.ShouldCareAboutCrime(criminal, victim, crime))
                        {
                            if (result == null) result = new List<Pawn>();
                            result.Add(p);
                        }
                }

            num++;
        }

        return result;
    }

    public static CrimeReaction GetReactionTo(this Pawn witness, Pawn criminal, Pawn victim)
    {
        if (witness.Faction != null && witness.Faction != criminal.Faction)
        {
            if (victim.Faction == witness.Faction)
            {
                if (!witness.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                    return CrimeReaction.MoodVisitorFlee;
                return CrimeReaction.MoodVisitorAggro;
            }

            return CrimeReaction.MoodVisitor;
        }

        return CrimeReaction.MoodColonist;
    }

    public static bool ShouldCareAboutCrime(this Pawn witness, Pawn criminal, Pawn victim, JobDef crime)
    {
        //Main criminals and victims will not "observe" the crime, as they are in the crime in progress.
        if (witness == criminal)
            return false;
        if (witness == victim)
            return false;

        //Vampires are only upset by diablerie
        if (witness.IsVampire(true) && crime != VampDefOf.ROMV_Diablerie)
            return false;

        //Don't test animals
        if (!witness.RaceProps.Humanlike)
            return false;

        //The blind or sleeping will not care.
        if (!witness.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            return false;
        if (!witness.Awake())
            return false;

        //Traits are handled for same faction
        if (witness.Faction == victim.Faction)
            if (witness.story.traits.HasTrait(TraitDefOf.Psychopath) ||
                witness.story.traits.HasTrait(TraitDefOf.Bloodlust))
                return false;
        return true;
    }

    //This prevents issues with downed characters trying to take jobs, etc.
    public static bool CanTakeWitnessJob(Pawn witness)
    {
        return !witness.Dead && !witness.Downed && !witness.IsFighting() && witness.CurJob is { } j &&
               j.def != JobDefOf.FleeAndCower && j.def != JobDefOf.AttackMelee;
    }


    public static void HandleWitnessesOf(JobDef crime, Pawn criminal, Pawn victim)
    {
        if (!criminal.IsVampire(true)) return;

        var curCrime = GetCrime(crime);
        if (curCrime == null) return;

        //Log.Message("1");
        var witnesses = WitnessesOf(criminal, victim, crime);
        //Log.Message("2");


        if (!witnesses.NullOrEmpty())
            //Log.Message("Loop 1 Enter");


            foreach (var witness in witnesses)
            {
                //Log.Message("Loop 1 Step 1");

                //Log.Message("Loop 1 Step 2");

                Thought_MemoryObservation thought_MemoryObservation = null;
                //Log.Message("Loop 1 Step 3");

                switch (witness.GetReactionTo(criminal, victim))
                {
                    case CrimeReaction.MoodColonist:
                        if (curCrime.Value.ColonistThought != null)
                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                    .MakeThought(curCrime.Value.ColonistThought);
                        break;
                    case CrimeReaction.MoodVisitor:
                        if (curCrime.Value.VisitorThought != null)

                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                    .MakeThought(curCrime.Value.VisitorThought);
                        break;
                    case CrimeReaction.MoodVisitorFlee:
                        if (curCrime.Value.VisitorThought != null)
                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                    .MakeThought(curCrime.Value.VisitorThought);
                        if (CanTakeWitnessJob(witness))
                        {
                            var fleeLoc = CellFinderLoose.GetFleeDest(witness, new List<Thing> { criminal });
                            witness.jobs.StartJob(new Job(JobDefOf.FleeAndCower, fleeLoc));
                            if (witness.Faction != null && !witness.Faction.HostileTo(criminal.Faction))
                                witness.Faction.RelationWith(criminal.Faction).baseGoodwill = -100;
                        }

                        break;
                    case CrimeReaction.MoodVisitorAggro:
                        if (curCrime.Value.VisitorThought != null)
                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                    .MakeThought(curCrime.Value.VisitorThought);
                        if (CanTakeWitnessJob(witness))
                        {
                            witness.jobs.StartJob(new Job(JobDefOf.AttackMelee, criminal));
                            if (witness.Faction != null && !witness.Faction.HostileTo(criminal.Faction))
                                witness.Faction.RelationWith(criminal.Faction).baseGoodwill = -100;
                        }

                        break;
                }
                //Log.Message("Loop 1 Step 6");

                if (thought_MemoryObservation != null)
                {
                    //Log.Message("Loop 1 Step 7");
                    thought_MemoryObservation.Target = criminal;
                    //Log.Message("Loop 1 Step 8");

                    witness.needs.mood.thoughts.memories.TryGainMemory(thought_MemoryObservation);
                    //Log.Message("Loop 1 Step 9");
                }
            }
    }
}