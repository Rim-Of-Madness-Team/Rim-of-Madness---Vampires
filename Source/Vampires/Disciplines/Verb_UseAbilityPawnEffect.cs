using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class Verb_UseAbilityPawnEffect : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect(Pawn target)
        {
            if (this.Ability.Def.MainVerb.isViolent && target.Faction != this.CasterPawn.Faction)
            {
                target.mindState.Notify_DamageTaken(new DamageInfo(DamageDefOf.Cut, -1, -1, this.CasterPawn));
            }
            target.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(target.DrawPos, target.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult &&
                this.TargetsAoE[0].Thing is Pawn p)
            {
                Effect(p);
                outResult = true;
                return;
            }
            outResult = false;
            return;
        }
    }
}
