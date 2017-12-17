using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_ReadMind : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess);
            HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(VampDefOf.ROMV_MindReadingHediff, CasterPawn);
            if (hediff.TryGetComp<HediffComp_ReadMind>() is HediffComp_ReadMind rm)
            {
                rm.MindBeingRead = target;
            }
            hediff.Severity = 1.0f;
            CasterPawn.health.AddHediff(hediff, null, null);
        }
    }
}
