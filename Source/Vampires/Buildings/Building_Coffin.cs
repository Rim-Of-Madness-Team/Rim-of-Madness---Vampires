using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire
{
    public class Building_Coffin : Building_Sarcophagus
    {
        // RimWorld.Building_Grave
#pragma warning disable 169
        private Graphic cachedGraphicFull;
#pragma warning restore 169


        //public override void Draw()

        //public override bool Accepts(Thing thing)

        public Building_Bed Bed
        {
            get
            {
                if (this.GetComps<CompVampBed>().FirstOrDefault() is CompVampBed vbed)
                {
                    if (vbed.Bed != null)
                    { return vbed.Bed; }
                }
                return null;
            }
        } 
        

        private int CorpseCapacity => this.def.size.x;
        private int CorpsesLoaded => innerContainer.Count;
        public override bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing) && CorpsesLoaded < CorpseCapacity && GetStoreSettings().AllowedToAccept(thing) && ((Bed == null || Bed?.CompAssignableToPawn?.AssignedPawnsForReading?.Count == 0) ? true : false);
        }

        // RimWorld.Building_Grave
        //public override Graphic Graphic


        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.AllComps != null)
            {
                for (int i = 0; i < AllComps.Count; i++)
                {
                    foreach (Gizmo item in AllComps[i].CompGetGizmosExtra())
                    {
                        yield return item;
                    }
                }
            }

            if (((def.BuildableByPlayer && def.passability != Traversability.Impassable && !def.IsDoor) || def.building.forceShowRoomStats) && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null && Find.Selector.SingleSelectedObject == this)
            {
                yield return new Gizmo_RoomStats(this);
            }
            if (def.Minifiable && base.Faction == Faction.OfPlayer)
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(def);
            }
            Command command = BuildCopyCommandUtility.BuildCopyCommand(def, base.Stuff);
            if (command != null)
            {
                yield return command;
            }
            if (base.Faction == Faction.OfPlayer)
            {
                foreach (Command item in BuildFacilityCommandUtility.BuildFacilityCommands(def))
                {
                    yield return item;
                }
            }

            var p = Corpse?.InnerPawn ?? (Pawn)(innerContainer.FirstOrDefault()) ?? null;
            if ((p?.IsVampire() ?? false) || (p?.HasVampireHediffs() ?? false))
            {
                foreach (Gizmo y in HarmonyPatches.GraveGizmoGetter(p, this))
                    yield return y;
            }

            if (this.GetComps<CompVampBed>().FirstOrDefault() is CompVampBed vbed)
            {
                if (vbed.Bed != null)
                {
                    if (vbed.Bed.def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
                    {
                        Command_Toggle command_Toggle0 = new Command_Toggle();
                        command_Toggle0.defaultLabel = "ROMV_VampiresOnly".Translate();
                        command_Toggle0.defaultDesc = "ROMV_VampiresOnlyDesc".Translate();
                        command_Toggle0.icon = TexButton.ROMV_VampiresOnly;
                        command_Toggle0.isActive = (() => vbed.VampiresOnly);
                        command_Toggle0.toggleAction = delegate
                        {
                            vbed.VampiresOnly = !vbed.VampiresOnly;
                        };
                        command_Toggle0.hotKey = KeyBindingDefOf.Misc2;
                        yield return command_Toggle0;

                        Command_Toggle command_Toggle = new Command_Toggle();
                        command_Toggle.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
                        command_Toggle.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
                        command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
                        command_Toggle.isActive = (() => vbed.Bed.ForPrisoners);
                        command_Toggle.toggleAction = delegate
                        {
                            AccessTools.Method(typeof(Building_Bed), "ToggleForPrisonersByInterface").Invoke(vbed.Bed, null);
                        };
                        if (!((bool)(AccessTools.Method(typeof(Building_Bed), "RoomCanBePrisonCell").Invoke(vbed.Bed, new object[] { this.GetRoom() }))) && !vbed.Bed.ForPrisoners)
                        {
                            command_Toggle.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
                        }
                        command_Toggle.hotKey = KeyBindingDefOf.Misc3;
                        command_Toggle.turnOffSound = null;
                        command_Toggle.turnOnSound = null;
                        yield return command_Toggle;
                        Command_Toggle command_Toggle2 = new Command_Toggle();
                        command_Toggle2.defaultLabel = "CommandBedSetAsMedicalLabel".Translate();
                        command_Toggle2.defaultDesc = "CommandBedSetAsMedicalDesc".Translate();
                        command_Toggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical");
                        command_Toggle2.isActive = (() => vbed.Bed.Medical);
                        command_Toggle2.toggleAction = delegate
                        {
                            vbed.Bed.Medical = !vbed.Bed.Medical;
                        };
                        command_Toggle2.hotKey = KeyBindingDefOf.Misc2;
                        yield return command_Toggle2;
                        if (!vbed.Bed.ForPrisoners && !vbed.Bed.Medical)
                        {
                            Command_Action command_Action = new Command_Action();
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
}
