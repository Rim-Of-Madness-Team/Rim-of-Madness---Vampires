using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Vampire;

public class GenStep_VampireLair : GenStep
{
    private const int Size = 16;

    private static readonly List<CellRect> possibleRects = new();

    public override int SeedPart => 778579656;

    public override void Generate(Map map, GenStepParams parms)
    {
        CellRect rectToDefend;
        if (!MapGenerator.TryGetVar("RectOfInterest", out rectToDefend)) rectToDefend = CellRect.SingleCell(map.Center);
        Faction faction;
        if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named(
                "ROMV_Sabbat")); //Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
        else
            faction = map.ParentFaction;
        var resolveParams = default(ResolveParams);
        resolveParams.rect = GetOutpostRect(rectToDefend, map);
        resolveParams.faction = faction;
        resolveParams.edgeDefenseWidth = 2;
        //resolveParams.edgeDefenseTurretsCount = new int?(Rand.RangeInclusive(0, 1));
        resolveParams.edgeDefenseMortarsCount = 0;
        resolveParams.settlementPawnGroupPoints = 0.4f;
        BaseGen.globalSettings.map = map;
        BaseGen.globalSettings.minBuildings = 1;
        BaseGen.globalSettings.minBarracks = 1;
        BaseGen.symbolStack.Push("factionBase", resolveParams);
        BaseGen.Generate();
    }

    private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
    {
        possibleRects.Add(new CellRect(rectToDefend.minX - 1 - 16, rectToDefend.CenterCell.z - 8, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - 8, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.minZ - 1 - 16, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8, rectToDefend.maxZ + 1, 16, 16));
        var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        if (possibleRects.Any()) return possibleRects.RandomElement();
        return rectToDefend;
    }
}