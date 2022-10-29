using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace Vampire;

public class GenStep_AncientVampireCrypt : GenStep
{
    private const int Size = 16;

    private static readonly List<CellRect> possibleRects = new();

    public override int SeedPart => 538784729;

    public static CellRect TopHalf(CellRect rect)
    {
        return new CellRect(rect.minX, rect.minZ, rect.Width, (int)(rect.Height / 2f));
    }


    public static CellRect BottomHalf(CellRect rect)
    {
        return new CellRect(rect.minX, rect.minZ + (int)(rect.Height / 2f), rect.Width, (int)(rect.Height / 2f));
    }


    public static void MakeTomb(ResolveParams rp)
    {
        var
            thingDef = ThingDefOf
                .BlocksGranite; //rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, false);
        var
            floorDef = TerrainDefOf
                .PavedTile; //rp.floorDef ?? BaseGenUtility.CorrespondingTerrainDef(thingDef, true);
        if (rp.noRoof == null || !rp.noRoof.Value) BaseGen.symbolStack.Push("roof", rp);
        var resolveParams = rp;
        resolveParams.wallStuff = thingDef;
        BaseGen.symbolStack.Push("edgeWalls", resolveParams);

        //Wall pillars
        var pillarParams = rp;
        pillarParams.rect = rp.rect.ContractedBy(3);
        pillarParams.wallStuff = thingDef;
        pillarParams.chanceToSkipWallBlock = 0.7f;
        BaseGen.symbolStack.Push("edgeWalls", pillarParams);

        //Place the sealed tomb
        var throneArea = rp;
        throneArea.rect = CellRect.SingleCell(rp.rect.CenterCell);
        throneArea.singleThingDef = ThingDef.Named("ROMV_SealedCoffin");
        throneArea.singleThingStuff = ThingDefOf.BlocksGranite;
        BaseGen.symbolStack.Push("thing", throneArea);

        var resolveParams2 = rp;
        resolveParams2.floorDef = floorDef;
        BaseGen.symbolStack.Push("floor", resolveParams2);

        var outerArea = rp;
        outerArea.rect = rp.rect.ExpandedBy(2);
        outerArea.wallStuff = thingDef;
        outerArea.chanceToSkipWallBlock = 0.8f;
        BaseGen.symbolStack.Push("edgeWalls", outerArea);

        BaseGen.symbolStack.Push("clear", outerArea);
        if (rp.addRoomCenterToRootsToUnfog != null && rp.addRoomCenterToRootsToUnfog.Value &&
            Current.ProgramState == ProgramState.MapInitializing)
            MapGenerator.rootsToUnfog.Add(rp.rect.CenterCell);
    }

    public static Def GetDefSilentFail(Type type, string targetDefName)
    {
        if (type == typeof(SoundDef)) return SoundDef.Named(targetDefName);
        return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, "GetNamedSilentFail",
            targetDefName);
    }

    public static void FillWithGraves(ResolveParams rp)
    {
        var map = BaseGen.globalSettings.map;

        var cemetaryFences = rp;
        cemetaryFences.wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(TechLevel.Medieval);
        cemetaryFences.chanceToSkipWallBlock = 0.125f;
        cemetaryFences.rect = rp.rect.ExpandedBy(1);
        BaseGen.symbolStack.Push("edgeWalls", cemetaryFences);


        var @bool = Rand.Bool;
        var thingDef = ThingDefOf.Grave;
        ThingDef thingDef2;
        if ((thingDef2 = rp.singleThingStuff) == null)
            thingDef2 = GenStuff.RandomStuffByCommonalityFor(thingDef,
                rp.faction == null ? TechLevel.Undefined : rp.faction.def.techLevel);
        var singleThingStuff = thingDef2;
        foreach (var intVec in rp.rect)
        {
            if (@bool)
            {
                if (intVec.x % 3 != 0 || intVec.z % 2 != 0) continue;
            }
            else if (intVec.x % 2 != 0 || intVec.z % 3 != 0)
            {
                continue;
            }

            var rot = !@bool ? Rot4.North : Rot4.West;
            if (!GenSpawn.WouldWipeAnythingWith(intVec, rot, thingDef, map,
                    x => x.def.category == ThingCategory.Building))
                if (!BaseGenUtility.AnyDoorAdjacentCardinalTo(GenAdj.OccupiedRect(intVec, rot, thingDef.Size), map))
                    if (Rand.Value < 0.3f)
                    {
                        var resolveParams = rp;
                        resolveParams.rect = GenAdj.OccupiedRect(intVec, rot, thingDef.size);
                        resolveParams.singleThingDef = thingDef;
                        resolveParams.singleThingStuff = singleThingStuff;
                        resolveParams.thingRot = rot;

                        var singleThingDef = rp.singleThingDef ?? ThingDefOf.Grave;
                        var graveParams = rp;
                        graveParams.singleThingDef = singleThingDef;
                        var skipSingleThingIfHasToWipeBuildingOrDoesntFit =
                            rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
                        graveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit =
                            skipSingleThingIfHasToWipeBuildingOrDoesntFit == null ||
                            skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value;
                        graveParams.postThingSpawn = delegate(Thing x)
                        {
                            if (x is Building_Grave g)
                            {
                                var pawnKindDef = DefDatabase<PawnKindDef>.AllDefs.Where(y =>
                                        y?.defaultFactionType?.isPlayer == false && y?.race?.defName == "Human")
                                    .RandomElement();
                                var factionDef = DefDatabase<FactionDef>.AllDefs
                                    .Where(z => z.humanlikeFaction && !z.isPlayer).RandomElement();
                                var faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                                if (Rand.Value < 0.8f)
                                {
                                    var pawnToFillWith = PawnGenerator.GeneratePawn(pawnKindDef, faction);
                                    if (pawnToFillWith.IsVampire(true))
                                    {
                                        pawnToFillWith.Destroy();
                                        return;
                                    }

                                    pawnToFillWith.Kill(null);
                                    if (pawnToFillWith.Corpse is { } c)
                                    {
                                        c.SetForbidden(true);
                                        g.TryGetInnerInteractableThingOwner().TryAdd(c);
                                    }
                                }
                            }
                        };
                        BaseGen.symbolStack.Push("thing", graveParams);
                    }
        }


        var resolveParams2 = rp;
        resolveParams2.floorDef = TerrainDef.Named("SoilRich");
        BaseGen.symbolStack.Push("floor", resolveParams2);
    }

    public override void Generate(Map map, GenStepParams parms)
    {
        CellRect rectToDefend;
        if (!MapGenerator.TryGetVar("RectOfInterest", out rectToDefend)) rectToDefend = CellRect.SingleCell(map.Center);
        var faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROMV_LegendaryVampires"));
        var rp = default(ResolveParams);
        rp.rect = GetOutpostRect(rectToDefend, map);
        rp.rect = rp.rect.ExpandedBy(30);
        Log.Message(rp.rect.minX + " " + rp.rect.minZ);
        rp.faction = faction;
        rp.floorDef = TerrainDefOf.FlagstoneSandstone;
        rp.wallStuff = ThingDefOf.BlocksGranite;
        rp.pathwayFloorDef = TerrainDef.Named("SilverTile");
        rp.edgeDefenseWidth = 2;
        rp.edgeDefenseTurretsCount = 0; // new int?(Rand.RangeInclusive(0, 1));
        rp.edgeDefenseMortarsCount = 0;
        rp.settlementPawnGroupPoints = 0.4f;
        BaseGen.globalSettings.map = map;
//			BaseGen.globalSettings.minBuildings = 1;
//			BaseGen.globalSettings.minBarracks = 1;
//			BaseGen.symbolStack.Push("factionBase", resolveParams);
        var singlePawnLord = rp.singlePawnLord ??
                             LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(rp.rect.CenterCell), map);


        BaseGen.symbolStack.Push("outdoorLighting", rp);
        BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);

        //Make tomb
        var rpTop = rp;
        var tombWidth = rp.rect.Width / 2;
        rpTop.rect =
            TopHalf(new CellRect(rp.rect.minX + rp.rect.minX / 2 + tombWidth / 2, rp.rect.minZ / 2, tombWidth,
                rp.rect.Height / 2));
        MakeTomb(rpTop);

        //Make a road
        var roadParams = rp;
        roadParams.floorDef = TerrainDef.Named("FlagstoneGranite");
        var roadWidth = Rand.Range(2, 4); //rp.rect.Width / 8;
        roadParams.rect = new CellRect(rp.rect.minX + rp.rect.minX / 2 + tombWidth / 2, rp.rect.minZ / 2, roadWidth,
            rp.rect.Height);
        BaseGen.symbolStack.Push("floor", roadParams);

        //Make graveyard
        var rpBottom = rp;
        rpBottom.rect = BottomHalf(rp.rect);
        FillWithGraves(rpBottom);


        BaseGen.symbolStack.Push("clear", rp);
        if (rp.addRoomCenterToRootsToUnfog != null && rp.addRoomCenterToRootsToUnfog.Value &&
            Current.ProgramState == ProgramState.MapInitializing)
            MapGenerator.rootsToUnfog.Add(rp.rect.CenterCell);

        BaseGen.Generate();
    }

    private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
    {
        possibleRects.Add(new CellRect(rectToDefend.minX - 1 - 16,
            rectToDefend.CenterCell.z - 8, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.maxX + 1,
            rectToDefend.CenterCell.z - 8, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8,
            rectToDefend.minZ - 1 - 16, 16, 16));
        possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - 8,
            rectToDefend.maxZ + 1, 16, 16));
        var mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
        possibleRects.RemoveAll(x => !x.FullyContainedWithin(mapRect));
        if (possibleRects.Any()) return possibleRects.RandomElement();
        return rectToDefend;
    }
}