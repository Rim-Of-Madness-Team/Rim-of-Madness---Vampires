using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire;

/// <summary>
///     Vampire shares secrets and knowledge from experience.
/// </summary>
public class InteractionWorker_VampiricLore : InteractionWorker
{
    //What is the XP benefit from this interaciton.
    public const float VAMPIRE_XPPERLEVELDIFF = 10f;

    //Very common interaction
    private const float BaseSelectionWeight = 1f;

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel,
        out LetterDef letterDef, out LookTargets lookTargets)
    {
        base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef,
            out lookTargets);
        var diff = initiator.VampComp().Level - recipient.VampComp().Level;
        var xpToGive = (int)(10 + VAMPIRE_XPPERLEVELDIFF * diff);
        if (recipient.IsGhoul()) xpToGive = (int)(xpToGive * 0.5f);
        recipient.VampComp().XP += xpToGive;
        MoteMaker.ThrowText(recipient.DrawPos, recipient.MapHeld, "XP +" + xpToGive);
    }

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        //We need two individuals that are part of the colony
        if (!initiator.IsColonist || !recipient.IsColonist) return 0f;

        //The initiator must be a vampire.
        if (!initiator.IsVampire(true)) return 0f;
        if (!recipient.IsGhoul() && !recipient.IsVampire(true)) return 0f;

        //If they are sleeping, don't do this.
        if (initiator.jobs.curDriver.asleep) return 0f;
        if (recipient.jobs.curDriver.asleep) return 0f;

        //The recipient must be a lower level vampire or ghoul.
        var xpRecipient = recipient.VampComp().XP;
        var xpInitiator = initiator.VampComp().XP;

        //The initiator must have more XP than the recipient.
        if (xpInitiator < xpRecipient)
            return 0f;

        //If they have a good relationship, increase the chances of the interaction.
        if (initiator.relations.OpinionOf(recipient) > 0) return Rand.Range(0.8f, 1f);
        return 0f;
    }
}