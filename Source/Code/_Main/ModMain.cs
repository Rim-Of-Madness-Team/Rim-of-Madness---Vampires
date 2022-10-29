using UnityEngine;
using Verse;

namespace Vampire;

public class ModMain : Mod
{
    public ModMain(ModContentPack content) : base(content)
    {
    }

    public override string SettingsCategory()
    {
        return "Rim of Madness - Vampires";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (Find.World?.GetComponent<WorldComponent_VampireSettings>() is { } modSettings)
        {
            var fColWidth = inRect.width * 0.25f;
            var fRowHeight = inRect.height * 0.08f;
            var rowCount = 1;
            var colCount = 1;

            //World setting dialog box
            var rDialogButton = new Rect(inRect.x, inRect.y, fColWidth * colCount, fRowHeight);
            if (Widgets.ButtonText(rDialogButton, "ROMV_VampireSettingsButton".Translate()))
                Find.WindowStack.Add(new Dialog_VampireSetup());
            rowCount++;

            // Allow vampire AI toggle box
            var rAIToggle = new Rect(rDialogButton.x, rDialogButton.y + fRowHeight * rowCount, fColWidth * colCount,
                fRowHeight);
            var rAIToggleLeft = rAIToggle.LeftPart(0.666f).Rounded();
            var rAIToggleRight = rAIToggle.RightPart(0.333f).Rounded();
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rAIToggleLeft, "ROMV_VampireAIToggle".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.CheckboxLabeled(rAIToggleRight, "", ref VampireSettings.Get.aiToggle);
            rowCount++;

            // Daylight damage
            var rDamageToggle = new Rect(rDialogButton.x, rDialogButton.y + fRowHeight * rowCount, fColWidth * colCount,
                fRowHeight);
            var rDamageToggleLeft = rDamageToggle.LeftPart(0.666f).Rounded();
            var rDamageToggleRight = rDamageToggle.RightPart(0.333f).Rounded();
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rDamageToggleLeft, "ROMV_VampireSunlightDamageToggle".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.CheckboxLabeled(rDamageToggleRight, "", ref VampireSettings.Get.damageToggle);
            TooltipHandler.TipRegion(rDamageToggle, () => "ROMV_VampireSunlightDamageToggleTooltip".Translate(),
                2807861);

            rowCount++;

            // Forced vampire slumber during daylight
            var rSlumberToggle = new Rect(rDialogButton.x, rDialogButton.y + fRowHeight * rowCount,
                fColWidth * colCount, fRowHeight);
            var rSlumberToggleLeft = rSlumberToggle.LeftPart(0.666f).Rounded();
            var rSlumberToggleRight = rSlumberToggle.RightPart(0.333f).Rounded();
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rSlumberToggleLeft, "ROMV_VampireSlumberToggle".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.CheckboxLabeled(rSlumberToggleRight, "", ref VampireSettings.Get.slumberToggle);
            TooltipHandler.TipRegion(rSlumberToggle, () => "ROMV_VampireSlumberToggleTooltip".Translate(), 2807861);

            rowCount++;
        }
        else
        {
            Widgets.Label(inRect, "ROMV_VampireSettingsUnavailable".Translate());
        }
    }
}