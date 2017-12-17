using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Vicissitude
{
    public class DisciplineEffect_ShapeMonstrosity : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            if (JecsTools.GrappleUtility.TryGrapple(CasterPawn, target))
            {
                IntVec3 curLoc = target.PositionHeld;
                Map curMap = target.MapHeld;
                Name tempName = target.Name;
                target.DeSpawn();
                Pawn p = PawnGenerator.GeneratePawn(VampDefOf.ROMV_MonstrosityA, CasterPawn.Faction);
                GenSpawn.Spawn(p, curLoc, curMap);
                p.Name = tempName;
            }
        }
    }
}
