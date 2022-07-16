using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Random = UnityEngine.Random;
using System.Collections;
using static Vampire.VampireTracker;

namespace Vampire
{
    /// <summary>
    /// The Vampire Tracker is a WorldComponent that starts when the world is created.
    /// It fulfills several purposes...
    ///     1) It generates the first generation vampire, Caine.
    ///     2) It generates Caine's children (2nd generation)
    ///     3) When a vampire appears in a player's game, it generates a vampire lineage up to that generation.
    ///         E.g. Dracula, a 5th generation vampire is created in the player's game, so...
    ///              we create the sire of Dracula (4th gen vamp) and their sire before that (3rd gen vamp)
    ///     4) It stores Vampires as VampireData to prevent data cleaning mods from removing them.
    ///         E.g. If the character is deleted from a cleaning mod, this VampireData will regenerate
    ///              them based on saved code.
    ///     5) Allows players to make a final configuration of vampires on scenario startup
    /// </summary>
    public class WorldComponent_VampireTracker : WorldComponent
    {
        private bool finalConfiguationSet = false;
        private bool worldLoaded = false;
        private bool debugPrinted = false;
        public bool vampiresLoaded = false;
        private Pawn firstVampire;
        private List<Pawn> activeVampires = null;
        private List<Pawn> dormantVampires = null;
        private List<Pawn> tempVampires = new List<Pawn>();
        private List<Pawn> tmpSunlightPolicyVampires = new List<Pawn>();
        private List<SunlightPolicy> tmpSunlightPolicies = new List<SunlightPolicy>();
        public Dictionary<Pawn, List<RoyalTitle>> tempVampireTitles = new Dictionary<Pawn, List<RoyalTitle>>();
        public Dictionary<Pawn, int> recentVampires = new Dictionary<Pawn, int>();
        private Dictionary<VampireRecord, Pawn> worldVampires = new Dictionary<VampireRecord, Pawn>();
        public Dictionary<Pawn,SunlightPolicy> sunlightPolicies = new Dictionary<Pawn,SunlightPolicy>();
        public HashSet<Pawn> vampireList = new HashSet<Pawn>();
        
        public Pawn FirstVampire
        {
            get
            {
                if (firstVampire == null)
                {
                    firstVampire = VampireGen.GenerateVampire(1, VampDefOf.ROMV_Caine, null, null, true);
                }
                return firstVampire;
            }
        }
        public List<Pawn> HigherGenVampires
        {
            get
            {
                if (activeVampires == null)
                {
                    activeVampires = new List<Pawn>();
                }
                return activeVampires;
            }
        }
        public List<Pawn> LowerGenVampires
        {
            get
            {
                if (dormantVampires == null)
                {
                    //Log.Message("1");
                    dormantVampires = new List<Pawn>();
                    List<Pawn> generationTwo = new List<Pawn>();
                    List<Pawn> generationThree = new List<Pawn>();
                    //First Generation
                    Pawn Caine = FirstVampire;
                    //Find.WorldPawns.PassToWorld(Caine, PawnDiscardDecideMode.KeepForever);
                    //Log.Message("2");

                    //Second Generation
                    for (int i = 0; i < 3; i++)
                    {
                        Pawn secondGenVamp = VampireGen.GenerateVampire(2, VampDefOf.ROMV_TheThree, Caine);
                        generationTwo.Add(secondGenVamp);
                        //Find.WorldPawns.PassToWorld(secondGenVamp, PawnDiscardDecideMode.KeepForever);
                    }
                    //Log.Message("3");
                    //Third Generation
                    foreach (BloodlineDef clan in DefDatabase<BloodlineDef>.AllDefs.Where(x =>
                        x != VampDefOf.ROMV_Caine && x != VampDefOf.ROMV_TheThree))
                    {
                        Pawn randSecondGenVamp = generationTwo.RandomElement();
                        Pawn clanFounderVamp = VampireGen.GenerateVampire(3, clan, randSecondGenVamp);
                        generationThree.Add(clanFounderVamp);
                        //Find.WorldPawns.PassToWorld(clanFounderVamp, PawnDiscardDecideMode.KeepForever);
                    }
                    dormantVampires.Add(Caine);
                    //Log.Message("5b");

                    dormantVampires.AddRange(generationTwo);
                    dormantVampires.AddRange(generationThree);
                    //    Concat(generationFive));
                    //Log.Message("5c");
                }
                return dormantVampires;
            }
        }

        public Pawn GetVampire(string searchKey)
        {
            var record = worldVampires.Keys.FirstOrDefault(x => x.ID == searchKey);
            if (record != null)
            {
                if (worldVampires[record] != null)
                {
                    return worldVampires[record];
                }
                return worldVampires[record] = record.GenerateForWorld();
            }
            Log.Error("Failed to get vampire ID " + searchKey);
            return null;
        }
        
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            
            //Constant tick check.
            //Alert player to powerful vampire spawns
            if (recentVampires.Any())
                recentVampires.RemoveAll(x => x.Key.Dead || x.Key.DestroyedOrNull());
            if (recentVampires.Any())
            {
                var recentVampiresKeys =
                    new List<Pawn>(recentVampires.Keys.Where(x =>
                        x.Spawned && x.Faction != Faction.OfPlayerSilentFail));

                foreach (var key in recentVampiresKeys)
                {
                    recentVampires[key] += 1;
                    if (recentVampires[key] > 100)
                    {
                        recentVampires.Remove(key);
                        tempVampires.Add(key);
                        if (!key.Spawned || key.Faction == Faction.OfPlayerSilentFail) continue;
                        var generation = key?.VampComp()?.Generation;
                        if (generation != null && generation <= 8)
                            Find.LetterStack.ReceiveLetter("ROMV_PowerfulVampireLabel".Translate(),
                                "ROMV_PowerfulVampireDesc".Translate(new object[]
                                {
                                    key.LabelShort,
                                    VampireStringUtility.AddOrdinal(generation.Value),
                                }), LetterDefOf.ThreatSmall, key, null);
                    }
                }
            }
            
            //Rarer tick check
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                //First spawn check
                if (!vampiresLoaded)
                {
                    vampiresLoaded = true;
                    foreach (Map map in Find.Maps)
                    {
                        foreach (Pawn spawnedPawn in map.mapPawns.AllPawns)
                        {
                            if (spawnedPawn.IsVampire(false))
                            {
                                VampireTracker.AddVampire(spawnedPawn);
                                if (!sunlightPolicies.ContainsKey(spawnedPawn))
                                {
                                    VampireTracker.SetSunlightPolicy(spawnedPawn,SunlightPolicy.Restricted);
                                }
                            }
                        }
                    }
                }


                if (!VampireSettings.Get.settingsWindowSeen)
                {
                    VampireSettings.Get.settingsWindowSeen = true;
                    if (VampireSettings.ShouldUseSettings)
                    {
                        Find.WindowStack.Add(new Dialog_VampireSetup()
                    {
                        forcePause = true
                        
                    });
                        
                    }
                }
                
                if (!worldLoaded)
                {
                    worldLoaded = true;
                    WorldVampiresCheck();
                }
                
                
                if (tempVampires.Count > 1)
                {
                    var recentVampiresKeys = new List<Pawn>(tempVampires);
                    tempVampires.Clear();
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var pawn in recentVampiresKeys)
                    {
                        stringBuilder.AppendLine("    " + pawn.Name.ToStringShort + " (" +
                                                 VampireStringUtility.AddOrdinal(pawn.VampComp().Generation) + ")");
                    }
                    string vampList = "ROMV_VampiresArrivalDesc".Translate(stringBuilder.ToString());
                    Find.LetterStack.ReceiveLetter("ROMV_VampiresArrivalLabel".Translate(), vampList,
                        LetterDefOf.ThreatSmall, recentVampiresKeys.FirstOrDefault(), null);
                }

                //In a hidey hole? Let's check if it's time to leave.
                CleanVampGuestCache();
                if (HarmonyPatches.VampGuestCache == null || !HarmonyPatches.VampGuestCache.Any()) return;
                foreach (var keyValuePair in HarmonyPatches.VampGuestCache)
                {
                    Pawn p = keyValuePair.Key;
                    if (p == null) continue;
                    if (p.Downed) continue;
                    if (keyValuePair.Value + 16000 > Find.TickManager.TicksGame) continue;
                    if (p.CurJob?.def == JobDefOf.Goto) continue;
                    if (p.InMentalState || p.IsFighting()) continue;
                    if (p.IsSunRisingOrDaylight()) continue;
                    if (p.ParentHolder is Building_HideyHole g)
                    {
                        g.EjectContents();
                    }
                    TryGiveJobGiverToVampGuest(p);
                }
            }
        }

        private void WorldVampiresCheck()
        {
            if (worldVampires?.Count > 0)
            {
                foreach (VampireRecord key in worldVampires.Keys)
                {
                    if (worldVampires[key] == null)
                    {
                        worldVampires[key] = key.GenerateForWorld();
                        Log.Warning(key.ID + " had no worldpawn generated. Regenerated.");
                    }
                }
            }
        }

        public Pawn GetLaterGenerationVampire(Pawn childe, BloodlineDef bloodline, int idealGenerationOfChilde = -1)
        {
            if (idealGenerationOfChilde == -1)
            {
                idealGenerationOfChilde = GetNewlySpawnedVampireGeneration(childe);
            }

            if (!HigherGenVampires.NullOrEmpty() && HigherGenVampires?.FindAll(x => x.VampComp() is CompVampire v &&
                                                                              !x.Spawned && v.Bloodline == bloodline &&
                                                                              v.Generation ==
                                                                              idealGenerationOfChilde - 1) is List<Pawn>
                    vamps && !vamps.NullOrEmpty())
            {
                return vamps.RandomElement();
            }
            List<Pawn> vampsGen = TryGeneratingBloodline(childe, bloodline);
            return vampsGen.FirstOrDefault(x => x?.VampComp()?.Generation == idealGenerationOfChilde - 1);
        }
        private int GetNewlySpawnedVampireGeneration(Pawn childe)
        {
            if (childe?.VampComp()?.Generation != -1) return childe?.VampComp()?.Generation ?? Mathf.Clamp(VampireGen.RandHigherGenerationWeak, 1, 13);

            var result = -1;
            if (Rand.Value < 0.1)
            {
                result = VampireGen.RandHigherGenerationTough;

                //Log.Message("Vampires :: Spawned " + result + " generaton vampire.");
                return result;
            }
            result = Mathf.Clamp(VampireGen.RandHigherGenerationWeak, 1, 13);
            //Log.Message("Vampires :: Spawned " + result + " generaton vampire.");                
            return result;
        }

        private List<Pawn> TryGeneratingBloodline(Pawn childe, BloodlineDef bloodline)
        {
            List<Pawn> tempOldGen = new List<Pawn>(LowerGenVampires);
            if (bloodline == null) bloodline = VampireUtility.RandBloodline;
            Pawn thirdGenVamp = tempOldGen.FirstOrDefault(x =>
                x?.VampComp() is CompVampire v && v.Generation == 3 && v.Bloodline == bloodline);
            List<Pawn> futureGenerations = new List<Pawn>();
            Pawn curSire = thirdGenVamp;
            if (curSire == null)
            {
                Log.Warning("Cannot find third generation sire.");
                return null;
            }
            for (int curGen = 4; curGen < 14; curGen++)
            {
                Pawn newVamp = VampireGen.GenerateVampire(curGen, bloodline, curSire);
                futureGenerations.Add(newVamp);
                curSire = newVamp;
            }
            activeVampires.AddRange(futureGenerations);
            PrintVampires();
            return futureGenerations;
        }

        public WorldComponent_VampireTracker(World world) : base(world)
        {
        }

        private static void TryGiveJobGiverToVampGuest(Pawn p)
        {
            var thinkNode_JobGiver = (ThinkNode_JobGiver) Activator.CreateInstance(typeof(JobGiver_ExitMapBest));
            thinkNode_JobGiver.ResolveReferences();
            var thinkResult = thinkNode_JobGiver.TryIssueJobPackage(p, default(JobIssueParams));
            if (thinkResult.Job != null)
            {
                //Log.Message("Vampire Guest Handler :: " + p.LabelShort + " :: Started ExitMapBest job.");
                p.jobs.StartJob(thinkResult.Job, JobCondition.None, null, false, true, null, null, false);
            }
            else
            {
                //Log.Message("Vampire Guest Handler :: " + p.LabelShort + " :: Failed to give ExitMapBest job.");
            }
        }

        private static void CleanVampGuestCache()
        {
            HarmonyPatches.VampGuestCache.RemoveAll(
                x => x.Key is Pawn p &&
                     (p.Dead || p.Faction == Faction.OfPlayerSilentFail || p.IsPrisoner ||
                      (!p.Spawned && !(p.ParentHolder is Building_HideyHole))));
        }

        public void PrintVampires()
        {
            //int count = 0;
            //Log.Message("Dormant Vampires");
            //List<Pawn> tempDormantVampires = new List<Pawn>(DormantVampires);
            //StringBuilder s = new StringBuilder();
            //foreach (Pawn vamp in tempDormantVampires)
            //{
            //    count++;
            //    s.AppendLine(vamp.VampComp().Generation + " | " + vamp.VampComp().Bloodline.LabelCap + " | " + vamp.LabelShort);
            //}
            //Log.Message("Active Vampires");
            //List<Pawn> tempActiveVampires = new List<Pawn>(ActiveVampires);
            //foreach (Pawn vamp in tempActiveVampires)
            //{
            //    count++;
            //    s.AppendLine(vamp.VampComp().Generation + " | " + vamp.VampComp().Bloodline.LabelCap + " | " + vamp.LabelShort);
            //}
            //s.AppendLine("Total Vampires: " + count);
            //Log.Message(s.ToString());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref firstVampire, "firstVampire");
            Scribe_Collections.Look(ref dormantVampires, "dormantVampires", LookMode.Deep);
            Scribe_Collections.Look(ref activeVampires, "activeVampires", LookMode.Deep);
            Scribe_Collections.Look(ref sunlightPolicies, "sunlightPolicies", LookMode.Reference, LookMode.Value, ref tmpSunlightPolicyVampires, ref tmpSunlightPolicies);
        }

        public void AddVampire(Pawn pawn, Pawn sire, BloodlineDef bloodline, int generation, float? age)
        {
            try
            {

            //Log.Message("1");
            //Log.Message(sire.LabelShort + " Gen: " + generation);
            var sireId = (generation == 1) ? "" : sire?.VampComp()?.Generation.ToString() + "_" + sire?.VampComp()?.Bloodline.ToString() + "_" + sire?.Name?.ToStringFull + "_" + sire?.gender.ToString();
            //Log.Message("2");
            
            //Make a temporary new record of the vampire.
            var newRecord = new VampireRecord(pawn, generation, bloodline,
                age ?? new FloatRange(18f, 100f).RandomInRange, sireId, pawn.Faction);
            //Log.Message("3");
            
            //Check to make sure the record doesn't already exist.
            if (worldVampires?.Count > 0)
            {
                //Log.Message("4a");

                if (worldVampires.FirstOrDefault(x => x.Key.Equals(newRecord)) is KeyValuePair<VampireRecord, Pawn> rec &&
                    rec.Key is VampireRecord vampRec)
                {

                    worldVampires[vampRec] = pawn;
                    return;
                }   
            }
            //Log.Message("5");

            //If not, add a new record
            worldVampires?.Add(newRecord, pawn);

            //Log.Message("6");

                
            }
            catch (Exception e)
            {
                Log.Message(e.ToString());
            }
                
            //Check all other vampire records for issues
            WorldVampiresCheck();
            
            //Log.Message("7");

        }
    }
}