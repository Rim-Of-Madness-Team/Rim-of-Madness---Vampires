using Verse;

namespace Vampire;

public class Hediff_AddedPart_Fangs : Hediff_AddedPart
{
    /// <summary>
    ///     Sometimes raiders would arrive with fangs, and yet
    ///     have no vampirism.
    ///     This will fix that issue entirely, and a bit heavy handedly.
    /// </summary>
    public override bool ShouldRemove
    {
        get
        {
            if (Part?.def?.defName == "Jaw")
            {
                if (pawn != null)
                {
                    //Restore the tongue
                    if (pawn?.health?.hediffSet?.GetMissingPartsCommonAncestors()
                            .FirstOrDefault(x => x.Part?.def?.defName == "Tongue") is { } missingTongue)
                        pawn.health.RestorePart(missingTongue.Part);

                    //Readd the fangs and the tongue
                    VampireGen.AddFangsHediff(pawn);
                }

                return true;
            }

            if (pawn.IsVampire(true))
                return false;
            return true;
        }
    }

    /// <summary>
    ///     If raiders show up with fangs, and the fangs are removed by the 'ShouldRemove' check,
    ///     so we should also give them back their jaws.
    ///     8/7/22 -- we should probably give everyone back their jaws
    /// </summary>
    public override void PostRemoved()
    {
        //if (!pawn.IsVampire(true))
        pawn.health.RestorePart(Part, this, false);

        base.PostRemoved();
    }
}