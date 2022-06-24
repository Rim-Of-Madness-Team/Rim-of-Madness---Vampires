﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire
{
    public class DisciplineEffect_Hunger : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            CasterPawn.Drawer.Notify_DebugAffected();
            MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, AbilityUser.StringsToTranslate.AU_CastSuccess);
            bool coolantUser = VampireUtility.IsCoolantUser(CasterPawn);
            int num = GenRadial.NumCellsInRadius(3.9f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 curCell = CasterPawn.PositionHeld + GenRadial.RadialPattern[i];
                if (curCell.GetThingList(CasterPawn.MapHeld) is List<Thing> things && !things.NullOrEmpty())
                {
                    List<Thing> temp = new List<Thing>(things);
                    foreach (Thing t in temp)
                    { 
                        if (coolantUser && t.def.defName == "ROMV_Filth_Coolant" )
                        {
                            CasterPawn.BloodNeed().AdjustBlood(1);
                            t.Destroy();
                        }
                        if (t.def.defName == "Filth_Blood")
                        {
                            CasterPawn.BloodNeed().AdjustBlood(1);
                            t.Destroy();
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
