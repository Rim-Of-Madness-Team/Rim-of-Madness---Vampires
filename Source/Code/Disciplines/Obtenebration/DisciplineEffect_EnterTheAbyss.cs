using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_EnterTheAbyss : Verb_UseAbility
{
    public void Effect()
    {
        CasterPawn.Drawer.Notify_DebugAffected();
        if (TargetsAoE.FirstOrDefault(x => x is { } y && y.Cell != default) is { } t)
        {
            var destination = t.Cell;

            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastSuccess);
            if (destination.IsValid)
            {
                CasterPawn.Position = destination;
                CasterPawn.stances.stunner.StunFor(new IntRange(5, 10).RandomInRange, CasterPawn, false);
                CasterPawn.Notify_Teleported();

                //Cheap solution...
                Ability.CooldownTicksLeft = Ability.MaxCastingTicks;
                CasterPawn.BloodNeed().AdjustBlood(-3);
                return;
            }

            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastFailure);
        }
    }


    public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
    {
        return true;
    }

    public override void PostCastShot(bool inResult, out bool outResult)
    {
        if (inResult)
        {
            Effect();
            outResult = true;
        }

        outResult = inResult;
    }
}