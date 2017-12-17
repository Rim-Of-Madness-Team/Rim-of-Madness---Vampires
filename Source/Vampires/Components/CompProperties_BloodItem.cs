using Verse;

namespace Vampire
{

    public class CompProperties_BloodItem : CompProperties
    {
        public BloodType bloodType = BloodType.LowBlood;
        public int bloodPoints = 1;        
        public CompProperties_BloodItem() => compClass = typeof(CompBloodItem);
    }
}
