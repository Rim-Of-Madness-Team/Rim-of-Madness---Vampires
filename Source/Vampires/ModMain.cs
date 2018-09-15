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
                if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, inRect.width * 0.25f, inRect.height * 0.15f), "ROMV_VampireSettingsButton".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_VampireSetup());
                }
            }
            else
            {
                Widgets.Label(inRect, "ROMV_VampireSettingsUnavailable".Translate());
            }
        }
    }
}
