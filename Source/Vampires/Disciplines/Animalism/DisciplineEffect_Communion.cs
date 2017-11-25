using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_Communion : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            //target.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                for (int i = 1; i <= 3; i++)
                {
                    PawnTemporary p = (PawnTemporary)PawnGenerator.GeneratePawn(VampDefOf.ROMV_BatSpectralKind, Faction.OfPlayer);
                    p.Master = this.CasterPawn;
                    GenSpawn.Spawn(p, t.Cell, this.CasterPawn.Map);
                }
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
