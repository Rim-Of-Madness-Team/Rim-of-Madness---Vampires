using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Vampire;

public static class VampireSkinColors
{
    // RimWorld.PawnSkinColors
    private static readonly SkinColorData[] MelaninTable =
    {
        new(0f, 0f, new Color(0.955f, 0.945f, 0.925f)), //new Color(0.9490196f, 0.929411769f, 0.8784314f)),
        new(0.25f, 0.215f, new Color(0.83f, 0.83f, 0.83f)), //new Color(1f, 0.9372549f, 0.8352941f)),
        new(0.5f, 0.715f, new Color(0.92f, 0.91f, 0.85f)), //new Color(1f, 0.9372549f, 0.7411765f)),
        new(0.75f, 0.8f, new Color(0.84f, 0.81f, 0.75f)), //new Color(0.894117653f, 0.619607866f, 0.3529412f)),
        new(0.9f, 0.95f, new Color(0.56f, 0.52f, 0.48f)), //new Color(0.509803951f, 0.356862754f, 0.1882353f)),
        new(1f, 1f, new Color(0.44f, 0.42f, 0.38f)) //new Color(0.3882353f, 0.274509817f, 0.141176477f))
    };

    // RimWorld.PawnSkinColors
    private static int GetSkinDataIndexOfMelanin(float melanin)
    {
        var result = 0;
        for (var i = 0; i < MelaninTable.Length; i++)
        {
            if (melanin < MelaninTable[i].melanin) break;
            result = i;
        }

        return result;
    }

    // RimWorld.PawnSkinColors
    public static Color GetVampireSkinColor(Pawn pawn, float melanin)
    {
        var skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
        if (pawn?.VampComp()?.Bloodline?.skinColors is { } colors && !colors.NullOrEmpty())
        {
            if (skinDataIndexOfMelanin == colors.Count - 1) return colors[skinDataIndexOfMelanin];
            var tt = Mathf.InverseLerp(MelaninTable[skinDataIndexOfMelanin].melanin,
                MelaninTable[skinDataIndexOfMelanin + 1].melanin, melanin);
            return Color.Lerp(colors[skinDataIndexOfMelanin], colors[skinDataIndexOfMelanin + 1], tt);
        }

        if (skinDataIndexOfMelanin == MelaninTable.Length - 1) return MelaninTable[skinDataIndexOfMelanin].color;
        var t = Mathf.InverseLerp(MelaninTable[skinDataIndexOfMelanin].melanin,
            MelaninTable[skinDataIndexOfMelanin + 1].melanin, melanin);
        return Color.Lerp(MelaninTable[skinDataIndexOfMelanin].color, MelaninTable[skinDataIndexOfMelanin + 1].color,
            t);
    }

    private struct SkinColorData
    {
        public readonly float melanin;

        public float selector;

        public readonly Color color;

        public SkinColorData(float melanin, float selector, Color color)
        {
            this.melanin = melanin;
            this.selector = selector;
            this.color = color;
        }
    }
}