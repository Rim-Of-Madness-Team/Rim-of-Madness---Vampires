using System;
using HarmonyLib;
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

        public static Graphic invisibleForm = null;


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
                    if (GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor, true) is
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

    }
}