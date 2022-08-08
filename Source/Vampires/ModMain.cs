using UnityEngine;
using Verse;

namespace Vampire
{

    public class ModMain : Mod
    {
        public ModMain(ModContentPack content) : base(content)
        {
        }

        public override string SettingsCategory() => "Rim of Madness - Vampires";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (Find.World?.GetComponent<WorldComponent_VampireSettings>() is WorldComponent_VampireSettings modSettings)
            {
                float fColWidth = inRect.width * 0.25f;
                float fRowHeight = inRect.height * 0.08f;
                int rowCount = 1;
                int colCount = 1;

                //World setting dialog box
                Rect rDialogButton = new Rect(inRect.x, inRect.y, fColWidth * colCount, fRowHeight);
                if (Widgets.ButtonText(rDialogButton, "ROMV_VampireSettingsButton".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_VampireSetup());
                }
                rowCount++;

                // Allow vampire AI toggle box
                Rect rAIToggle = new Rect(rDialogButton.x, rDialogButton.y + (fRowHeight * rowCount), fColWidth * colCount, fRowHeight);
                Rect rAIToggleLeft = rAIToggle.LeftPart(0.666f).Rounded();
                Rect rAIToggleRight = rAIToggle.RightPart(0.333f).Rounded();
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rAIToggleLeft, "ROMV_VampireAIToggle".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.CheckboxLabeled(rAIToggleRight, "", ref VampireSettings.Get.aiToggle);
                rowCount++;

                // Forced vampire slumber during daylight
                Rect rSlumberToggle = new Rect(rDialogButton.x, rDialogButton.y + (fRowHeight * rowCount), fColWidth * colCount, fRowHeight);
                Rect rSlumberToggleLeft = rSlumberToggle.LeftPart(0.666f).Rounded();
                Rect rSlumberToggleRight = rSlumberToggle.RightPart(0.333f).Rounded();
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rSlumberToggleLeft, "ROMV_VampireSlumberToggle".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.CheckboxLabeled(rSlumberToggleRight, "", ref VampireSettings.Get.slumberToggle);
                rowCount++;

                // Daylight damage
                Rect rDamageToggle = new Rect(rDialogButton.x, rDialogButton.y + (fRowHeight * rowCount), fColWidth * colCount, fRowHeight);
                Rect rDamageToggleLeft = rDamageToggle.LeftPart(0.666f).Rounded();
                Rect rDamageToggleRight = rDamageToggle.RightPart(0.333f).Rounded();
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rDamageToggleLeft, "ROMV_VampireSunlightDamageToggle".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.CheckboxLabeled(rDamageToggleRight, "", ref VampireSettings.Get.damageToggle);
                rowCount++;
            }
            else
            {
                Widgets.Label(inRect, "ROMV_VampireSettingsUnavailable".Translate());
            }
        }
    }
}
