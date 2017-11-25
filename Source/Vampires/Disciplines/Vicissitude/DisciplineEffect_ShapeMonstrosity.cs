using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_ShapeMonstrosity : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            if (!JecsTools.GrappleUtility.CanGrapple(this.CasterPawn, target))
            {
                IntVec3 curLoc = target.PositionHeld;
                Map curMap = target.MapHeld;
                Name tempName = target.Name;
                target.DeSpawn();
                Pawn p = PawnGenerator.GeneratePawn(VampDefOf.ROMV_MonstrosityA, this.CasterPawn.Faction);
                GenSpawn.Spawn(p, curLoc, curMap);
                p.Name = tempName;
            }
        }
    }
}
