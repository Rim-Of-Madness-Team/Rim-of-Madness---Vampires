using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

// Token: 0x02000414 RID: 1044
public class VampLevelUpLetter : StandardLetter
{
    // Token: 0x1700060A RID: 1546
    // (get) Token: 0x06001F7B RID: 8059 RVA: 0x000C5BA9 File Offset: 0x000C3DA9
    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            foreach (var baseOption in base.Choices) yield return baseOption;

            yield return Option_JumpToLocationAndOpenVampireBioSheet;
        }
    }

    protected DiaOption Option_JumpToLocationAndOpenVampireBioSheet
    {
        get
        {
            var target = lookTargets.TryGetPrimaryTarget();
            var diaOption = new DiaOption("ROMV_OpenCharacterCard".Translate());
            diaOption.action = delegate
            {
                CameraJumper.TryJumpAndSelect(target);
                Find.LetterStack.RemoveLetter(this);
                if (CameraJumper.CanJump(target)) InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Character));
                HarmonyPatches.isSwitched = true;
            };
            diaOption.resolveTree = true;
            if (!CameraJumper.CanJump(target)) diaOption.Disable(null);
            return diaOption;
        }
    }
}