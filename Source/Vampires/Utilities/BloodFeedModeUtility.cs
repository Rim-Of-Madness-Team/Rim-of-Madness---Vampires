using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static void DrawFeedModeButton(Vector2 pos, Pawn pawn)
        {
            Texture2D icon = pawn.needs.TryGetNeed<Need_Blood>().preferredFeedMode.GetIcon();
            Rect rect = new Rect(pos.x, pos.y, 24f, 24f);
                         
                if (Widgets.ButtonImage(rect, icon))
                {
                pawn.needs.TryGetNeed<Need_Blood>().preferredFeedMode = BloodFeedModeUtility.GetNextResponse(pawn);
                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
                }
                UIHighlighter.HighlightOpportunity(rect, "ROMV_FeedMode");
                TooltipHandler.TipRegion(rect, string.Concat(new string[]
                {
        "ROMV_FeedMode_Tip".Translate(),
        "\n\n",
        "ROMV_FeedMode_CurrentMode".Translate(),
        ": ",
        pawn.needs.TryGetNeed<Need_Blood>().preferredFeedMode.GetLabel()
                }));
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
                    return BloodFeedModeUtility.NoneIcon;
                case PreferredFeedMode.AnimalNonLethal:
                    return BloodFeedModeUtility.AnimalNonLethalIcon;
                case PreferredFeedMode.AnimalLethal:
                    return BloodFeedModeUtility.AnimalLethalIcon;
                case PreferredFeedMode.HumanoidNonLethal:
                    return BloodFeedModeUtility.HumanoidNonLethalIcon;
                case PreferredFeedMode.HumanoidLethal:
                    return BloodFeedModeUtility.HumanoidLethalIcon;
                default:
                    return BaseContent.BadTex;
            }
        }
    }
}
