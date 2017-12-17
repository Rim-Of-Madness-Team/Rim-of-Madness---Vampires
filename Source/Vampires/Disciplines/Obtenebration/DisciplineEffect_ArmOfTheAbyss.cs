using RimWorld;
using Verse;

namespace Vampire
{
    //ROMV_AbyssalArmKind
    public class DisciplineEffect_ArmOfTheAbyss : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            //target.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess);
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                PawnTemporary p = (PawnTemporary)PawnGenerator.GeneratePawn(VampDefOf.ROMV_AbyssalArmKind, Faction.OfPlayer);
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
}
