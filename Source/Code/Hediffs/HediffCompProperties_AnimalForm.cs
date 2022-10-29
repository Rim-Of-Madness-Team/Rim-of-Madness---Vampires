using Verse;

namespace Vampire;

public class HediffCompProperties_AnimalForm : HediffCompProperties_Disappears
{
    public TransformationDef animalToChangeInto;
    public bool canGiveDamage = true;
    public bool immuneTodamage = false;

    public HediffCompProperties_AnimalForm()
    {
        compClass = typeof(HediffComp_AnimalForm);
    }
}