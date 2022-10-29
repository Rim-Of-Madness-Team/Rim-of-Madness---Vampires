using System.Collections.Generic;
using AbilityUser;
using RimWorld;
using Verse;

namespace Vampire;

public class DisciplineEffect_Hunger : Verb_UseAbility
{
    public virtual void Effect()
    {
        CasterPawn.Drawer.Notify_DebugAffected();
        MoteMaker.ThrowText(CasterPawn.DrawPos, CasterPawn.Map, StringsToTranslate.AU_CastSuccess);
        var coolantUser = CasterPawn.IsCoolantUser();
        var num = GenRadial.NumCellsInRadius(3.9f);
        for (var i = 0; i < num; i++)
        {
            var curCell = CasterPawn.PositionHeld + GenRadial.RadialPattern[i];
            if (curCell.GetThingList(CasterPawn.MapHeld) is { } things && !things.NullOrEmpty())
            {
                var temp = new List<Thing>(things);
                foreach (var t in temp)
                {
                    if (coolantUser && t.def.defName == "ROMV_Filth_Coolant")
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