using Verse;

namespace Vampire;

public class CompProperties_VampBed : CompProperties
{
    public ThingDef bedDef;
    public bool hideOriginalThing = true;

    public CompProperties_VampBed()
    {
        compClass = typeof(CompVampBed);
    }
}