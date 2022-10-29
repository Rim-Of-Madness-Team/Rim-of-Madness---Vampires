using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

[StaticConstructorOnStartup]
public static class VampireGraphicUtility
{
    // Token: 0x040039BC RID: 14780
    public static readonly Color SheetColorNormal = new(0.6313726f, 0.8352941f, 0.7058824f);

    // Token: 0x040039BD RID: 14781
    public static readonly Color SheetColorRoyal = new(0.67058825f, 0.9137255f, 0.74509805f);

    // Token: 0x040039BE RID: 14782
    public static readonly Color SheetColorForPrisoner = new(1f, 0.7176471f, 0.12941177f);

    // Token: 0x040039BF RID: 14783
    public static readonly Color SheetColorMedical = new(0.3882353f, 0.62352943f, 0.8862745f);

    // Token: 0x040039C0 RID: 14784
    public static readonly Color SheetColorMedicalForPrisoner = new(0.654902f, 0.3764706f, 0.15294118f);

    // Token: 0x040039C1 RID: 14785
    public static readonly Color SheetColorForSlave = new Color32(252, 244, 3, byte.MaxValue);

    // Token: 0x040039C2 RID: 14786
    public static readonly Color SheetColorMedicalForSlave = new Color32(153, 148, 0, byte.MaxValue);


    public static Graphic invisibleForm = null;


    // Verse.GraphicDatabaseHeadRecords
    public static Graphic_Multi GetVampireHead(Pawn pawn, string graphicPath, Color skinColor)
    {
        if (pawn.VampComp() is { } v && v.IsVampire && !v.Transformed && v.Bloodline.headGraphicsPath != "")
            return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, ShaderDatabase.CutoutSkin,
                Vector2.one, skinColor);
        return null;
    }

    // RimWorld.Pawn_StoryTracker
    public static string GetHeadGraphicPath(Pawn pawn)
    {
        try
        {
            if (pawn.VampComp() is { } v && v.IsVampire && v.Bloodline.headGraphicsPath != "")
                if (pawn.story.headType.GetGraphic(pawn.story.SkinColor) is
                    Graphic giggity)
                {
                    var
                        headGraphicPath =
                            giggity
                                .path; //pawn.story.HeadGraphicPath; //Traverse.Create(pawn.story).Field("headGraphicPath").GetValue<string>();
                    var pathToReplace = "Things/Pawn/Humanlike/Heads/";
                    headGraphicPath = headGraphicPath.Replace(pathToReplace, v.Bloodline.headGraphicsPath);
                    return headGraphicPath;
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


        if (pawn?.VampComp() is { } v && v.IsVampire && v.Bloodline != null &&
            v?.Bloodline?.nakedBodyGraphicsPath != "")
        {
            //Log.Message("2");


            if (v.Transformed)
                return null;
            //Log.Message("3");

            var str = "Naked_" + bodyType;
            //Log.Message("4");

            var path = v.Bloodline.nakedBodyGraphicsPath + str;
            //Log.Message("5");

            var result = GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
            //Log.Message("6");
            return result;
        }

        return null;
    }
}