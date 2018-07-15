using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace Vampire
{
    /// <summary>
    /// Vampire shares secrets and knowledge from experience.
    /// </summary>
    public class InteractionWorker_VampiricLore : InteractionWorker
    {

        //What is the XP benefit from this interaciton.
        public const float VAMPIRE_XPPERLEVELDIFF = 10f;

        //Very common interaction
        private const float BaseSelectionWeight = 1f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
            {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef);
            int diff = initiator.VampComp().Level - recipient.VampComp().Level;
            int xpToGive = (int)(10 + (VAMPIRE_XPPERLEVELDIFF * diff));
            if (recipient.IsGhoul()) xpToGive = (int)(xpToGive * 0.5f);
            recipient.VampComp().XP += xpToGive;
            MoteMaker.ThrowText(recipient.DrawPos, recipient.MapHeld, "XP +" + xpToGive);

        }
  
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //We need two individuals that are part of the colony
            if (!initiator.IsColonist || !recipient.IsColonist) return 0f;

            //The initiator must be a vampire.
            if (!initiator.IsVampire()) return 0f;
            if (!recipient.IsGhoul() && !recipient.IsVampire()) return 0f;
            
            //If they are sleeping, don't do this.
            if (initiator.jobs.curDriver.asleep) return 0f;
            if (recipient.jobs.curDriver.asleep) return 0f;
            
            //The recipient must be a lower level vampire or ghoul.
            int xpRecipient = recipient.VampComp().XP;
            int xpInitiator = initiator.VampComp().XP;

            //The initiator must have more XP than the recipient.
            if (xpInitiator < xpRecipient)
                return 0f;

            //If they have a good relationship, increase the chances of the interaction.
            if (initiator.relations.OpinionOf(recipient) > 0) return Rand.Range(0.8f, 1f);
            return 0f;
        }
    }
}
