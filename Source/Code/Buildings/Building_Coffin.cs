using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public class Building_Coffin : Building_Sarcophagus
{
    
    private static readonly Color SheetColorNormal = new Color(0.6313726f, 0.8352941f, 0.7058824f);
    private static readonly Color SheetColorRoyal = new Color(0.67058825f, 0.9137255f, 0.74509805f);
    public static readonly Color SheetColorForPrisoner = new Color(1f, 0.7176471f, 0.12941177f);
    private static readonly Color SheetColorMedical = new Color(0.3882353f, 0.62352943f, 0.8862745f);
    private static readonly Color SheetColorMedicalForPrisoner = new Color(0.654902f, 0.3764706f, 0.15294118f);
    private static readonly Color SheetColorForSlave = new Color32(252, 244, 3, byte.MaxValue);
    private static readonly Color SheetColorMedicalForSlave = new Color32(153, 148, 0, byte.MaxValue);
    
    // RimWorld.Building_Grave
#pragma warning disable 169
    private Graphic cachedGraphicFull;
#pragma warning restore 169


    //        public override void Draw()

    //public override bool Accepts(Thing thing)

    public Building_Bed Bed
    {
        get
        {
            if (GetComps<CompVampBed>().FirstOrDefault() is { } vbed)
                if (vbed.Bed != null)
                    return vbed.Bed;
            return null;
        }
    }
    
    public override Graphic Graphic
    {
        get
        {
            if (!HasCorpse && !HasAnyContents)
            {
                return def.graphicData.GraphicColoredFor(this);
            }
            if (def.building.fullGraveGraphicData == null)
            {
                return def.graphicData.GraphicColoredFor(this);
            }
            if (cachedGraphicFull == null)
            {
                cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
            }
            return cachedGraphicFull;
        }
    }

    public override Color DrawColor
    {
        get
        {
            if (def.MadeFromStuff)
            {
                return base.DrawColor;
            }
            return DrawColorTwo;
        }
    }
    
    public override Color DrawColorTwo
    {
        get
        {
            try
            {
                if (Bed is not { } b)
                    return base.DrawColorTwo;
                if (Bed?.Spawned != true)
                    return base.DrawColorTwo;

                bool medical = b.Medical;
                BedOwnerType bedOwnerType = b.ForOwnerType;
                if (bedOwnerType != BedOwnerType.Prisoner)
                {
                    if (bedOwnerType != BedOwnerType.Slave)
                    {
                        if (medical)
                        {
                            return SheetColorMedical;
                        }

                        if (def == ThingDefOf.RoyalBed)
                        {
                            return SheetColorRoyal;
                        }

                        return SheetColorNormal;
                    }
                    else
                    {
                        if (!medical)
                        {
                            return SheetColorForSlave;
                        }

                        return SheetColorMedicalForSlave;
                    }
                }
                else
                {
                    if (!medical)
                    {
                        return SheetColorForPrisoner;
                    }

                    return SheetColorMedicalForPrisoner;
                }
            }
            catch
            {
                return base.DrawColorTwo;
            }
        }
    }
    

    private int CorpseCapacity => def.size.x;
    private int CorpsesLoaded => innerContainer.Count;

    public override bool Accepts(Thing thing)
    {
        return innerContainer.CanAcceptAnyOf(thing) && CorpsesLoaded < CorpseCapacity &&
               GetStoreSettings().AllowedToAccept(thing) &&
               (Bed == null || Bed?.CompAssignableToPawn?.AssignedPawnsForReading?.Count == 0 ? true : false);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        IEnumerator<Gizmo> enumerator = null;
        // if (((this.def.BuildableByPlayer && this.def.passability != Traversability.Impassable && !this.def.IsDoor) ||
        //      this.def.building.forceShowRoomStats) && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null &&
        //     Find.Selector.SingleSelectedObject == this)
        // {
        //     yield return new Gizmo_RoomStats(this);
        // }
        Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
        if (selectMonumentMarkerGizmo != null)
        {
            yield return selectMonumentMarkerGizmo;
        }
        if (this.def.Minifiable && base.Faction == Faction.OfPlayer)
        {
            yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
        }
        ColorInt? glowerColorOverride = null;
        CompGlower comp;
        if ((comp = base.GetComp<CompGlower>()) != null && comp.HasGlowColorOverride)
        {
            glowerColorOverride = new ColorInt?(comp.GlowColor);
        }
        Command command = BuildCopyCommandUtility.BuildCopyCommand(this.def, base.Stuff, base.StyleSourcePrecept as Precept_Building, base.StyleDef, true, glowerColorOverride);
        if (command != null)
        {
            yield return command;
        }
        if (base.Faction == Faction.OfPlayer || this.def.building.alwaysShowRelatedBuildCommands)
        {
            foreach (Command command2 in BuildRelatedCommandUtility.RelatedBuildCommands(this.def))
            {
                yield return command2;
            }
            IEnumerator<Command> enumerator2 = null;
        }

        var p = Corpse?.InnerPawn ?? (Pawn)innerContainer.FirstOrDefault() ?? null;
        if ((p?.IsVampire(true) ?? false) || (p?.HasVampireHediffs() ?? false))
            foreach (var y in HarmonyPatches.GraveGizmoGetter(p, this))
                yield return y;

        if (GetComps<CompVampBed>().FirstOrDefault() is { } vbed)
            if (vbed.Bed != null)
                if (vbed.Bed.def.building.bed_humanlike && Faction == Faction.OfPlayer)
                {
                    var command_Toggle0 = new Command_Toggle();
                    command_Toggle0.defaultLabel = "ROMV_VampiresOnly".Translate();
                    command_Toggle0.defaultDesc = "ROMV_VampiresOnlyDesc".Translate();
                    command_Toggle0.icon = TexButton.ROMV_VampiresOnly;
                    command_Toggle0.isActive = () => vbed.VampiresOnly;
                    command_Toggle0.toggleAction = delegate { vbed.VampiresOnly = !vbed.VampiresOnly; };
                    command_Toggle0.hotKey = KeyBindingDefOf.Misc2;
                    yield return command_Toggle0;

                    var command_Toggle = new Command_Toggle();
                    command_Toggle.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
                    command_Toggle.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
                    command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
                    command_Toggle.isActive = () => vbed.Bed.ForPrisoners;
                    command_Toggle.toggleAction = delegate
                    {
                        AccessTools.Method(typeof(Building_Bed), "SetBedOwnerTypeByInterface").Invoke(vbed.Bed,
                            new object[] { vbed.Bed.ForPrisoners ? BedOwnerType.Colonist : BedOwnerType.Prisoner });
                    };
                    if (!(bool)AccessTools.Method(typeof(Building_Bed), "RoomCanBePrisonCell")
                            .Invoke(vbed.Bed, new object[] { this.GetRoom() }) &&
                        !vbed.Bed.ForPrisoners)
                        command_Toggle.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
                    command_Toggle.hotKey = KeyBindingDefOf.Misc3;
                    command_Toggle.turnOffSound = null;
                    command_Toggle.turnOnSound = null;
                    yield return command_Toggle;
                    var command_Toggle2 = new Command_Toggle();
                    command_Toggle2.defaultLabel = "CommandBedSetAsMedicalLabel".Translate();
                    command_Toggle2.defaultDesc = "CommandBedSetAsMedicalDesc".Translate();
                    command_Toggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical");
                    command_Toggle2.isActive = () => vbed.Bed.Medical;
                    command_Toggle2.toggleAction = delegate { vbed.Bed.Medical = !vbed.Bed.Medical; };
                    command_Toggle2.hotKey = KeyBindingDefOf.Misc2;
                    yield return command_Toggle2;
                    if (!vbed.Bed.ForPrisoners && !vbed.Bed.Medical)
                    {
                        var command_Action = new Command_Action();
                        command_Action.defaultLabel = "CommandThingSetOwnerLabel".Translate();
                        command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
                        command_Action.defaultDesc = "CommandBedSetOwnerDesc".Translate();
                        command_Action.action = delegate
                        {
                            Find.WindowStack.Add(new Dialog_AssignBuildingOwner(vbed.Bed.CompAssignableToPawn));
                        };
                        command_Action.hotKey = KeyBindingDefOf.Misc3;
                        yield return command_Action;
                    }
                }
    }


    //public override IEnumerable<Gizmo> GetGizmos()
    //{
    //    foreach (Gizmo gizmo in base.GetGizmos())
    //    {
    //        yield return gizmo;
    //    }
    //    if (StorageTabVisible)
    //    {
    //        foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
    //        {
    //            yield return item;
    //        }
    //    }
    //    if (!HasCorpse)
    //    {
    //        Command_Action command_Action = new Command_Action();
    //        command_Action.defaultLabel = "CommandGraveAssignColonistLabel".Translate();
    //        command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
    //        command_Action.defaultDesc = "CommandGraveAssignColonistDesc".Translate();
    //        command_Action.action = delegate
    //        {
    //            Find.WindowStack.Add(new Dialog_AssignBuildingOwner(CompAssignableToPawn));
    //        };
    //        command_Action.hotKey = KeyBindingDefOf.Misc3;
    //        yield return command_Action;
    //    }
    //}

    //public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
}