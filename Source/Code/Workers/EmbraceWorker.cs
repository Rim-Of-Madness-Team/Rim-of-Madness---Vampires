using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public class EmbraceWorker
{
    public BloodlineDef def;

    public virtual bool CanEmbrace(CompVampire sire, CompVampire childe)
    {
        if (sire == null || childe == null) return false;
        if (childe.AbilityUser.RaceProps.Humanlike &&
            !childe.IsVampire &&
            childe.Sire == null &&
            sire.IsVampire)
            return true;
        return false;
    }

    public void RemoveColonistDiedThoughts(Pawn vampToBe, bool wasColonist)
    {
        //Log.Message("RemoveColonistDiedThoughts Called");
        if (vampToBe.MapHeld.mapPawns.FreeColonistsAndPrisonersSpawned.ToList() is { } thoughtHavers &&
            !thoughtHavers.NullOrEmpty())
            foreach (var thoughtHaver in thoughtHavers)
            {
                var memories = thoughtHaver.needs.mood.thoughts.memories;
                //Log.Message("Memory Cleaner");
                if (memories.Memories.FindAll(x =>
                            (x.otherPawn == vampToBe && x.def.defName.Contains("Died")) ||
                            x.def.defName.Contains("Death"))
                        is { } mems && !mems.NullOrEmpty())
                    foreach (var mem in mems)
                        memories.RemoveMemory(mem);
                    //Log.Message("Memory removed");
            }
    }

    public virtual void DoEffect(CompVampire sireComp, CompVampire childeComp)
    {
        if (childeComp.AbilityUser is { } p)
        {
            var vampToBe = p;
            if (childeComp?.BloodPool is { } bloodOfVampToBe &&
                sireComp?.BloodPool is { } bloodOfVampMaster)
                bloodOfVampToBe.TransferBloodTo(9999, bloodOfVampMaster);
            var wasColonist = p.Faction == Faction.OfPlayer;
            ResurrectionUtility.Resurrect(vampToBe);
            if (vampToBe.Faction != sireComp?.AbilityUser?.Faction)
                vampToBe.SetFaction(sireComp.AbilityUser.Faction, sireComp.AbilityUser);

            RemoveColonistDiedThoughts(vampToBe, wasColonist);

            if (sireComp?.BloodPool is { } sB && vampToBe?.BloodNeed() is { } cB)
            {
                cB.CurBloodPoints = 0;
                sB.TransferBloodTo(1, cB);
                cB.CurBloodPoints = 1;
            }

            vampToBe.VampComp().Notify_Embraced(sireComp);
        }
    }

    public virtual bool TryEmbrace(Pawn sire, Pawn childe)
    {
        if (CanEmbrace(sire.GetComp<CompVampire>(), childe.GetComp<CompVampire>()))
        {
            DoEffect(sire.GetComp<CompVampire>(), childe.GetComp<CompVampire>());
            return true;
        }

        return false;
    }
}