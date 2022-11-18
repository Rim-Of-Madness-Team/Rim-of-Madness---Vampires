using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire;

public partial class HarmonyPatches
{
    // RimWorld.CharacterCardUtility
    public static bool isSwitched;

    public static void HarmonyPatches_UI(Harmony harmony)
    {
        // UI
        /////////////////////////////////////////////////////////////////////////////////////
        //Adds vampire skill sheet button to CharacterCard
        //Log.Message("30");
        harmony.Patch(
            AccessTools.Method(typeof(CharacterCardUtility), "DrawCharacterCard",
                new[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) , typeof(bool)}), null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DrawCharacterCard)));
        //Log.Message("31");
        //Fills the character card with a vampire skill sheet
        harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Character), "FillTab"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FillTab)));
        //Log.Message("32");

        harmony.Patch(AccessTools.Method(typeof(Verb_Shoot), "TryCastShot"),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TryCastShot)));
        //Log.Message("33");
    }

    public static void Vamp_DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback,
        Rect creationRect = default, bool showName = true)
    {
        if (pawn.IsVampire(true) || pawn.IsGhoul())
        {
            var flag = randomizeCallback != null;
            if (!flag && pawn.IsColonist && !pawn.health.Dead)
            {
                var guilty = pawn.guilt.IsGuilty;
                var royalty = pawn.royalty.AllTitlesForReading.Count > 0;
                var veSkills = ModsConfig.IsActive("vanillaexpanded.skills");
                var distance = guilty ? 174 : 140;
                distance += royalty ? 34 : 0;
                distance += veSkills ? 41 : 0;
                var rect7 = new Rect(CharacterCardUtility.BasePawnCardSize.x - distance, 14f, 30f, 30f);
                TooltipHandler.TipRegion(rect7,
                    new TipSignal(
                        pawn.IsGhoul() ? "ROMV_GhoulSheet".Translate() : "ROMV_VampireSheet".Translate()));
                if (Widgets.ButtonImage(rect7,
                        pawn.IsGhoul() ? TexButton.ROMV_GhoulIcon : TexButton.ROMV_VampireIcon))
                    isSwitched = true;
            }
        }
    }

    // RimWorld.ITab_Pawn_Character
    public static bool Vamp_FillTab(ITab_Pawn_Character __instance)
    {
        var p = (Pawn)AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout")
            .Invoke(__instance, null);
        if (p.IsVampire(true) || p.IsGhoul())
        {
            var rect = new Rect(17f, 17f, CharacterCardUtility.BasePawnCardSize.x,
                CharacterCardUtility.BasePawnCardSize.y);
            if (isSwitched)
                VampireCardUtility.DrawVampCard(rect, p);
            else
                CharacterCardUtility.DrawCharacterCard(rect, p);
            return false;
        }

        return true;
    }
}