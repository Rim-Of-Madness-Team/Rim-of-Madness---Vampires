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
            }
            else
            {
                Widgets.Label(inRect, "ROMV_VampireSettingsUnavailable".Translate());
            }
        }
    }
}
