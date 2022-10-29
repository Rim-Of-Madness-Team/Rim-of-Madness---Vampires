using Verse;

namespace Vampire;

public class CompProperties_BloodItem : CompProperties
{
    public int bloodPoints = 1;
    public BloodType bloodType = BloodType.LowBlood;

    public CompProperties_BloodItem()
    {
        compClass = typeof(CompBloodItem);
    }
}