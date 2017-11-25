using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{

    public class CompProperties_BloodItem : CompProperties
    {
        public BloodType bloodType = BloodType.LowBlood;
        public int bloodPoints = 1;        
        public CompProperties_BloodItem() => this.compClass = typeof(CompBloodItem);
    }
}
