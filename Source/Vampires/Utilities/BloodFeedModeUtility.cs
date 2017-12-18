using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Vampire
{
    [StaticConstructorOnStartup]
    public static class BloodFeedModeUtility
    {
        private static readonly Texture2D NoneIcon = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/None");
        private static readonly Texture2D AnimalNonLethalIcon = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/AnimalNonLethal");
        private static readonly Texture2D AnimalLethalIcon = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/AnimalLethal");
        private static readonly Texture2D HumanoidNonLethalIcon = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/HumanoidNonLethal");
        private static readonly Texture2D HumanoidLethalIcon = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/HumanoidLethal");

        private static readonly Texture2D HumanoidTypeAll = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/HumanoidAll");
        private static readonly Texture2D HumanoidTypePrisonersOnly = ContentFinder<Texture2D>.Get("UI/Icons/BloodFeedMode/HumanoidPrisoners");

        public static void DrawFeedModeButton(Vector2 pos, Pawn pawn)
        {
            Need_Blood vampBlood = pawn.needs.TryGetNeed<Need_Blood>();
            Texture2D icon = vampBlood.preferredFeedMode.GetIcon();
            Rect rect = new Rect(pos.x, pos.y, 24f, 24f);
                         
                if (Widgets.ButtonImage(rect, icon))
                {
                vampBlood.preferredFeedMode = GetNextResponse(pawn);
                    SoundDefOf.TickHigh.PlayOneShotOnCamera();
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
                }
                UIHighlighter.HighlightOpportunity(rect, "ROMV_FeedMode");
                TooltipHandler.TipRegion(rect, string.Concat(new string[]
                {
        "ROMV_FeedMode_Tip".Translate(),
        "\n\n",
        "ROMV_FeedMode_CurrentMode".Translate(),
        ": ",
        vampBlood.preferredFeedMode.GetLabel()
                }));
            if (vampBlood.preferredFeedMode > PreferredFeedMode.AnimalLethal)
            {
                Texture2D iconSub = vampBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.All ? HumanoidTypeAll : HumanoidTypePrisonersOnly;
                Rect rectSub = new Rect(pos.x, rect.yMax + 5f, 24f, 24f);

                if (Widgets.ButtonImage(rectSub, iconSub))
                {
                    if (vampBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.All)
                        vampBlood.preferredHumanoidFeedType = PreferredHumanoidFeedType.PrisonersOnly;
                    else if (vampBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.PrisonersOnly)
                        vampBlood.preferredHumanoidFeedType = PreferredHumanoidFeedType.All;
                    SoundDefOf.TickHigh.PlayOneShotOnCamera();
                }
                UIHighlighter.HighlightOpportunity(rectSub, "ROMV_FeedModeHumanoidType");
                TooltipHandler.TipRegion(rectSub, string.Concat(new string[]
                {
        "ROMV_FeedMode_CurrentType".Translate(),
        ": ",
        vampBlood.preferredHumanoidFeedType == PreferredHumanoidFeedType.All ? "ROMV_FeedMode_TypeAll".Translate() : "ROMV_FeedMode_TypePrisonersOnly".Translate()
                }));
            }
        }

        // RimWorld.HostilityResponseModeUtility
        public static PreferredFeedMode GetNextResponse(Pawn pawn)
        {
            switch (pawn.needs.TryGetNeed<Need_Blood>().preferredFeedMode)
            {
                case PreferredFeedMode.None:
                    return PreferredFeedMode.AnimalNonLethal;
                case PreferredFeedMode.AnimalNonLethal:
                    if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        return PreferredFeedMode.HumanoidNonLethal;
                    }
                    return PreferredFeedMode.AnimalLethal;
                case PreferredFeedMode.AnimalLethal:
                    return PreferredFeedMode.HumanoidNonLethal;
                case PreferredFeedMode.HumanoidNonLethal:
                    if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        return PreferredFeedMode.None;
                    }
                    return PreferredFeedMode.HumanoidLethal;
                case PreferredFeedMode.HumanoidLethal:
                    return PreferredFeedMode.None;
                default:
                    return PreferredFeedMode.None;
            }
        }


        public static string GetLabel(this PreferredFeedMode feed)
        {
            switch (feed)
            {
                case PreferredFeedMode.None:
                    return "ROMV_FeedMode_None".Translate();
                case PreferredFeedMode.AnimalNonLethal:
                    return "ROMV_FeedMode_AnimalNonLethal".Translate();
                case PreferredFeedMode.AnimalLethal:
                    return "ROMV_FeedMode_AnimalLethal".Translate();
                case PreferredFeedMode.HumanoidNonLethal:
                    return "ROMV_FeedMode_HumanoidNonLethal".Translate();
                case PreferredFeedMode.HumanoidLethal:
                    return "ROMV_FeedMode_HumanoidLethal".Translate();
                default:
                    throw new InvalidOperationException();
            }
        }


        // RimWorld.HostilityResponseModeUtility
        public static Texture2D GetIcon(this PreferredFeedMode response)
        {
            switch (response)
            {
                case PreferredFeedMode.None:
                    return NoneIcon;
                case PreferredFeedMode.AnimalNonLethal:
                    return AnimalNonLethalIcon;
                case PreferredFeedMode.AnimalLethal:
                    return AnimalLethalIcon;
                case PreferredFeedMode.HumanoidNonLethal:
                    return HumanoidNonLethalIcon;
                case PreferredFeedMode.HumanoidLethal:
                    return HumanoidLethalIcon;
                default:
                    return BaseContent.BadTex;
            }
        }


    }
}
