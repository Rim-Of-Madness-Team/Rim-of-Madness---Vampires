using System;
using UnityEngine;
using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class TexButton
    {
        //VampireIcon
        public static readonly Texture2D ROMV_VampireIcon = ContentFinder<Texture2D>.Get("UI/VampireIcon", true);
        public static readonly Texture2D ROMV_HumanIcon = ContentFinder<Texture2D>.Get("UI/HumanIcon", true);
        public static readonly Texture2D ROMV_PointEmpty = ContentFinder<Texture2D>.Get("UI/PointEmpty", true);
        public static readonly Texture2D ROMV_PointFull = ContentFinder<Texture2D>.Get("UI/PointFull", true);
    }
}