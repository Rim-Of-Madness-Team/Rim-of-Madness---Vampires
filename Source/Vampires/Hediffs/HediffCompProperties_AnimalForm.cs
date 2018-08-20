using Verse;

namespace Vampire
{
    public class HediffCompProperties_AnimalForm : HediffCompProperties_Disappears
    {
        public TransformationDef animalToChangeInto;
        public bool immuneTodamage = false;
        public bool canGiveDamage = true;
        public HediffCompProperties_AnimalForm()
        {
            compClass = typeof(HediffComp_AnimalForm);
        }
    }
}
