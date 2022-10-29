using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class Verb_UseAbilityPawnEffect : Verb_UseAbility
{
    public virtual void Effect(Pawn target)
    {
        if (Ability.Def.MainVerb.isViolent && target.Faction != CasterPawn.Faction)
            target.mindState.Notify_DamageTaken(new DamageInfo(DamageDefOf.Cut, -1, 0f, -1, CasterPawn));
        target.Drawer.Notify_DebugAffected();
        MoteMaker.ThrowText(target.DrawPos, target.Map, StringsToTranslate.AU_CastSuccess);
    }

    public override void PostCastShot(bool inResult, out bool outResult)
    {
        if (inResult &&
            TargetsAoE[0].Thing is Pawn p)
        {
            Effect(p);
            outResult = true;
            return;
        }

        outResult = false;
    }
}