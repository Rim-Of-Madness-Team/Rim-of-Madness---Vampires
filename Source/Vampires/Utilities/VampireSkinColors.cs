using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Vampire
{
    public static class VampireSkinColors
    {
        private struct SkinColorData
        {
            public float melanin;

            public float selector;

            public Color color;

            public SkinColorData(float melanin, float selector, Color color)
            {
                this.melanin = melanin;
                this.selector = selector;
                this.color = color;
            }
        }

        // RimWorld.PawnSkinColors
        private static readonly SkinColorData[] MelaninTable = new SkinColorData[]
        {
            new SkinColorData(0f, 0f, new Color(0.955f, 0.945f, 0.925f)),//new Color(0.9490196f, 0.929411769f, 0.8784314f)),
            new SkinColorData(0.25f, 0.215f, new Color(0.83f, 0.83f, 0.83f)),//new Color(1f, 0.9372549f, 0.8352941f)),
            new SkinColorData(0.5f, 0.715f, new Color(0.92f, 0.91f, 0.85f)),//new Color(1f, 0.9372549f, 0.7411765f)),
            new SkinColorData(0.75f, 0.8f, new Color(0.84f, 0.81f, 0.75f)),//new Color(0.894117653f, 0.619607866f, 0.3529412f)),
            new SkinColorData(0.9f, 0.95f, new Color(0.56f, 0.52f, 0.48f)),//new Color(0.509803951f, 0.356862754f, 0.1882353f)),
            new SkinColorData(1f, 1f, new Color(0.44f, 0.42f, 0.38f)) //new Color(0.3882353f, 0.274509817f, 0.141176477f))
        };

        // RimWorld.PawnSkinColors
        private static int GetSkinDataIndexOfMelanin(float melanin)
        {
            int result = 0;
            for (int i = 0; i < VampireSkinColors.MelaninTable.Length; i++)
            {
                if (melanin < VampireSkinColors.MelaninTable[i].melanin)
                {
                    break;
                }
                result = i;
            }
            return result;
        }
        
        // RimWorld.PawnSkinColors
        public static Color GetVampireSkinColor(Pawn pawn, float melanin)
        {

            int skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
            if (pawn?.VampComp()?.Bloodline?.skinColors is List<Color> colors && !colors.NullOrEmpty())
            {
                if (skinDataIndexOfMelanin == colors.Count - 1)
                {
                    return colors[skinDataIndexOfMelanin];
                }
                float tt = Mathf.InverseLerp(VampireSkinColors.MelaninTable[skinDataIndexOfMelanin].melanin, VampireSkinColors.MelaninTable[skinDataIndexOfMelanin + 1].melanin, melanin);
                return Color.Lerp(colors[skinDataIndexOfMelanin], colors[skinDataIndexOfMelanin + 1], tt);
            }
            if (skinDataIndexOfMelanin == VampireSkinColors.MelaninTable.Length - 1)
            {
                return VampireSkinColors.MelaninTable[skinDataIndexOfMelanin].color;
            }
            float t = Mathf.InverseLerp(VampireSkinColors.MelaninTable[skinDataIndexOfMelanin].melanin, VampireSkinColors.MelaninTable[skinDataIndexOfMelanin + 1].melanin, melanin);
            return Color.Lerp(VampireSkinColors.MelaninTable[skinDataIndexOfMelanin].color, VampireSkinColors.MelaninTable[skinDataIndexOfMelanin + 1].color, t);
        }
    }
}
