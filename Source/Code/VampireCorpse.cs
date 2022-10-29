using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbilityUser;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class VampireCorpse : Corpse
{
    private const int VanishAfterTicksSinceDessicated = 6000000;


    private Graphic ashesCache;


    ///VAMPIRE CORPSE ITEMS ////////////////////////////////////////////////////////////////////////////////////////////////////
    private int bloodPoints = -1;

    private bool burnedToAshes;

    private int corpseBurnCount;

    private bool diableried;

    private ThingOwner<Pawn> innerContainer;

    private BillStack operationsBillStack;

    private int timeOfDeath = -1000;

    private int vanishAfterTimestamp = -1000;

    public VampireCorpse()
    {
        operationsBillStack = new BillStack(this);
        innerContainer = new ThingOwner<Pawn>(this, true, LookMode.Reference);
    }

    private bool ShouldVanish => InnerPawn.RaceProps.Animal && vanishAfterTimestamp > 0 &&
                                 Age >= vanishAfterTimestamp && Spawned && this.GetRoom() != null &&
                                 this.GetRoom().TouchesMapEdge && !Map.roofGrid.Roofed(Position);

    public int BloodPoints
    {
        get => bloodPoints;
        set => bloodPoints = value;
    }

    public bool BurnedToAshes
    {
        get => burnedToAshes;
        set
        {
            //if (value == true)
            //{
            //    if (InnerPawn is Pawn p)
            //    {
            //        List<BodyPartRecord> parts = p.health.hediffSet.GetNotMissingParts().ToList();
            //        foreach (BodyPartRecord rec in parts)
            //        {
            //            if (p.health.hediffSet.PartIsMissing(rec))
            //            {
            //                continue;
            //            }
            //            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(DamageDefOf.Burn, p, rec);
            //            Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, p);
            //            hediff_Injury.Part = rec;
            //            hediff_Injury.source = null;
            //            hediff_Injury.sourceBodyPartGroup = null;
            //            hediff_Injury.sourceHediffDef = null;
            //            hediff_Injury.Severity = 999999;
            //            p.health.AddHediff(hediff_Injury, null, new DamageInfo?(new DamageInfo(DamageDefOf.Burn, 999999, 1f, -1, null, rec)));
            //        }
            //    }
            //}
            burnedToAshes = value;
            if (value)
            {
                if (DefDatabase<HistoryEventDef>.GetNamedSilentFail("ROMV_VampireDied") is { } def)
                    Find.HistoryEventsManager.RecordEvent(
                        new HistoryEvent(
                            def, InnerPawn.Named(HistoryEventArgsNames.Doer)
                        ));

                Find.LetterStack.ReceiveLetter("ROMV_FinalDeath".Translate(),
                    "ROMV_FinalDeathDesc".Translate(InnerPawn.Label), VampDefOf.ROMV_BadMessage, this);
            }

            if (value && InnerPawn?.royalty is { } pRoyalty && !Diableried)
            {
                HarmonyPatches.keepTitles = false;
                pRoyalty.Notify_PawnKilled();
                HarmonyPatches.keepTitles = true;
            }
        }
    }

    public bool Diableried
    {
        get => diableried;
        set => diableried = value;
    }

    public Graphic Ashes
    {
        get
        {
            if (ashesCache == null)
            {
                var temp = new GraphicData();
                temp.texPath = "Things/Item/Resource/VampireAshes";
                temp.graphicClass = typeof(Graphic_Single);
                ashesCache = temp.Graphic;
            }

            return ashesCache;
        }
    }

    public override string Label
    {
        get
        {
            if (Diableried)
                return "ROMV_SoullessHuskOf".Translate(base.Label);
            return burnedToAshes ? "ROMV_AshesOf".Translate(base.Label) : new TaggedString(base.Label);
        }
    }

    public bool CanResurrect => InnerPawn != null && !BurnedToAshes &&
                                InnerPawn.Faction == Faction.OfPlayerSilentFail && !Diableried &&
                                this.GetRotStage() < RotStage.Dessicated;

    private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
    {
        var source = from x in InnerPawn.health.hediffSet.GetNotMissingParts()
            where x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this, x) > 0.001f
            select x;
        if (!source.Any()) return null;
        return source.MinBy(x => Mathf.Abs(FoodUtility.GetBodyPartNutrition(this, x) - nutritionWanted));
    }

    private void NotifyColonistBar()
    {
        if (InnerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
            Find.ColonistBar.MarkColonistsDirty();
    }

    public override string GetInspectString()
    {
        var s = new StringBuilder();

        if (!burnedToAshes)
        {
            if (base.GetInspectString() is { } baseString && baseString != "")
                s.AppendLine(baseString);
            s.AppendLine("Blood points remaining: " + bloodPoints);
        }
        else
        {
            if (InnerPawn.Faction != null) s.AppendLine("Faction".Translate() + ": " + InnerPawn.Faction.Name);
            s.AppendLine("DeadTime".Translate(new object[]
            {
                Age.ToStringTicksToPeriod()
            }));
        }

        return s.ToString().TrimEndNewlines();
    }


    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        var building = this.StoringThing();
        if (building != null && building.def == ThingDefOf.Grave) return;
        if (!burnedToAshes)
            InnerPawn.Drawer.renderer.RenderPawnAt(drawLoc);
        else
            Ashes.Draw(drawLoc, Rot4.North, this);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos())
            yield return g;

        var bloodResurrection = DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricResurrection");
        if (CanResurrect)
            yield return new Command_Action
            {
                defaultLabel = bloodResurrection.label,
                defaultDesc = bloodResurrection.GetDescription(),
                icon = bloodResurrection.uiIcon,
                action = delegate
                {
                    var AbilityUser = InnerPawn;
                    AbilityUser.Drawer.Notify_DebugAffected();
                    ResurrectionUtility.Resurrect(AbilityUser);
                    MoteMaker.ThrowText(AbilityUser.PositionHeld.ToVector3(), AbilityUser.MapHeld,
                        StringsToTranslate.AU_CastSuccess);
                    AbilityUser.BloodNeed().AdjustBlood(-99999999);
                    HealthUtility.AdjustSeverity(AbilityUser, VampDefOf.ROMV_TheBeast, 1.0f);
                    var MentalState_VampireBeast = DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
                    AbilityUser.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true);
                    Find.LetterStack.ReceiveLetter("ROMV_TheBeast".Translate(),
                        "ROMV_TheBeastDesc".Translate(InnerPawn.Label), VampDefOf.ROMV_FrenzyMessage, this);
                },
                disabled = false
            };
    }

    public override void TickRare()
    {
        base.TickRare();

        if (!Spawned || MapHeld == null) return;

        if (BurnedToAshes) return;

        var curVec = PositionHeld;
        if (curVec.Roofed(MapHeld) || !VampireUtility.IsDaylight(MapHeld)) return;

        FleckMaker.ThrowSmoke(DrawPos, Map, 1f);
        FleckMaker.ThrowFireGlow(PositionHeld.ToVector3(), Map, 1f);
        if (corpseBurnCount > 10)
            BurnedToAshes = true;
        corpseBurnCount++;
    }

    // Token: 0x060047A3 RID: 18339 RVA: 0x002079C4 File Offset: 0x00205DC4
    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref bloodPoints, "bloodPoints", -1);
        Scribe_Values.Look(ref burnedToAshes, "burnedToAshes");
        Scribe_Values.Look(ref diableried, "diableried");

        Scribe_Values.Look(ref timeOfDeath, "timeOfDeath");
        Scribe_Values.Look(ref vanishAfterTimestamp, "vanishAfterTimestamp");
        Scribe_Values.Look(ref everBuriedInSarcophagus, "everBuriedInSarcophagus");
        Scribe_Deep.Look(ref operationsBillStack, "operationsBillStack", this);
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }
}