using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Vampire
{
    public partial class HarmonyPatches
    {
        public static void HarmonyPatches_UI(Harmony harmony)
        {
            // UI
            /////////////////////////////////////////////////////////////////////////////////////
            //Adds vampire skill sheet button to CharacterCard
            //Log.Message("30");
            harmony.Patch(
                AccessTools.Method(typeof(CharacterCardUtility), "DrawCharacterCard",
                    new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_DrawCharacterCard)));
            //Log.Message("31");
            //Fills the character card with a vampire skill sheet
            harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Character), "FillTab"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_FillTab)), null);
            //Log.Message("32");

            harmony.Patch(AccessTools.Method(typeof(Verb_Shoot), "TryCastShot"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Vamp_TryCastShot)), null);
            //Log.Message("33");


        }


        // RimWorld.CharacterCardUtility
        public static bool isSwitched = false;
        public static void Vamp_DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback,
            Rect creationRect = default(Rect))
        {
            if (pawn.IsVampire(true) || pawn.IsGhoul())
            {
                bool flag = randomizeCallback != null;
                if (!flag && pawn.IsColonist && !pawn.health.Dead)
                {
                    bool guilty = pawn.guilt.IsGuilty;
                    bool royalty = pawn.royalty.AllTitlesForReading.Count > 0;
                    int distance = (guilty) ? 174 : 140;
                    distance += (royalty) ? 34 : 0;
                    Rect rect7 = new Rect(CharacterCardUtility.BasePawnCardSize.x - distance, 14f, 30f, 30f);
                    TooltipHandler.TipRegion(rect7,
                        new TipSignal(
                            (pawn.IsGhoul()) ? "ROMV_GhoulSheet".Translate() : "ROMV_VampireSheet".Translate()));
                    if (Widgets.ButtonImage(rect7,
                        (pawn.IsGhoul()) ? TexButton.ROMV_GhoulIcon : TexButton.ROMV_VampireIcon))
                    {
                        isSwitched = true;
                    }
                }
            }
        }

        // RimWorld.ITab_Pawn_Character
        public static bool Vamp_FillTab(ITab_Pawn_Character __instance)
        {
            Pawn p = (Pawn)AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout")
                .Invoke(__instance, null);
            if (p.IsVampire(true) || p.IsGhoul())
            {
                Rect rect = new Rect(17f, 17f, CharacterCardUtility.BasePawnCardSize.x,
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
}
