using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public partial class HarmonyPatches
{
    public static void HarmonyPatches_Graphics(Harmony harmony)
    {
        // GRAPHICS
        ////////////////////////////////////////////////////////////////////////////
        //Gives different skin color for Vampires
        harmony.Patch(AccessTools.Method(typeof(Pawn_StoryTracker), "get_SkinColor"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(get_SkinColor_Vamp)));
        //Log.Message("22");
        //Changes vampire appearances and statistics based on their current forms
        harmony.Patch(AccessTools.Method(typeof(Pawn), "get_BodySize"), null, new HarmonyMethod(
            typeof(HarmonyPatches), nameof(VampireBodySize)));
        //Log.Message("23");
        harmony.Patch(AccessTools.Method(typeof(Pawn), "get_HealthScale"), null, new HarmonyMethod(
            typeof(HarmonyPatches), nameof(VampireHealthScale)));
        //Log.Message("24");
        harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), null, new HarmonyMethod(
            typeof(HarmonyPatches), nameof(Vamp_ResolveAllGraphics)));
        //Log.Message("25");
        harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"), new HarmonyMethod(
            typeof(HarmonyPatches), nameof(Vamp_ResolveApparelGraphics)));

        //Log.Message("26");
        harmony.Patch(AccessTools.Method(typeof(Scenario), "Notify_NewPawnGenerating"), null, new HarmonyMethod(
            typeof(HarmonyPatches), nameof(Vamp_NewPawnGenerating)));

        //harmony.Patch(AccessTools.Property(typeof(RaceProperties), "Humanlike").GetGetMethod(), new HarmonyMethod(
        //    typeof(HarmonyPatches),
        //    nameof(Vamp_HumanlikeMeshExclusion)), null);

        // TODO Needs fixing
        //            ////Log.Message("26");
        //            harmony.Patch(
        //                AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal",
        //                    new[]
        //                    {
        //                        typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
        //                        typeof(bool), typeof(bool)
        //                    }), null, null,
        //                new HarmonyMethod(typeof(HarmonyPatches), nameof(RenderPawnInternalTranspiler)));
        //
        //Log.Message("27");
        harmony.Patch(
            AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal",
                new[]
                {
                    typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode),
                    typeof(PawnRenderFlags)
                }), new HarmonyMethod(typeof(HarmonyPatches),
                nameof(RenderVampire)));


        //Log.Message("27a");
        //Vampires do not make breath motes
        harmony.Patch(AccessTools.Method(typeof(PawnBreathMoteMaker), "ProcessPostTickVisuals"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_NoBreathingMote)));

        //Log.Message("28");
        harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "MatsBodyBaseAt"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_MatsBodyBaseAt)));
        //Log.Message("29");
    }


    public static bool RenderVampire(PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody,
        Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
    {
        var quat = Quaternion.AngleAxis(angle, Vector3.up);

        var p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (p?.Map?.GetComponent<MapComponent_HiddenTracker>()?.hiddenCharacters?.Contains(p) ?? false)
        {
            //<texPath>Things/Pawn/Animal/Tenebrous/tenebrous</texPath>
            //<drawSize>2.0</drawSize>
            if (VampireGraphicUtility.invisibleForm == null)
            {
                var graphicData = new GraphicData();
                graphicData.drawSize = new Vector2(2, 2);
                graphicData.texPath = "Things/Pawn/Hidden/hidden";
                VampireGraphicUtility.invisibleForm = graphicData.Graphic;
            }

            __instance.graphics.nakedGraphic = VampireGraphicUtility.invisibleForm;
            return false;
        }

        if (p?.Map?.GetComponent<MapComponent_HiddenTracker>()?.toRemoveCharacters?.Contains(p) ?? false)
        {
            __instance.graphics.nakedGraphic = null;
            if (p.IsVampire(true) && p.VampComp() is { } vampCompy)
                vampCompy.atDirty = true;
            p?.Map?.GetComponent<MapComponent_HiddenTracker>()?.toRemoveCharacters.Remove(p);
            //return false;
        }

        if (p?.VampComp() is { } v)
        {
            if (v.Transformed)
            {
                if (__instance.graphics.nakedGraphic == null || v.CurFormGraphic == null || v.atDirty)
                {
                    if (v.CurrentForm != null)
                    {
                        if (v.CurrentForm.GetCompProperties<CompAnimated.CompProperties_Animated>() is
                            CompAnimated.CompProperties_Animated Props)
                        {
                            var curGraphic = v.CurFormGraphic;
                            v.CurFormGraphic = CompAnimated.CompAnimated.ResolveCurGraphic(p, Props, ref curGraphic,
                                ref v.atCurIndex, ref v.atCurTicks, ref v.atDirty, false);
                        }
                        else
                        {
                            v.CurFormGraphic = v.CurrentForm.bodyGraphicData.Graphic;
                        }
                    }
                    else
                    {
                        v.CurFormGraphic = p.kindDef.lifeStages[p.ageTracker.CurLifeStageIndex].bodyGraphicData
                            .Graphic; // v.CurrentForm.lifeStages[0].bodyGraphicData.Graphic;
                    }

                    __instance.graphics.nakedGraphic = v.CurFormGraphic;
                    __instance.graphics.ResolveApparelGraphics();
                }

                Mesh mesh = null;
                if (renderBody)
                {
                    var loc = rootLoc;
                    loc.y += 0.0046875f;
                    if (bodyDrawType == RotDrawMode.Dessicated && !p.RaceProps.Humanlike &&
                        __instance.graphics.dessicatedGraphic != null && !flags.HasFlag(PawnRenderFlags.Portrait))
                    {
                        __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, p);
                    }
                    else
                    {
                        mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                        var list = __instance.graphics.MatsBodyBaseAt(bodyFacing, p.Dead, bodyDrawType);
                        for (var i = 0; i < list.Count; i++)
                        {
                            var damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                            var scaleVector = new Vector3(loc.x, loc.y, loc.z);
                            if (flags.HasFlag(PawnRenderFlags.Portrait))
                            {
                                scaleVector.x *= 1f + (1f - (flags.HasFlag(PawnRenderFlags.Portrait)
                                        ? v.CurrentForm.bodyGraphicData.drawSize
                                        : v.CurrentForm.bodyGraphicData.drawSize)
                                    .x);
                                scaleVector.z *= 1f + (1f - (flags.HasFlag(PawnRenderFlags.Portrait)
                                        ? v.CurrentForm.bodyGraphicData.drawSize
                                        : v.CurrentForm.bodyGraphicData.drawSize)
                                    .y);
                            }
                            else
                            {
                                scaleVector = new Vector3(0, 0, 0);
                            }

                            GenDraw.DrawMeshNowOrLater(mesh, loc + scaleVector, quat, damagedMat,
                                flags.FlagSet(PawnRenderFlags.DrawNow));
                            //HasFlag(PawnRenderFlags.Portrait));

                            //GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.TRS(loc + scaleVector, quat, Vector3.one), damagedMat, flags.FlagSet(PawnRenderFlags.DrawNow));

                            loc.y += 0.0046875f;
                        }

                        if (bodyDrawType == RotDrawMode.Fresh)
                        {
                            var drawLoc = rootLoc;
                            drawLoc.y += 0.01875f;
                            Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>()
                                .RenderPawnOverlay(drawLoc, mesh, quat, flags.FlagSet(PawnRenderFlags.DrawNow), 
                                    PawnOverlayDrawer.OverlayLayer.Body, bodyFacing, false);
                            //Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>().RenderOverBody(drawLoc, mesh, quat, flags.HasFlag(PawnRenderFlags.Portrait), BodyTypeDef.WoundLayer.Body, bodyFacing);
                        }
                    }
                }

                return false;
            }

            if (!v.Transformed && v.CurFormGraphic != null)
            {
                v.CurFormGraphic = null;
                __instance.graphics.ResolveAllGraphics();
            }
        }

        return true;
    }


    // RimWorld.Pawn_StoryTracker
    public static bool get_SkinColor_Vamp(Pawn_StoryTracker __instance, ref Color __result)
    {
        var p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (p.IsVampire(true))
        {
            __result = VampireSkinColors.GetVampireSkinColor(p,
                Traverse.Create(__instance).Field("melanin").GetValue<float>());
            return false;
        }

        return true;
    }

    // Verse.Pawn
    public static void VampireBodySize(Pawn __instance, ref float __result)
    {
        if (__instance?.VampComp() is { } v && v.Transformed && v.CurrentForm != null)
            __result = v.CurrentForm
                .baseBodySize; //Mathf.Clamp((__result * w.CurrentWerewolfForm.def.sizeFactor) + (w.CurrentWerewolfForm.level * 0.1f), __result, __result * (w.CurrentWerewolfForm.def.sizeFactor * 2));
    }

    // Verse.Pawn
    public static void VampireHealthScale(Pawn __instance, ref float __result)
    {
        if (__instance?.VampComp() is { } v && v.Transformed && v.CurrentForm != null)
            __result = v.CurrentForm
                .baseHealthScale; //Mathf.Clamp((__result * w.CurrentWerewolfForm.def.sizeFactor) + (w.CurrentWerewolfForm.level * 0.1f), __result, __result * (w.CurrentWerewolfForm.def.sizeFactor * 2));
    }


    public static void Vamp_ResolveAllGraphics(PawnGraphicSet __instance)
    {
        if (__instance?.pawn?.VampComp() is { } v && v.IsVampire && !v.Transformed)
        {
            if (v?.Bloodline?.nakedBodyGraphicsPath != "")
            {
                var newBodyGraphic = VampireGraphicUtility.GetNakedBodyGraphic(__instance.pawn,
                    __instance.pawn.story.bodyType, ShaderDatabase.CutoutSkin, __instance.pawn.story.SkinColor);
                if (newBodyGraphic != null)
                    __instance.nakedGraphic = newBodyGraphic;
            }

            if (v?.Bloodline?.headGraphicsPath != "")
            {
                var headPath = VampireGraphicUtility.GetHeadGraphicPath(__instance.pawn);
                if (headPath != "")
                {
                    Graphic newHeadGraphic = VampireGraphicUtility.GetVampireHead(__instance.pawn, headPath,
                        __instance.pawn.story.SkinColor);
                    if (newHeadGraphic != null)
                        __instance.headGraphic = newHeadGraphic;
                }
            }

            __instance.ResolveApparelGraphics();
        }
    }

    // Verse.PawnGraphicSet
    public static bool Vamp_ResolveApparelGraphics(PawnGraphicSet __instance)
    {
        if (__instance.pawn.VampComp() is { } v && v.CurrentForm != null)
        {
            __instance.ClearCache();
            __instance.apparelGraphics.Clear();
            return false;
        }

        return true;
    }


    //Scenario
    public static void Vamp_NewPawnGenerating(Scenario __instance, Pawn pawn, PawnGenerationContext context)
    {
        if (VampireSettings.ShouldUseSettings)
        {
            if (Rand.Chance(VampireSettings.Get.spawnPct) && pawn.RaceProps.Humanlike)
            {
                var hediff = HediffMaker.MakeHediff(VampDefOf.ROM_Vampirism, pawn);
                hediff.Severity = 1f;
                pawn.health.AddHediff(hediff);
            }
        }
        else if (VampireSettings.Get.settingsWindowSeen == false)
        {
            if (VampireSettings.Get.mode == GameMode.Disabled)
            {
                VampireSettings.Get.settingsWindowSeen = true;
                VampireSettings.Get.mode = GameMode.Standard;
                VampireSettings.Get.ApplySettings();
            }
        }
    }

    // RimWorld.PawnBreathMoteMaker
    public static bool Vamp_NoBreathingMote(PawnBreathMoteMaker __instance)
    {
        var pawn = (Pawn)AccessTools.Field(typeof(PawnBreathMoteMaker), "pawn").GetValue(__instance);
        if (pawn.IsVampire(true)) return false;

        return true;
    }

    //PawnGraphicSet.MatsBodyBaseAt
    public static bool Vamp_MatsBodyBaseAt(PawnGraphicSet __instance, Rot4 facing, RotDrawMode bodyCondition,
        bool drawClothes, ref List<Material> __result)
    {
        if (__instance.nakedGraphic != null) return true;
        if (__instance.pawn.IsVampire(true) && __instance?.pawn?.VampComp()?.CurrentForm != null)
        {
            __instance.nakedGraphic = __instance.pawn.VampComp().CurrentForm.bodyGraphicData.Graphic;
            return true;
        }

        __result = new List<Material>();
        __result.Add(GraphicDatabase.Get<Graphic_Single>("NullTex").MatSingle);
        return false;
    }
}