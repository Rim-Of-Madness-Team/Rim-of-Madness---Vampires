using Verse;

namespace Vampire;

public class DisciplineEffect_Possession : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        var hediff = HediffMaker.MakeHediff(VampDefOf.ROMV_PossessionHediff, target);
        hediff.Severity = 1.0f;
        target.health.AddHediff(hediff);
        hediff.TryGetComp<HediffComp_Possession>().ActivateEffect(CasterPawn);
    }
}