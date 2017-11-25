using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Vampire
{
    public class CompProperties_VampBed : CompProperties
    {
        public ThingDef bedDef;
        public bool hideOriginalThing = true;

        public CompProperties_VampBed()
        {
            this.compClass = typeof(CompVampBed);
        }
    }
}
