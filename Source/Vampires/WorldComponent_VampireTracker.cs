using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace Vampire
{
    public class WorldComponent_VampireTracker : WorldComponent
    {
        public Pawn firstVampire;
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

        public Pawn GetLaterGenerationVampire(Pawn childe, BloodlineDef bloodline, int idealGenerationOfChilde = -1)
        {
            if (idealGenerationOfChilde == -1)
            {
                idealGenerationOfChilde = (childe?.VampComp()?.Generation == -1)? Rand.Range(10,13) : childe?.VampComp()?.Generation ?? Rand.Range(10, 13);
            }

            if (!ActiveVampires.NullOrEmpty() && ActiveVampires?.FindAll(x => x.VampComp() is CompVampire v &&
            !x.Spawned && v.Bloodline == bloodline && v.Generation == idealGenerationOfChilde - 1) is List<Pawn> vamps && !vamps.NullOrEmpty())
            {
                return vamps.RandomElement();
            }
            List<Pawn> vampsGen = TryGeneratingBloodline(childe, bloodline);
            return vampsGen.FirstOrDefault(x => x?.VampComp()?.Generation == idealGenerationOfChilde - 1);
        }

        public List<Pawn> TryGeneratingBloodline(Pawn childe, BloodlineDef bloodline)
        {
            List<Pawn> tempOldGen = new List<Pawn>(DormantVampires);
            if (bloodline == null) bloodline = VampireUtility.RandBloodline;
            Pawn thirdGenVamp = tempOldGen.FirstOrDefault(x => x?.VampComp() is CompVampire v && v.Generation == 3 && v.Bloodline == bloodline);
            List<Pawn> futureGenerations = new List<Pawn>();
            Pawn curSire = thirdGenVamp;
            if (curSire == null)
            {
                Log.Error("Cannot find third generation sire.");
                return null;
            }
            for (int curGen = 4; curGen < 14; curGen++)
            {
                Pawn newVamp = VampireGen.GenerateVampire(curGen, bloodline, curSire, null, false);
                futureGenerations.Add(newVamp);
                curSire = newVamp;
            }
            activeVampires.AddRange(futureGenerations);
            PrintVampires();
            return futureGenerations;
        }

        public List<Pawn> activeVampires = null;
        public List<Pawn> ActiveVampires
        {
            get
            {
                if (activeVampires == null)
                {
                    activeVampires = new List<Pawn>();

                    //List<Pawn> tempOldGenerations = new List<Pawn>(DormantVampires);
                    //List<Pawn> fifthGeneration = tempOldGenerations.FindAll(x => x.VampComp().Generation == 5);
                    //List<Pawn> futureGenerations = new List<Pawn>();

                    //foreach (Pawn fifthGenVamp in fifthGeneration)
                    //{
                    //    for (int i = 0; i < Rand.Range(2, 3); i++)
                    //    {
                    //        Pawn curSire = fifthGenVamp;
                    //        for (int curGen = 6; curGen < 14; curGen++)
                    //        {
                    //            Pawn newVamp = VampireUtility.GenerateVampire(curGen, curSire.VampComp().Bloodline, curSire, false);
                    //            futureGenerations.Add(newVamp);
                    //            curSire = newVamp;
                    //        }
                    //    }
                    //}
                    //activeVampires.AddRange((fifthGeneration).Concat(futureGenerations));
                }
                return activeVampires;
            }
        }

        public List<Pawn> dormantVampires = null;
        public List<Pawn> DormantVampires
        {
            get
            {
                if (dormantVampires == null)
                {
                    //Log.Message("1");
                    dormantVampires = new List<Pawn>();
                    List<Pawn> generationTwo = new List<Pawn>();
                    List<Pawn> generationThree = new List<Pawn>();
                    List<Pawn> generationFour = new List<Pawn>();
                    List<Pawn> generationFive = new List<Pawn>();
                    //First Generation
                    Pawn Caine = FirstVampire;
                    //Find.WorldPawns.PassToWorld(Caine, PawnDiscardDecideMode.KeepForever);
                    //Log.Message("2");

                    //Second Generation
                    for (int i = 0; i < 3; i++)
                    {
                        Pawn secondGenVamp = VampireGen.GenerateVampire(2, VampDefOf.ROMV_TheThree, Caine, null, false);
                        generationTwo.Add(secondGenVamp);
                        //Find.WorldPawns.PassToWorld(secondGenVamp, PawnDiscardDecideMode.KeepForever);
                    }
                    //Log.Message("3");
                    //Third Generation
                    foreach (BloodlineDef clan in DefDatabase<BloodlineDef>.AllDefs.Where(x => x != VampDefOf.ROMV_Caine && x != VampDefOf.ROMV_TheThree))
                    {
                        Pawn randSecondGenVamp = generationTwo.RandomElement();
                        Pawn clanFounderVamp = VampireGen.GenerateVampire(3, clan, randSecondGenVamp, null, false);
                        generationThree.Add(clanFounderVamp);
                        //Find.WorldPawns.PassToWorld(clanFounderVamp, PawnDiscardDecideMode.KeepForever);
                    }
                    //Log.Message("4");
                    ////Fourth Generation
                    //foreach (Pawn genThreeVamp in generationThree)
                    //{
                    //    for (int i = 0; i < Rand.Range(2,3); i++)
                    //    {
                    //        Pawn genFourVamp = VampireUtility.GenerateVampire(4, genThreeVamp.VampComp().Bloodline, genThreeVamp, false);
                    //        generationFour.Add(genFourVamp);
                    //    }
                    //}
                    //Log.Message("5");
                    ////Fifth Generation
                    //foreach (Pawn genFourVamp in generationFour)
                    //{
                    //    for (int i = 0; i < Rand.Range(2,3); i++)
                    //    {
                    //        Pawn genFiveVamp = VampireUtility.GenerateVampire(5, genFourVamp.VampComp().Bloodline, genFourVamp, false);
                    //        generationFive.Add(genFiveVamp);
                    //    }
                    //}
                    //Log.Message("5a");

                    dormantVampires.Add(Caine);
                    //Log.Message("5b");

                    dormantVampires.AddRange(generationTwo);
                    dormantVampires.AddRange(generationThree);
                    dormantVampires.AddRange(generationFour);//.
                    //    Concat(generationFive));
                    //Log.Message("5c");

                }
                return dormantVampires;
            }
        }

        public WorldComponent_VampireTracker(World world) : base(world)
        {

        }

        private bool debugPrinted = false;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            //if (debugPrinted == false)
            //{
            //    debugPrinted = true;
            //    PrintVampires();
            //}
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
            Scribe_References.Look<Pawn>(ref this.firstVampire, "firstVampire");
            Scribe_Collections.Look<Pawn>(ref this.dormantVampires, "dormantVampires", LookMode.Deep);
            Scribe_Collections.Look<Pawn>(ref this.activeVampires, "activeVampires", LookMode.Deep);
        }
    }
}
