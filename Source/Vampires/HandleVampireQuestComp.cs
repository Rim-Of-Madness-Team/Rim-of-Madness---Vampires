using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire
{
	public class HandleVampireQuestComp : WorldObjectComp, IThingHolder
	{
		//public HandleVampireQuestComp()
		//{
		//	this.rewards = new ThingOwner<Thing>(this);
		//}

		public bool Active => this.active;

		public void StartQuest()
		{
			this.StopQuest();
			this.active = true;
		}

		public void StopQuest()
		{
			this.active = false;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (this.active)
			{
				MapParent mapParent = this.parent as MapParent;
				if (mapParent != null)
				{
					this.CheckAllEnemiesDefeated(mapParent);
				}
			}
		}

		private void CheckAllEnemiesDefeated(MapParent mapParent)
		{
			if (inductedWithVampirism)
			{
				this.GiveRewardsAndSendLetter();
				this.StopQuest();
			}
			if (!mapParent.HasMap)
			{
				return;
			}
			if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map))
			{
				return;
			}
			this.GiveRewardsAndSendLetter();
			this.StopQuest();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref this.active, "active", false, false);
			//Scribe_Values.Look<float>(ref this.relationsImprovement, "relationsImprovement", 0f, false);
			//Scribe_References.Look<Faction>(ref this.requestingFaction, "requestingFaction", false);
			//Scribe_Deep.Look<ThingOwner>(ref this.rewards, "rewards", new object[]
			//{
			//	this
			//});
		}

		private void GiveRewardsAndSendLetter()
		{
			Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;
			//HandleVampireQuestComp.tmpRewards.AddRange(this.rewards);
			//this.rewards.Clear();
			IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
			//DropPodUtility.DropThingsNear(intVec, map, HandleVampireQuestComp.tmpRewards, 110, false, false, true, false);
			//HandleVampireQuestComp.tmpRewards.Clear();
			Find.LetterStack.ReceiveLetter("LetterLabelHandleVampireQuestCompleted".Translate(), "LetterHandleVampireQuestCompleted".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(intVec, map, false), null);
		}
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null; //this.rewards;
		}
/*

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
		}*/

		public override string CompInspectStringExtra()
		{
			return this.active ? "ROMV_QuestHandleVampire".Translate() : null;
		}

		public bool InductedWithVampirism
		{
			get => inductedWithVampirism;
			set => inductedWithVampirism = value;
		}

		private bool inductedWithVampirism;
		
		private bool active;

		//public Faction requestingFaction;

		//public float relationsImprovement;
	}
}
