using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public static class VampireRelationUtility
    {

        public static Pawn FindSireFor(Pawn pawn, BloodlineDef bloodline = null, int idealGeneration = -1)
        {
            Pawn result = null;
            result = Find.World.GetComponent<WorldComponent_VampireTracker>().GetLaterGenerationVampire(pawn, bloodline, idealGeneration);
            return result;
        }

        public static void SetSireChildeRelations(Pawn thisChilde, CompVampire sireComp, int optionalGeneration = -1)
        {
            //Log.Message("2" + thisChilde.LabelShort + " " + optionalGeneration.ToString());

            if (sireComp == null)
            {
                if (thisChilde.relations.GetFirstDirectRelationPawn(VampDefOf.ROMV_Sire) is Pawn discoveredSire)
                {
                    thisChilde.VampComp().Sire = discoveredSire;
                    if (sireComp?.Childer is List<Pawn> childer && !childer.NullOrEmpty() && !childer.Contains(thisChilde))
                    {
                        childer.Add(thisChilde);
                    }
                    return;
                }
                else
                {
                    //Log.Warning("Vampires must have a sire. Choosing one.");
                    BloodlineDef bloodlineToApply = thisChilde.VampComp().Bloodline;
                    if (bloodlineToApply == null)
                    {
                        Log.Warning("Vampires must have a blood line. Choosing one.");
                        bloodlineToApply = VampireUtility.RandBloodline;
                        thisChilde.VampComp().Bloodline = bloodlineToApply;
                    }
                    Pawn sire = FindSireFor(thisChilde, bloodlineToApply, optionalGeneration); //Find.World.GetComponent<WorldComponent_VampireTracker>().GetLaterGenerationVampire(thisChilde, bloodlineToApply, optionalGeneration);
                    sireComp = sire.VampComp();
                    thisChilde.VampComp().Sire = sire;
                }
            }
            CompVampire childeComp = thisChilde.GetComp<CompVampire>();
            sireComp.AbilityUser.relations.AddDirectRelation(VampDefOf.ROMV_Childe, thisChilde);
            thisChilde.relations.AddDirectRelation(VampDefOf.ROMV_Sire, sireComp.AbilityUser);
            childeComp.Sire = sireComp.AbilityUser;
            sireComp.Childer.Add(thisChilde);
        }

        // RimWorld.ParentRelationUtility
        public static void SetSire(this Pawn pawn, Pawn newSire)
        {
            Pawn sire = pawn.GetSire();
            if (sire != newSire)
            {
                if (sire != null)
                    pawn.relations.RemoveDirectRelation(VampDefOf.ROMV_Sire, sire);
                if (newSire != null)
                    pawn.relations.AddDirectRelation(VampDefOf.ROMV_Sire, newSire);
            }
        }

        public static Pawn GetSire(this Pawn pawn)
        {
            if (!pawn.RaceProps.IsFlesh)
            {
                return null;
            }
            return pawn.relations.GetFirstDirectRelationPawn(VampDefOf.ROMV_Sire, (Pawn x) => x != null);
        }


        //public static Pawn GetRegnant(this Pawn pawn)
        //{
        //    if (!pawn.RaceProps.IsFlesh)
        //    {
        //        return null;
        //    }
        //    return pawn.relations.GetFirstDirectRelationPawn(VampDefOf.ROMV_Sire, (Pawn x) => x != null);
        //}


        //// RimWorld.ParentRelationUtility
        //public static List<Pawn> GetGhouls(this Pawn pawn)
        //{
        //    if (!pawn.RaceProps.IsFlesh)
        //    {
        //        return null;
        //    }
        //    List<Pawn> result = new List<Pawn>();
        //    for (int i = 0; i < pawn.relations.DirectRelations.Count; i++)
        //    {
        //        DirectPawnRelation directPawnRelation = pawn.relations.DirectRelations[i];
        //        if (directPawnRelation.def == VampDefOf.ROMV_Childe)
        //        {
        //            result.Add(directPawnRelation.otherPawn);
        //        }
        //    }
        //    return result;
        //}


        // RimWorld.ParentRelationUtility
        public static List<Pawn> GetChilder(this Pawn pawn)
        {
            if (!pawn.RaceProps.IsFlesh)
            {
                return null;
            }
            List<Pawn> result = new List<Pawn>();
            for (int i = 0; i < pawn.relations.DirectRelations.Count; i++)
            {
                DirectPawnRelation directPawnRelation = pawn.relations.DirectRelations[i];
                if (directPawnRelation.def == VampDefOf.ROMV_Childe)
                {
                    result.Add(directPawnRelation.otherPawn);
                }
            }
            return result;
        }

        public static bool IsChildeOf(this Pawn pawnToCheck, Pawn sire)
        {
            return sire?.GetChilder()?.Contains(pawnToCheck) ?? false;
        }
    }
}
