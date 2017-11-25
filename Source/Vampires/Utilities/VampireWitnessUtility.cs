using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Vampire
{
    /// <summary>
    /// When crimes are committed, like feeding on visitors,
    /// these are some reactions they can have
    /// </summary>
    public enum CrimeReaction
    {
        None,
        MoodColonist,
        MoodVisitorFlee,
        MoodVisitorAggro
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Crime
    {
        public JobDef CrimeDef;
        public ThoughtDef ColonistThought;
        public ThoughtDef VisitorThought;
        
        public Crime(JobDef newCrimeDef, ThoughtDef newColonistThought, ThoughtDef newVisitorThought)
        {
            this.CrimeDef = newCrimeDef;
            this.ColonistThought = newColonistThought;
            this.VisitorThought = newVisitorThought;
        }
    }

    /// <summary>
    /// This class handles all issues where vampires are feeding on
    /// folks in public.
    /// </summary>
    public static class VampireWitnessUtility
    {
        public static List<Crime> AllCrimes()
        {
            return new List<Crime>()
            {
                new Crime(VampDefOf.ROMV_Feed, 
                    VampDefOf.ROMV_WitnessedVampireFeeding, VampDefOf.ROMV_WitnessedVampireFeedingVisitor),
                new Crime(VampDefOf.ROMV_Embrace, VampDefOf.ROMV_WitnessedVampireEmbrace, VampDefOf.ROMV_WitnessedVampireEmbraceVisitor),
                new Crime(VampDefOf.ROMV_Diablerie, VampDefOf.ROMV_WitnessedVampireDiablerie, VampDefOf.ROMV_WitnessedVampireDiablerieVisitor)
            };
        }

        public static Crime GetCrime(JobDef jobDef)
        {
            return AllCrimes().FirstOrDefault(x => x.CrimeDef == jobDef);
        }

        /// <summary>
        /// Gets all the witnesses of a criminal's acts.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static List<Pawn> WitnessesOf(Pawn criminal, Pawn victim)
        {
            List<Pawn> result = null;
            Map map = criminal.Map;
            int num = 0;
            while ((float)num < 100f)
            {
                IntVec3 intVec = criminal.Position + GenRadial.RadialPattern[num];
                if (intVec.InBounds(map))
                {
                    if (GenSight.LineOfSight(intVec, criminal.Position, map, true, null, 0, 0))
                    {
                        List<Thing> thingList = intVec.GetThingList(map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            //
                            if (thingList[i] is Pawn p && p.ShouldCareAboutCrime(criminal, victim))
                            {
                                if (result == null) result = new List<Pawn>();
                                result.Add(p);
                            }
                        }
                    }
                }
                num++;
            }
            return result;
        }
        
        public static CrimeReaction GetReactionTo(this Pawn witness, Pawn criminal, JobDef crime)
        {
            if (witness.Faction != null && witness.Faction != criminal.Faction)
            {
                if (!witness.story.WorkTagIsDisabled(WorkTags.Violent))
                    return CrimeReaction.MoodVisitorFlee;
                return CrimeReaction.MoodVisitorAggro;
            }
            return CrimeReaction.MoodColonist;
        }

        public static bool ShouldCareAboutCrime(this Pawn witness, Pawn criminal, Pawn victim)
        {
            //Main criminals and victims will not "observe" the crime, as they are in the crime in progress.
            if (witness == criminal)
                return false;
            if (witness == victim)
                return false;

            //Don't test animals
            if (!witness.RaceProps.Humanlike)
                return false;

            //The blind or sleeping will not care.
            if (!witness.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                return false;
            if (!RestUtility.Awake(witness))
                return false;
            
            //Traits are handled for same faction
            if (witness.Faction == victim.Faction)
            {
                if (witness.story.traits.HasTrait(TraitDefOf.Psychopath) ||
                    witness.story.traits.HasTrait(TraitDefOf.Bloodlust))
                    return false;
            }
            return true;
        }

        public static void HandleWitnessesOf(JobDef crime, Pawn criminal, Pawn victim)
        {
            //Log.Message("1");
            List<Pawn> witnesses = VampireWitnessUtility.WitnessesOf(criminal, victim);
            //Log.Message("2");

            if (!witnesses.NullOrEmpty())
            {
                //Log.Message("Loop 1 Enter");

                foreach (Pawn witness in witnesses)
                {
                    //Log.Message("Loop 1 Step 1");

                    Crime curCrime = GetCrime(crime);
                    //Log.Message("Loop 1 Step 2");

                    Thought_MemoryObservation thought_MemoryObservation = null;
                    //Log.Message("Loop 1 Step 3");

                    switch (witness.GetReactionTo(criminal, crime))
                    {
                        case CrimeReaction.MoodColonist:
                            //Log.Message("Loop 1 Step 4");

                            thought_MemoryObservation = 
                                (Thought_MemoryObservation)ThoughtMaker
                                .MakeThought(curCrime.ColonistThought);
                           //Log.Message("Loop 1 Step 5");

                            break;
                        case CrimeReaction.MoodVisitorFlee:
                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                .MakeThought(curCrime.VisitorThought);
                            if (witness.CurJob is Job j && j.def != JobDefOf.FleeAndCower)
                            {
                                IntVec3 fleeLoc = CellFinderLoose.GetFleeDest(witness, new List<Thing>() { criminal }, 23f);
                                witness.jobs.TryTakeOrderedJob(new Verse.AI.Job(JobDefOf.FleeAndCower, fleeLoc));
                                if (witness.Faction != null && !witness.Faction.HostileTo(criminal.Faction))
                                {
                                    witness.Faction.SetHostileTo(criminal.Faction, true);
                                }
                            }
                            break;
                        case CrimeReaction.MoodVisitorAggro:
                            thought_MemoryObservation =
                                (Thought_MemoryObservation)ThoughtMaker
                                .MakeThought(curCrime.VisitorThought);
                            if (witness.CurJob is Job k && k.def != JobDefOf.AttackMelee)
                            {
                                witness.jobs.TryTakeOrderedJob(new Job(JobDefOf.AttackMelee, criminal));
                                if (witness.Faction != null && !witness.Faction.HostileTo(criminal.Faction))
                                {
                                    witness.Faction.SetHostileTo(criminal.Faction, true);
                                }
                            }
                            break;
                    }
                    //Log.Message("Loop 1 Step 6");

                    if (thought_MemoryObservation != null)
                    {
                    //Log.Message("Loop 1 Step 7");
                        thought_MemoryObservation.Target = criminal;
                        //Log.Message("Loop 1 Step 8");

                        witness.needs.mood.thoughts.memories.TryGainMemory(thought_MemoryObservation, null);
                        //Log.Message("Loop 1 Step 9");

                    }
                }
            }
        }

    }
}
