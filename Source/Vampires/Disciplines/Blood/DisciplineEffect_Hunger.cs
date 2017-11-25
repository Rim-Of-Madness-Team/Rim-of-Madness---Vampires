using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_Hunger : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(this.CasterPawn.DrawPos, this.CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess, -1f);
            int num = GenRadial.NumCellsInRadius(3.9f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 curCell = this.CasterPawn.PositionHeld + GenRadial.RadialPattern[i];
                if (curCell.GetThingList(this.CasterPawn.MapHeld) is List<Thing> things && !things.NullOrEmpty())
                {
                    List<Thing> temp = new List<Thing>(things);
                    foreach (Thing t in temp)
                    {
                        if (t.def.defName == "FilthBlood")
                        {
                            this.CasterPawn.BloodNeed().AdjustBlood(1);
                            t.Destroy(DestroyMode.Vanish);
                        }
                    }
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
