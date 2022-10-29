using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire;

public class HandleVampireQuestComp : WorldObjectComp, IThingHolder
{
    private bool active;

    //public HandleVampireQuestComp()
    //{
    //	this.rewards = new ThingOwner<Thing>(this);
    //}

    public bool Active => active;

    public bool InductedWithVampirism { get; set; }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return null; //this.rewards;
    }

    public void StartQuest()
    {
        StopQuest();
        active = true;
    }

    public void StopQuest()
    {
        active = false;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (active)
        {
            var mapParent = parent as MapParent;
            if (mapParent != null) CheckAllEnemiesDefeated(mapParent);
        }
    }

    private void CheckAllEnemiesDefeated(MapParent mapParent)
    {
        if (InductedWithVampirism)
        {
            GiveRewardsAndSendLetter();
            StopQuest();
        }

        if (!mapParent.HasMap) return;
        if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map)) return;
        GiveRewardsAndSendLetter();
        StopQuest();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref active, "active");
        //Scribe_Values.Look<float>(ref this.relationsImprovement, "relationsImprovement", 0f, false);
        //Scribe_References.Look<Faction>(ref this.requestingFaction, "requestingFaction", false);
        //Scribe_Deep.Look<ThingOwner>(ref this.rewards, "rewards", new object[]
        //{
        //	this
        //});
    }

    private void GiveRewardsAndSendLetter()
    {
        var map = Find.AnyPlayerHomeMap ?? ((MapParent)parent).Map;
        //HandleVampireQuestComp.tmpRewards.AddRange(this.rewards);
        //this.rewards.Clear();
        var intVec = DropCellFinder.TradeDropSpot(map);
        //DropPodUtility.DropThingsNear(intVec, map, HandleVampireQuestComp.tmpRewards, 110, false, false, true, false);
        //HandleVampireQuestComp.tmpRewards.Clear();
        Find.LetterStack.ReceiveLetter("LetterLabelHandleVampireQuestCompleted".Translate(),
            "LetterHandleVampireQuestCompleted".Translate(), LetterDefOf.PositiveEvent,
            new GlobalTargetInfo(intVec, map));
    }
/*

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
		}*/

    public override string CompInspectStringExtra()
    {
        return active ? "ROMV_QuestHandleVampire".Translate() : null;
    }

    //public Faction requestingFaction;

    //public float relationsImprovement;
}