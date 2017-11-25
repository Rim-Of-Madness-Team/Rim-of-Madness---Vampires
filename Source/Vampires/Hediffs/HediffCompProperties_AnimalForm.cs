using System;
using Verse;

namespace Vampire
{
    public class HediffCompProperties_AnimalForm : HediffCompProperties_Disappears
    {
        public PawnKindDef animalToChangeInto;
        public bool immuneTodamage = false;
        public bool canGiveDamage = true;
        public HediffCompProperties_AnimalForm()
        {
            this.compClass = typeof(HediffComp_AnimalForm);
        }
    }
}
