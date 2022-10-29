using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Vampire;

public class Building_CoffinSealed : Building_Coffin
{
    private Pawn sealedVampire;

    public override void PostMake()
    {
        base.PostMake();
        LongEventHandler.QueueLongEvent(delegate
        {
            sealedVampire =
                VampireGen.GenerateVampire(Rand.Range(4, 7), VampireUtility.RandBloodline, null,
                    Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROMV_LegendaryVampires")));
            GenSpawn.Spawn(sealedVampire, PositionHeld, MapHeld);
            sealedVampire.DeSpawn();
            innerContainer.TryAdd(sealedVampire);
        }, "ROMV_GeneratingAncientVampire", false, exception => { });
    }

    public override void Open()
    {
        base.Open();
        sealedVampire.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk,
            "Awakened");
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref sealedVampire, "sealedVampire");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var bloodAwaken = DefDatabase<VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricAwaken");
        foreach (var g in base.GetGizmos()
                     .Where(x => !(x is Command_Action y && y.defaultLabel == bloodAwaken.label)))
            yield return g;

        var AbilityUser = sealedVampire;
        if (!AbilityUser?.Dead ?? false)
            yield return new Command_Action
            {
                defaultLabel = bloodAwaken.label,
                defaultDesc = bloodAwaken.GetDescription(),
                icon = bloodAwaken.uiIcon,
                action = delegate
                {
                    AbilityUser.BloodNeed().AdjustBlood(-1);
                    EjectContents();
                    sealedVampire.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk,
                        bloodAwaken.label);
                    if (def == VampDefOf.ROMV_HideyHole)
                        Destroy();
                }
            };
    }
}