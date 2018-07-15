using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Vampire
{
	public class GenStep_VampireLair : GenStep
	{
		public override void Generate(Map map, GenStepParams parms)
        {
			CellRect rectToDefend;
			if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out rectToDefend))
			{
				rectToDefend = CellRect.SingleCell(map.Center);
			}
			Faction faction;
			if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
			{
				faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named(
					"ROMV_Sabbat")); //Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			}
			else
			{
				faction = map.ParentFaction;
			}
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = this.GetOutpostRect(rectToDefend, map);
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = new int?(2);
			//resolveParams.edgeDefenseTurretsCount = new int?(Rand.RangeInclusive(0, 1));
			resolveParams.edgeDefenseMortarsCount = new int?(0);
			resolveParams.settlementPawnGroupPoints = new float?(0.4f);
			BaseGen.globalSettings.map = map;
			BaseGen.globalSettings.minBuildings = 1;
			BaseGen.globalSettings.minBarracks = 1;
			BaseGen.symbolStack.Push("factionBase", resolveParams);
			BaseGen.Generate();
		}

		private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
		{
			GenStep_VampireLair.possibleRects.Add(new CellRect(rectToDefend.minX - 1 - 16, rectToDefend.CenterCell.z - 8, 16, 16));
			GenStep_VampireLair.possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - 8, 16, 16));
			GenStep_VampireLair.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.minZ - 1 - 16, 16, 16));
			GenStep_VampireLair.possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.maxZ + 1, 16, 16));
			CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
			GenStep_VampireLair.possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (GenStep_VampireLair.possibleRects.Any<CellRect>())
			{
				return GenStep_VampireLair.possibleRects.RandomElement<CellRect>();
			}
			return rectToDefend;
		}

		private const int Size = 16;

		private static List<CellRect> possibleRects = new List<CellRect>();

        public override int SeedPart => throw new System.NotImplementedException();
    }
}