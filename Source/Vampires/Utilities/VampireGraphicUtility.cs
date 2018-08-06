using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class VampireGraphicUtility
    {
        // Verse.GraphicDatabaseHeadRecords
        public static Graphic_Multi GetVampireHead(Pawn pawn, string graphicPath, Color skinColor)
        {
            if (pawn.VampComp() is CompVampire v && v.IsVampire && !v.Transformed && v.Bloodline.headGraphicsPath != "")
            {
                return (Graphic_Multi) GraphicDatabase.Get<Graphic_Multi>(graphicPath, ShaderDatabase.CutoutSkin,
                    Vector2.one, skinColor);
            }
            return null;
        }

        // RimWorld.Pawn_StoryTracker
        public static string GetHeadGraphicPath(Pawn pawn)
        {
            try
            {
                if (pawn.VampComp() is CompVampire v && v.IsVampire && v.Bloodline.headGraphicsPath != "")
                {
                    if (GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor) is
                        Graphic giggity)
                    {
                        string
                            headGraphicPath =
                                giggity
                                    .path; //pawn.story.HeadGraphicPath; //Traverse.Create(pawn.story).Field("headGraphicPath").GetValue<string>();
                        string pathToReplace = "Things/Pawn/Humanlike/Heads/";
                        headGraphicPath = headGraphicPath.Replace(pathToReplace, v.Bloodline.headGraphicsPath);
                        return headGraphicPath;
                    }
                }
            }
            catch
            {
            }
            return "";
        }

        // Verse.GraphicGetter_NakedHumanlike
        public static Graphic GetNakedBodyGraphic(Pawn pawn, BodyTypeDef bodyType, Shader shader, Color skinColor)
        {
            //Log.Message(pawn.ToString());
            //Log.Message(bodyType.ToString());
            //Log.Message(shader.ToString());
            //Log.Message(skinColor.ToString());
            //Log.Message("1");


            if (pawn?.VampComp() is CompVampire v && v.IsVampire && v.Bloodline != null &&
                v?.Bloodline?.nakedBodyGraphicsPath != "")
            {
                //Log.Message("2");


                if (v.Transformed)
                    return null;
                //Log.Message("3");

                string str = "Naked_" + bodyType.ToString();
                //Log.Message("4");

                string path = v.Bloodline.nakedBodyGraphicsPath + str;
                //Log.Message("5");

                Graphic result = GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
                //Log.Message("6");
                return result;
            }
            return null;
        }

        private static Graphic invisibleForm = null;

        public static bool RenderVampire(PawnRenderer __instance, Vector3 rootLoc, float quat, bool renderBody,
            Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
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
                if (p.IsVampire() && p.VampComp() is CompVampire vampCompy)
                    vampCompy.atDirty = true;
                p?.Map?.GetComponent<MapComponent_HiddenTracker>()?.toRemoveCharacters.Remove(p);
                //return false;
            }
            if (p?.VampComp() is CompVampire v)
            {
                if (v.Transformed)
                {
                    if (__instance.graphics.nakedGraphic == null || v.CurFormGraphic == null || v.atDirty)
                    {
                        if (v.CurrentForm != null)
                        {
                            if (v.CurrentForm.race.GetCompProperties<CompAnimated.CompProperties_Animated>() is
                                CompAnimated.CompProperties_Animated Props)
                            {
                                Graphic curGraphic = v.CurFormGraphic;
                                v.CurFormGraphic = CompAnimated.CompAnimated.ResolveCurGraphic(p, Props, ref curGraphic,
                                    ref v.atCurIndex, ref v.atCurTicks, ref v.atDirty, false);
                            }
                            else
                            {
                                v.CurFormGraphic = v.CurrentForm.lifeStages[0].bodyGraphicData.Graphic;
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
                        Vector3 loc = rootLoc;
                        loc.y += 0.0046875f;
                        if (bodyDrawType == RotDrawMode.Dessicated && !p.RaceProps.Humanlike &&
                            __instance.graphics.dessicatedGraphic != null && !portrait)
                        {
                            __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, p);
                        }
                        else
                        {
                            mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                            List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                            for (int i = 0; i < list.Count; i++)
                            {
                                Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
                                Vector3 scaleVector = new Vector3(loc.x, loc.y, loc.z);
                                if (portrait)
                                {
                                    scaleVector.x *=
                                        1f + (1f - (portrait
                                                  ? v.CurrentForm.lifeStages[0].bodyGraphicData.drawSize
                                                  : v.CurrentForm.lifeStages[0].bodyGraphicData.drawSize)
                                              .x);
                                    scaleVector.z *=
                                        1f + (1f - (portrait
                                                  ? v.CurrentForm.lifeStages[0].bodyGraphicData.drawSize
                                                  : v.CurrentForm.lifeStages[0].bodyGraphicData.drawSize)
                                              .y);
                                }
                                else scaleVector = new Vector3(0, 0, 0);
                                GenDraw.DrawMeshNowOrLater(mesh, loc + scaleVector, quat.ToQuat(), damagedMat, portrait);
                                loc.y += 0.0046875f;
                            }
                            if (bodyDrawType == RotDrawMode.Fresh)
                            {
                                Vector3 drawLoc = rootLoc;
                                drawLoc.y += 0.01875f;
                                Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>()
                                    .RenderOverBody(drawLoc, mesh, quat.ToQuat(), portrait);
                            }
                        }
                    }
                    return false;
                }
                else if (!v.Transformed && v.CurFormGraphic != null)
                {
                    v.CurFormGraphic = null;
                    __instance.graphics.ResolveAllGraphics();
                }
            }
            return true;
        }
    }
}