using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public class CompBloodItem : ThingComp
    {
        public CompProperties_BloodItem Props => this.props as CompProperties_BloodItem;
    }
}
