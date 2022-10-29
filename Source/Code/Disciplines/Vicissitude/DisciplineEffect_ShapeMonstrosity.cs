using Verse;

namespace Vampire;

public class DisciplineEffect_ShapeMonstrosity : Verb_UseAbilityPawnEffect
{
    public override void Effect(Pawn target)
    {
        base.Effect(target);
        if (JecsTools.GrappleUtility.TryGrapple(CasterPawn, target))
        {
            var curLoc = target.PositionHeld;
            var curMap = target.MapHeld;
            var tempName = target.Name;
            target.DeSpawn();
            var p = PawnGenerator.GeneratePawn(VampDefOf.ROMV_MonstrosityA, CasterPawn.Faction);
            GenSpawn.Spawn(p, curLoc, curMap);
            p.Name = tempName;
        }
    }
}