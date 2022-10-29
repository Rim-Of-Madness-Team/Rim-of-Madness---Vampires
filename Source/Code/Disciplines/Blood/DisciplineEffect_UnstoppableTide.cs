using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_UnstoppableTide : Verb_UseAbility
{
    public virtual void Effect()
    {
        //target.Drawer.Notify_DebugAffected();
        MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastSuccess);
        if (TargetsAoE[0] is { } t && t.Cell != default)
        {
            var p = (PawnTemporary)PawnGenerator.GeneratePawn(VampDefOf.ROMV_BloodMistKind, Faction.OfPlayer);
            p.Master = CasterPawn;
            GenSpawn.Spawn(p, t.Cell, CasterPawn.Map);
        }
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