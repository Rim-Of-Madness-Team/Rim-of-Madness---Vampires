using UnityEngine;
using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class TexButton
    {
        //VampireIcon
        public static readonly Texture2D ROMV_VampireIcon = ContentFinder<Texture2D>.Get("UI/VampireIcon");
        public static readonly Texture2D ROMV_NullTex = ContentFinder<Texture2D>.Get("NullTex");
        public static readonly Texture2D ROMV_GhoulIcon = ContentFinder<Texture2D>.Get("UI/GhoulIcon");
        public static readonly Texture2D ROMV_HumanIcon = ContentFinder<Texture2D>.Get("UI/HumanIcon");
        public static readonly Texture2D ROMV_PointEmpty = ContentFinder<Texture2D>.Get("UI/PointEmpty");
        public static readonly Texture2D ROMV_PointFull = ContentFinder<Texture2D>.Get("UI/PointFull");
        public static readonly Texture2D ROMV_SunlightPolicyRestricted = ContentFinder<Texture2D>.Get("UI/Icons/SunlightPolicy/SunlightPolicyRestricted");
        public static readonly Texture2D ROMV_SunlightPolicyRelaxed = ContentFinder<Texture2D>.Get("UI/Icons/SunlightPolicy/SunlightPolicyRelaxed");
        public static readonly Texture2D ROMV_SunlightPolicyNoAI = ContentFinder<Texture2D>.Get("UI/Icons/SunlightPolicy/SunlightPolicyNoAI");
        //public static readonly Texture2D ROMV_Ashes = ContentFinder<Texture2D>.Get("Things/Item/Resource/VampireAshes", true);
    }
}